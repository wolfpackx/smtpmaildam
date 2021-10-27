using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using MimeKit;
using SmtpMailDam.Common.Enums;
using SmtpMailDam.Common.Interfaces;
using SmtpMailDam.Common.Models;
using SmtpMailDam.Common.Utillity;
using SmtpMailDam.Website.Models;

namespace SmtpMailDam.Website.Controllers
{
    public class MailController : Controller
    {
        private ILogger<MailController> logger;
        private IMailboxRepository mailboxRepository;
        private IMailRepository mailRepository;

        public MailController(ILogger<MailController> logger, IMailboxRepository mailboxRepository, IMailRepository mailRepository)
        {
            this.logger = logger;
            this.mailRepository = mailRepository;
            this.mailboxRepository = mailboxRepository;
        }

        public ActionResult Details(Guid id)
        {
            if (Guid.Empty == id)
            {
                return this.BadRequest();
            }

            var mail = this.mailRepository.Get(id);

            if (mail == null)
            {
                return this.NotFound();
            }

            ViewData["Title"] = $"{mail.Subject}";

            if (mail.Status == (int)MailStatus.Unread)
            {
                mail.Status = (int)MailStatus.Read;
                this.mailRepository.Update(mail);
            }

            return View(MapMailToMailViewModel(mail, true));
        }

        public ActionResult Delete(Guid id)
        {
            if (Guid.Empty == id)
            {
                return this.BadRequest();
            }

            var mail = this.mailRepository.Get(id);

            if (mail == null)
            {
                return this.NotFound();
            }

            ViewData["Title"] = $"Delete mail {mail.Subject}";

            return View(MapMailToMailViewModel(mail, false));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id, IFormCollection collection)
        {
            if (ModelState.IsValid)
            {
                if (Guid.Empty == id)
                {
                    return this.BadRequest();
                }

                var mail = this.mailRepository.Get(id);

                if (mail == null)
                {
                    return this.NotFound();
                }

                var mailboxId = mail.MailboxId;

                this.mailRepository.Delete(mail);

                return RedirectToAction(nameof(MailboxController.Details), "Mailbox", new { id = mailboxId });
            }

            return Delete(id);
        }

        public ActionResult GetMailMessageEml(Guid id)
        {
            if (Guid.Empty == id)
            {
                return this.BadRequest();
            }

            var mail = this.mailRepository.Get(id);

            if (mail == null)
            {
                return this.NotFound();
            }

            using (var mailStream = new MemoryStream(mail.RawEmail))
            {
                MimeMessage mimeMessage = MimeMessage.Load(mailStream);

                using (var ms = new MemoryStream())
                {
                    mimeMessage.WriteTo(ms);
                    ms.Position = 0;

                    return new FileContentResult(ms.ToArray(), new MediaTypeHeaderValue("text/plain"))
                    {
                        FileDownloadName = $"mail-{DateTime.Now.ToString("yyyyMMddHHmm")}.eml"
                    };
                }
            }
        }

        public ActionResult GetAttachment([FromRoute] Guid id, [FromQuery] string attachmentid)
        {
            if (Guid.Empty == id)
            {
                return this.BadRequest();
            }

            var mail = this.mailRepository.Get(id);

            if (mail == null)
            {
                return this.NotFound();
            }

            using var ms = new MemoryStream(mail.RawEmail);
            ms.Position = 0;

            var message = MimeMessage.Load(ms);

            var visitor = new HtmlPreviewVisitor();

            message.Accept(visitor);

            var attachment = message.Attachments.FirstOrDefault(a => a.IsAttachment && (a.ContentId != null? a.ContentId : a.ContentDisposition.FileName?.GetHashCode().ToString()) == attachmentid);

            if (attachment == null)
            {
                return NotFound();
            }

            var attachmentStream = ParseAttachment(attachment);

            attachmentStream.Position = 0;
            var result = new FileStreamResult(attachmentStream, attachment.ContentType.MimeType);
            result.FileDownloadName = attachment.ContentDisposition.FileName;

            return result;
        }

        [HttpPost]
        public ActionResult SendToImap(Guid id)
        {
            if (Guid.Empty == id)
            {
                return this.BadRequest();
            }

            var mail = this.mailRepository.Get(id);

            if (mail == null)
            {
                return this.NotFound();
            }

            var mailbox = this.mailboxRepository.Get(mail.MailboxId, false);

            if (mailbox == null || !mailbox.ImapEnabled)
            {
                return this.NotFound();
            }

            using var ms = new MemoryStream(mail.RawEmail);
            ms.Position = 0;

            var message = MimeMessage.Load(ms);

            Imap.SendMessageToImap(mailbox, message);

            return Json(new { Message = "Ok" });
        }

        private MailViewModel MapMailToMailViewModel(Mail mail, bool transformMimeMessage)
        {
            if (mail == null)
            {
                return new MailViewModel();
            }

            string rawEmail = string.Empty;
            string renderedEmail = string.Empty;

            IEnumerable<AttachmentViewModel> attachements = new List<AttachmentViewModel>();

            if (transformMimeMessage && mail.RawEmail != null && mail.RawEmail.Length > 0)
            {
                using var ms = new MemoryStream(mail.RawEmail);
                ms.Position = 0;

                var message = MimeMessage.Load(ms);

                var visitor = new HtmlPreviewVisitor();

                message.Accept(visitor);

                int attachment = 1;

                attachements = message.Attachments.Where(a => a.IsAttachment).Select(a => 
                    new AttachmentViewModel 
                    {
                        Id = a.ContentId != null ? a.ContentId : a.ContentDisposition.FileName?.GetHashCode().ToString(),
                        Filename = !string.IsNullOrWhiteSpace(a.ContentDisposition?.FileName) ? a.ContentDisposition?.FileName : $"Attachement {attachment++}",
                        Size = a.ContentDisposition.Size.HasValue ? a.ContentDisposition.Size.Value : ParseAttachment(a).Length,
                        MimeType = a.ContentType.MimeType
                    }).ToList();

                renderedEmail = visitor.HtmlBody;

                using (var msEml = new MemoryStream())
                {
                    message.WriteTo(msEml);
                    msEml.Position = 0;

                    StreamReader reader = new StreamReader(msEml);
                    rawEmail = reader.ReadToEnd();
                }
            }

            return new MailViewModel
            {
                HtmlBody = !string.IsNullOrWhiteSpace(mail.HtmlBody) ? mail.HtmlBody.Trim() : string.Empty,
                TextBody = !string.IsNullOrWhiteSpace(mail.TextBody) ? mail.TextBody.Trim() : string.Empty,
                RenderedHtml = PrettifyHtml(renderedEmail.Trim()),
                RawEmail = rawEmail.Trim(),
                From = mail.From,
                MailId = mail.MailId,
                ReceiveDate = mail.ReceiveDate,
                Status = mail.Status,
                Subject = mail.Subject,
                To = mail.To,
                MailboxId = mail.MailboxId,
                Attachements = attachements
            };
        }

        private string PrettifyHtml(string html)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(html);

                using (StringWriter sw = new StringWriter())
                {
                    doc.Save(sw);
                    return sw.GetStringBuilder().ToString();
                }
            }
            catch
            {
                // Invalid xml
                return html;
            }
        }

        private MemoryStream ParseAttachment(MimeEntity attachment)
        {
            var attachmentStream = new MemoryStream();

            if (attachment is MessagePart)
            {
                var rfc822 = (MessagePart)attachment;

                rfc822.Message.WriteTo(attachmentStream);
            }
            else
            {
                var part = (MimePart)attachment;

                part.Content.DecodeTo(attachmentStream);
            }

            return attachmentStream;
        }
    }
}

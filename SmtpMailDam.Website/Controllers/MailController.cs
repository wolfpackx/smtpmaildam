using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            var mail = this.mailRepository.Get(id);

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
            var mail = this.mailRepository.Get(id);

            ViewData["Title"] = $"Delete mail {mail.Subject}";

            return View(MapMailToMailViewModel(mail, false));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id, IFormCollection collection)
        {
            if (ModelState.IsValid)
            {
                var mail = this.mailRepository.Get(id);

                var mailboxId = mail.MailboxId;

                this.mailRepository.Delete(mail);

                return RedirectToAction(nameof(MailboxController.Details), "Mailbox", new { id = mailboxId });
            }

            return Delete(id);
        }

        public ActionResult GetMailMessageEml(Guid id)
        {
            var mail = this.mailRepository.Get(id);

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

        private MailViewModel MapMailToMailViewModel(Mail mail, bool transformMimeMessage)
        {
            if (mail == null)
            {
                return new MailViewModel();
            }

            string rawEmail = string.Empty;
            string renderedEmail = string.Empty;

            if (transformMimeMessage && mail.RawEmail != null && mail.RawEmail.Length > 0)
            {
                using var ms = new MemoryStream(mail.RawEmail);
                ms.Position = 0;

                var message = MimeMessage.Load(ms);

                var visitor = new HtmlPreviewVisitor();

                message.Accept(visitor);

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
                RenderedHtml = renderedEmail.Trim(),
                RawEmail = rawEmail.Trim(),
                From = mail.From,
                MailId = mail.MailId,
                ReceiveDate = mail.ReceiveDate,
                Status = mail.Status,
                Subject = mail.Subject,
                To = mail.To,
                MailboxId = mail.MailboxId
            };
        }
    }
}

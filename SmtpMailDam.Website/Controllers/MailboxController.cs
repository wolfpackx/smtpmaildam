﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmtpMailDam.Common.Interfaces;
using SmtpMailDam.Common.Models;
using SmtpMailDam.Website.Models;
using SmtpMailDam.Common.Utillity;
using MimeKit;
using Microsoft.Net.Http.Headers;

namespace SmtpMailDam.Website.Controllers
{
    public class MailboxController : Controller
    {
        private ILogger<MailboxController> logger;
        private IMailboxRepository mailboxRepository;
        private IMailRepository mailRepository;

        public MailboxController(ILogger<MailboxController> logger, IMailboxRepository mailboxRepository, IMailRepository mailRepository)
        {
            this.logger = logger;
            this.mailRepository = mailRepository;
            this.mailboxRepository = mailboxRepository;
        }

        // GET: MailboxController
        public ActionResult Index()
        {
            var mailboxes = this.mailboxRepository.GetAll();

            ViewData["Title"] = $"Mailboxes";

            return View(mailboxes.Select(m => this.MapMailboxToMailboxViewModel(m)).ToList());
        }

        // GET: MailboxController/Details/5
        public ActionResult Details(Guid id)
        {
            var mailbox = this.mailboxRepository.Get(id, true);

            ViewData["Title"] = $"Mailbox {mailbox.Name}";

            return View(this.MapMailboxToMailboxViewModel(mailbox));
        }

        // GET: MailboxController/Create
        public ActionResult Create()
        {
            ViewData["Title"] = $"Create mailbox";
            return View();
        }

        // POST: MailboxController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Name")] MailboxViewModel mailbox)
        {
            if (ModelState.IsValid)
            {
                mailbox.MailboxId = Guid.NewGuid();
                mailbox.Username = Guid.NewGuid().ToString().Replace("-", string.Empty);
                mailbox.Password = Guid.NewGuid().ToString().Replace("-", string.Empty);
                this.mailboxRepository.Create(this.MapMailboxViewModelToMailbox(mailbox));
                return RedirectToAction(nameof(Index));
            }

            return View(mailbox);
        }

        // GET: MailboxController/Edit/5
        public ActionResult Edit(Guid id)
        {
            var mailbox = this.MapMailboxToMailboxViewModel(this.mailboxRepository.Get(id, false));

            ViewData["Title"] = $"Edit mailbox {mailbox.Name}";

            return View(mailbox);
        }

        // POST: MailboxController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Guid id, [Bind("MailboxId, Name")] MailboxViewModel mailbox)
        {
            if (id != mailbox.MailboxId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                this.mailboxRepository.Update(this.MapMailboxViewModelToMailbox(mailbox));
                return RedirectToAction(nameof(Index));
            }

            return View(mailbox);
        }

        // GET: MailboxController/Delete/5
        public ActionResult Delete(Guid id)
        {
            var mailbox = this.MapMailboxToMailboxViewModel(this.mailboxRepository.Get(id, false));

            ViewData["Title"] = $"Delete mailbox {mailbox.Name}";

            return View(mailbox);
        }

        // POST: MailboxController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id, IFormCollection collection)
        {
            if (ModelState.IsValid)
            {
                var mailbox = new Mailbox
                {
                    MailboxId = id
                };
                this.mailboxRepository.Delete(mailbox);

                return RedirectToAction(nameof(Index));
            }

            return Delete(id);
        }

        public ActionResult ClearMails(Guid id)
        {
            this.mailboxRepository.ClearMails(id);

            return RedirectToAction(nameof(Details), new { id = id });
        }

        public ActionResult Refresh(Guid id)
        {
            return RedirectToAction(nameof(Details), new { id = id });
        }

        private MailboxViewModel MapMailboxToMailboxViewModel(Mailbox mailbox)
        {
            if (mailbox == null)
            {
                return new MailboxViewModel();
            }

            return new MailboxViewModel
            {
                MailboxId = mailbox.MailboxId,
                Name = mailbox.Name,
                Username = mailbox.Username,
                Password = mailbox.Password,
                Mails = mailbox.Mails != null ? mailbox.Mails.Select(m => this.MapMailToMailViewModel(m, false)).ToList() : new List<MailViewModel>()
            };
        }

        private MailViewModel MapMailToMailViewModel(Mail mail, bool transformMimeMessage)
        {
            if (mail == null)
            {
                return new MailViewModel();
            }

            string rawEmail = string.Empty;
            string renderedEmail = string.Empty;

            if (transformMimeMessage && mail.RawEmail.Length > 0)
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
                HtmlBody = mail.HtmlBody,
                TextBody = mail.TextBody,
                RenderedHtml = renderedEmail,
                RawEmail = rawEmail,
                From = mail.From,
                MailId = mail.MailId,
                ReceiveDate = mail.ReceiveDate,
                Status = mail.Status,
                Subject = mail.Subject,
                To = mail.To,
                MailboxId = mail.MailboxId
            };
        }

        private Mailbox MapMailboxViewModelToMailbox(MailboxViewModel mailboxViewModel)
        {
            if (mailboxViewModel == null)
            {
                return new Mailbox();
            }

            return new Mailbox
            {
                MailboxId = mailboxViewModel.MailboxId,
                Name = mailboxViewModel.Name,
                Username = mailboxViewModel.Username,
                Password = mailboxViewModel.Password
            };
        }
    }
}
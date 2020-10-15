using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmtpMailDam.Common.Data;
using SmtpMailDam.Common.Interfaces;
using SmtpMailDam.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmtpMailDam.Common.Repositories
{
    public class MailboxRepository : IMailboxRepository
    {
        private ApplicationDbContext context;
        private ILogger<MailboxRepository> logger;

        public MailboxRepository(ApplicationDbContext context, ILogger<MailboxRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public void ClearMails(Guid mailboxId)
        {
            var mailbox = this.Get(mailboxId, true);

            this.context.Mail.RemoveRange(mailbox.Mails);

            this.context.SaveChanges();
        }

        public void Create(Mailbox mailbox)
        {
            this.context.Mailbox.Add(mailbox);
            this.context.SaveChanges();
        }

        public void Delete(Mailbox mailbox)
        {
            this.context.Mailbox.Remove(mailbox);
            this.context.SaveChanges();
        }

        public Mailbox Get(Guid id, bool includeMails)
        {
            if (includeMails)
            {
                return this.context.Mailbox.Include(m => m.Mails).FirstOrDefault(m => m.MailboxId == id);
            }
            else
            {
                return this.context.Mailbox.FirstOrDefault(m => m.MailboxId == id);
            }
        }

        public ICollection<Mailbox> GetAll()
        {
            return this.context.Mailbox.ToList();
        }

        public Mailbox LoginMailboxUser(string username, string password)
        {
            return this.context.Mailbox.FirstOrDefault(m => m.Username == username && m.Password == password);
        }

        public void Update(Mailbox mailbox)
        {
            Mailbox originalMailbox = this.Get(mailbox.MailboxId, false);

            originalMailbox.ImapEnabled = mailbox.ImapEnabled;
            originalMailbox.ImapHost = mailbox.ImapHost;
            originalMailbox.ImapPort = mailbox.ImapPort;
            originalMailbox.ImapSSLEnabled = mailbox.ImapSSLEnabled;
            originalMailbox.ImapUsername = mailbox.ImapUsername;
            originalMailbox.ImapPassword = mailbox.ImapPassword;

            this.context.Mailbox.Update(originalMailbox);
            this.context.SaveChanges();
        }
    }
}

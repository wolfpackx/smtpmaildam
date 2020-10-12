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
    public class MailRepository : IMailRepository
    {
        private ApplicationDbContext context;
        private ILogger<MailRepository> logger;

        public MailRepository(ApplicationDbContext context, ILogger<MailRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public void Create(Mail mail)
        {
            this.context.Mail.Add(mail);
            this.context.SaveChanges();
        }

        public void Delete(Mail mail)
        {
            this.context.Mail.Remove(mail);
            this.context.SaveChanges();
        }

        public Mail Get(Guid id)
        {
            return this.context.Mail.FirstOrDefault(m => m.MailId == id);
        }

        public ICollection<Mail> GetAll()
        {
            return this.context.Mail.ToList();
        }

        public ICollection<Mail> GetMailsFromMailbox(Guid mailboxId)
        {
            return this.context.Mail.Where(m => m.MailboxId == mailboxId).ToList();
        }

        public void Update(Mail mail)
        {
            this.context.Mail.Update(mail);
            this.context.SaveChanges();
        }
    }
}

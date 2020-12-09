using SmtpMailDam.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmtpMailDam.Common.Interfaces
{
    public interface IMailboxRepository
    {
        ICollection<Mailbox> GetAll(bool includeEmails = false);

        Mailbox LoginMailboxUser(string username, string password);

        Mailbox Get(Guid id, bool includeMails);

        void Update(Mailbox mailbox);

        void Create(Mailbox mailbox);

        void Delete(Mailbox mailbox);

        void ClearMails(Guid mailboxId);

        void ClearOldEmails(Guid mailboxId);

        void MarkAllMailsAsRead(Guid mailboxId);
    }
}

using SmtpMailDam.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmtpMailDam.Common.Interfaces
{
    public interface IMailRepository
    {
        ICollection<Mail> GetAll();

        ICollection<Mail> GetMailsFromMailbox(Guid mailId);

        Mail Get(Guid id);

        void Update(Mail mail);

        void Create(Mail mail);

        void Delete(Mail mail);
    }
}

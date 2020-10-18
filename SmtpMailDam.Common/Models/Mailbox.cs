using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmtpMailDam.Common.Models
{
    public class Mailbox
    {
        public Guid MailboxId { get; set; }

        public string Name { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public bool ImapEnabled { get; set; }

        public bool ImapSSLEnabled { get; set; }

        public string ImapHost { get; set; }

        public int ImapPort { get; set; }

        public string ImapUsername { get; set; }

        public string ImapPassword { get; set; }

        public bool Passthrough { get; set; }

        public ICollection<Mail> Mails { get; set; }
    }
}

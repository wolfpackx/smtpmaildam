using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SmtpMailDam.Website.Models
{
    public class MailboxViewModel
    {
        public Guid MailboxId { get; set; }

        public string Name { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        [DisplayName("Smtp port")]
        public int SmtpPort { get; set; }

        [DisplayName("Smtp host")]
        public string SmtpHost { get; set; }

        [DisplayName("Imap enabled")]
        public bool ImapEnabled { get; set; }

        [DisplayName("Imap SSL enabled")]
        public bool ImapSSLEnabled { get; set; }

        [DisplayName("Imap host")]
        public string ImapHost { get; set; }

        [DisplayName("Imap port")]
        public int? ImapPort { get; set; }

        [DisplayName("Imap username")]
        public string ImapUsername { get; set; }

        [DisplayName("Imap password")]
        public string ImapPassword { get; set; }

        [DisplayName("Passthrough")]
        public bool Passthrough { get; set; }

        [DisplayName("Mail retention in days")]
        [Range(0, 9999, ErrorMessage = "Price must be between 0 and 9999")]
        public int MailRetention { get; set; }

        public string Origin { get; set; }

        public IEnumerable<MailViewModel> Mails { get; set; }
    }
}

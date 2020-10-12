using System;
using System.Collections.Generic;
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

        public IEnumerable<MailViewModel> Mails { get; set; }
    }
}

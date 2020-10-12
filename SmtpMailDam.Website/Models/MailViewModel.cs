using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmtpMailDam.Website.Models
{
    public class MailViewModel
    {
        public Guid MailId { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public DateTime ReceiveDate { get; set; }

        public string Subject { get; set; }

        public string HtmlBody { get; set; }

        public string TextBody { get; set; }

        public string RenderedHtml { get; set; }

        public string RawEmail { get; set; }

        public int Status { get; set; }

        public Guid MailboxId { get; set; }
    }
}

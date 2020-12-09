using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SmtpMailDam.Website.Models
{
    public class MailViewModel
    {
        public Guid MailId { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        [DisplayName("Received date")]
        public DateTime ReceiveDate { get; set; }

        public string Subject { get; set; }

        public string HtmlBody { get; set; }

        public string TextBody { get; set; }

        public string RenderedHtml { get; set; }

        public string RawEmail { get; set; }

        public int Status { get; set; }

        public Guid MailboxId { get; set; }

        public IEnumerable<AttachmentViewModel> Attachements { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmtpMailDam.Common.Models
{
    public class Mail
    {
        public Guid MailId { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public DateTime ReceiveDate { get; set; }

        public string Subject { get; set; }

        public string HtmlBody { get; set; }

        public string TextBody { get; set; }

        [Column(TypeName = "varbinary(max)")]
        public byte[] RawEmail { get; set; }

        public Guid MailboxId { get; set; }

        public int Status { get; set; }
    }
}

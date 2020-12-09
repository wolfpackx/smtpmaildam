using Microsoft.AspNetCore.Routing.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmtpMailDam.Website.Models
{
    public class AttachmentViewModel
    {
        public string Id { get; set; }

        public string Filename { get; set; }

        public long Size { get; set; }
    }
}

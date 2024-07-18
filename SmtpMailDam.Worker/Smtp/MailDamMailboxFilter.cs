using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpMailDam.Worker.Smtp
{
    public class MailDamMailboxFilter : MailboxFilter
    {
        public override Task<bool> CanAcceptFromAsync(ISessionContext context, IMailbox @from, int size, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public override Task<bool> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox @from, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        public IMailboxFilter CreateInstance(ISessionContext context)
        {
            return new MailDamMailboxFilter();
        }
    }
}

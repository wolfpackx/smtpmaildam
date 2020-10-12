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
    public class MailboxFilter : IMailboxFilter, IMailboxFilterFactory
    {
        public Task<MailboxFilterResult> CanAcceptFromAsync(ISessionContext context, IMailbox @from, int size, CancellationToken cancellationToken)
        {
            return Task.FromResult(MailboxFilterResult.Yes);
        }

        public Task<MailboxFilterResult> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox @from, CancellationToken token)
        {
            return Task.FromResult(MailboxFilterResult.Yes);
        }

        public IMailboxFilter CreateInstance(ISessionContext context)
        {
            return new MailboxFilter();
        }
    }
}

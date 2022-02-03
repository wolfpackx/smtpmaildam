using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmtpMailDam.Common.Interfaces;
using SmtpServer;
using SmtpServer.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpMailDam.Worker.Smtp
{
    public class MailDamUserAuthenticator : UserAuthenticator
    {
        public override Task<bool> AuthenticateAsync(ISessionContext context, string user, string password, CancellationToken token)
        {
            var scope = (IServiceScope)context.Properties[SmtpServerConstants.Scope];

            ILogger<MailDamUserAuthenticator> logger = scope.ServiceProvider.GetRequiredService<ILogger<MailDamUserAuthenticator>>();

            Guid sessionId = (Guid)context.Properties[SmtpServerConstants.SessionId];

            try
            {
                var mailboxRepository = scope.ServiceProvider.GetRequiredService<IMailboxRepository>();

                var mailbox = mailboxRepository.LoginMailboxUser(user, password);

                if (mailbox == null)
                {
                    logger.LogWarning($"Invalid username {user} used for login in session {sessionId}");
                    return Task.FromResult(false);
                }

                context.Properties.Add(SmtpServerConstants.Mailbox, mailbox.MailboxId);

                logger.LogInformation($"Succesfull login for mailbox {mailbox.MailboxId} in session {sessionId}");

                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Login failed for username {user} in session {sessionId}");

                return Task.FromResult(false);
            }
        }
    }
}

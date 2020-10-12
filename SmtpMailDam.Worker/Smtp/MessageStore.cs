using SmtpMailDam.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmtpMailDam.Common.Models;
using SmtpMailDam.Common.Enums;
using System.IO;
using Microsoft.Extensions.Logging;
using SmtpMailDam.Worker.Smtp;

namespace SmtpMailDam.Worker.Smtp
{
    public class MessageStore : global::SmtpServer.Storage.MessageStore
    {
        public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, CancellationToken cancellationToken)
        {
            var scope = (IServiceScope)context.Properties[SmtpServerConstants.Scope];

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<MessageStore>>();

            Guid sessionId = (Guid)context.Properties[SmtpServerConstants.SessionId];
            var mailbox = (Guid)context.Properties[SmtpServerConstants.Mailbox];

            try
            {
                var mailRepository = scope.ServiceProvider.GetService<IMailRepository>();

                var textMessage = (ITextMessage)transaction.Message;

                var message = MimeKit.MimeMessage.Load(textMessage.Content);

                using var ms = new MemoryStream();
                textMessage.Content.Position = 0;
                textMessage.Content.CopyTo(ms);

                var mailId = Guid.NewGuid();

                var mail = new Mail
                {
                    MailId = mailId,
                    HtmlBody = message.HtmlBody,
                    TextBody = message.TextBody,
                    From = string.Join(",", message.From.Mailboxes.Select(m => !string.IsNullOrWhiteSpace(m.Name) ? $"{m.Name} <{m.Address}>" : m.Address).ToList()),
                    To = string.Join(",", message.To.Mailboxes.Select(m => !string.IsNullOrWhiteSpace(m.Name) ? $"{m.Name} <{m.Address}>" : m.Address).ToList()),
                    ReceiveDate = DateTime.Now,
                    MailboxId = mailbox,
                    Status = (int)MailStatus.Unread,
                    Subject = message.Subject,
                    RawEmail = ms.ToArray()
                };

                mailRepository.Create(mail);

                logger.LogInformation($"Storing mail {mailId} in session {sessionId} for mailbox {mailbox}");

                return Task.FromResult(SmtpResponse.Ok);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Storing of mail failed with in session {sessionId} for mailbox {mailbox}");

                return Task.FromResult(SmtpResponse.TransactionFailed);
            }
        }
    }
}

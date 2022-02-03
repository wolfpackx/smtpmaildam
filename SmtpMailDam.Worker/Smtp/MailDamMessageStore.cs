using SmtpMailDam.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using SmtpServer;
using SmtpServer.Protocol;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmtpMailDam.Common.Models;
using SmtpMailDam.Common.Enums;
using System.IO;
using Microsoft.Extensions.Logging;
using MimeKit;
using SmtpMailDam.Common.Utillity;
using System.Buffers;
using SmtpServer.Storage;

namespace SmtpMailDam.Worker.Smtp
{
    public class MailDamMessageStore : MessageStore
    {
        public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            var scope = (IServiceScope)context.Properties[SmtpServerConstants.Scope];

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<MailDamMessageStore>>();

            Guid sessionId = (Guid)context.Properties[SmtpServerConstants.SessionId];
            var mailboxId = (Guid)context.Properties[SmtpServerConstants.Mailbox];

            var mailId = Guid.NewGuid();

            var mailboxRepository = scope.ServiceProvider.GetService<IMailboxRepository>();

            var mailbox = mailboxRepository.Get(mailboxId, false);

            var mailRepository = scope.ServiceProvider.GetService<IMailRepository>();

            using var messageStream = new MemoryStream();

            var position = buffer.GetPosition(0);
            while (buffer.TryGet(ref position, out var memory))
            {
                messageStream.Write(memory.Span);
            }

            messageStream.Position = 0;

            var message = MimeKit.MimeMessage.LoadAsync(messageStream, cancellationToken).Result;

            bool result = true;

            if (mailbox.ImapEnabled && message != null)
            {
                try
                {
                    Imap.SendMessageToImap(mailbox, message);

                    logger.LogInformation($"Storing mail {mailId} in session {sessionId} for mailbox {mailbox.MailboxId} in imap");
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Storing of mail failed with in session {sessionId} for mailbox {mailbox.MailboxId} in imap because: {e.Message}");
                    result = false;
                }
            }

            if (!mailbox.Passthrough && message != null)
            {
                result = result && SaveMessage(logger, mailRepository, messageStream, mailId, message, mailboxId, sessionId);
            }

            return result ? Task.FromResult(SmtpResponse.Ok) : Task.FromResult(SmtpResponse.TransactionFailed);
        }

        private bool SaveMessage(ILogger<MailDamMessageStore> logger, IMailRepository mailRepository, MemoryStream messageStream, Guid mailId, MimeMessage message, Guid mailboxId, Guid sessionId)
        {
            try
            {
                var mail = new Mail
                {
                    MailId = mailId,
                    HtmlBody = message.HtmlBody,
                    TextBody = message.TextBody,
                    From = string.Join(",", message.From.Mailboxes.Select(m => !string.IsNullOrWhiteSpace(m.Name) ? $"{m.Name} <{m.Address}>" : m.Address).ToList()),
                    To = string.Join(",", message.To.Mailboxes.Select(m => !string.IsNullOrWhiteSpace(m.Name) ? $"{m.Name} <{m.Address}>" : m.Address).ToList()),
                    ReceiveDate = DateTime.Now,
                    MailboxId = mailboxId,
                    Status = (int)MailStatus.Unread,
                    Subject = message.Subject,
                    RawEmail = messageStream.ToArray()
                };

                mailRepository.Create(mail);

                logger.LogInformation($"Storing mail {mailId} in session {sessionId} for mailbox {mailboxId}");

                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Storing of mail failed with in session {sessionId} for mailbox {mailboxId} because: {e.Message}");

                return false;
            }
        }
    }
}

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
using MailKit.Net.Imap;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace SmtpMailDam.Worker.Smtp
{
    public class MessageStore : global::SmtpServer.Storage.MessageStore
    {
        public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, CancellationToken cancellationToken)
        {
            var scope = (IServiceScope)context.Properties[SmtpServerConstants.Scope];

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<MessageStore>>();

            Guid sessionId = (Guid)context.Properties[SmtpServerConstants.SessionId];
            var mailboxId = (Guid)context.Properties[SmtpServerConstants.Mailbox];

            MimeMessage message = null;
            var mailId = Guid.NewGuid();

            try
            {
                var mailRepository = scope.ServiceProvider.GetService<IMailRepository>();
                

                var textMessage = (ITextMessage)transaction.Message;

                message = MimeMessage.Load(textMessage.Content);

                using var ms = new MemoryStream();
                textMessage.Content.Position = 0;
                textMessage.Content.CopyTo(ms);

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
                    RawEmail = ms.ToArray()
                };

                mailRepository.Create(mail);

                logger.LogInformation($"Storing mail {mailId} in session {sessionId} for mailbox {mailboxId}");
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Storing of mail failed with in session {sessionId} for mailbox {mailboxId} because: {e.Message}");
            }

            try
            {
                var mailboxRepository = scope.ServiceProvider.GetService<IMailboxRepository>();

                var mailbox = mailboxRepository.Get(mailboxId, false);

                if (!mailbox.ImapEnabled || message == null)
                {
                    return Task.FromResult(SmtpResponse.Ok);
                }

                using (var client = new ImapClient())
                {
                    client.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
                    client.Connect(mailbox.ImapHost, mailbox.ImapPort, mailbox.ImapSSLEnabled ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto);

                    client.Authenticate(mailbox.ImapUsername, mailbox.ImapPassword);

                    client.Inbox.Append(message);

                    client.Disconnect(true);
                }

                logger.LogInformation($"Storing mail {mailId} in session {sessionId} for mailbox {mailboxId} in imap");

                return Task.FromResult(SmtpResponse.Ok);
            }
            catch(Exception e)
            {
                logger.LogError(e, $"Storing of mail failed with in session {sessionId} for mailbox {mailboxId} in imap because: {e.Message}");
                return Task.FromResult(SmtpResponse.TransactionFailed);
            }
        }

        public bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}

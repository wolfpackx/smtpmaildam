using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmtpMailDam.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SmtpMailDam.Worker.Jobs
{
    public interface IMailRetentionJob
    {
        void ClearOldEmails();
    }

    public class MailRetentionJob : IMailRetentionJob
    {
        private readonly ILogger<MailRetentionJob> logger;
        private readonly IMailboxRepository mailboxRepository;
        private readonly IConfiguration configuration;

        public MailRetentionJob(ILogger<MailRetentionJob> logger, IMailboxRepository mailboxRepository, IConfiguration configuration)
        {
            this.logger = logger;
            this.mailboxRepository = mailboxRepository;
            this.configuration = configuration;
        }

        public async Task Run(IJobCancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            ClearOldEmails();
        }

        public void ClearOldEmails()
        {
            this.logger.LogInformation("Starting MailRetention job");

            var mailboxes = mailboxRepository.GetAll();

            foreach (var mailbox in mailboxes)
            {
                try
                {
                    mailboxRepository.ClearOldEmails(mailbox.MailboxId);
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, $"Clearing emails failed for mailbox {mailbox.Name} older then {mailbox.MailRetention} days.");
                }
            }

            this.logger.LogInformation("Finished MailRetention job");
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmtpMailDam.Worker.Jobs;

namespace SmtpMailDam.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.ScheduleRecurringJobs();

            var smtpserverOptions = this._configuration.GetSection("SmtpServer");

            bool enabled = smtpserverOptions.GetValue<bool>("Enabled");

            if (!enabled)
            {
                return;
            }

            Smtp.SmtpServer1 smtpServer = new Smtp.SmtpServer1(
                _serviceProvider,
                smtpserverOptions.GetValue<string>("Ports"),
                smtpserverOptions.GetValue<string>("ServerName"),
                smtpserverOptions.GetValue<string>("SupportedSslProtocols"),
                smtpserverOptions.GetValue<bool>("Secure"),
                smtpserverOptions.GetValue<string>("CertificateFilePath"),
                smtpserverOptions.GetValue<string>("CertificatePasswordFilePath"),
                _serviceProvider.GetRequiredService<ILogger<Smtp.SmtpServer1>>());

            await smtpServer.Run();
        }

        private void ScheduleRecurringJobs()
        {
            RecurringJob.RemoveIfExists(nameof(MailRetentionJob));
            RecurringJob.AddOrUpdate<MailRetentionJob>(nameof(MailRetentionJob), job => job.Run(JobCancellationToken.Null), Cron.Daily());
        }
    }
}

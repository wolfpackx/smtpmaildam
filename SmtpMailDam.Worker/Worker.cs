using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            var smtpserverOptions = this._configuration.GetSection("SmtpServer");

            bool enabled = smtpserverOptions.GetValue<bool>("Enabled");

            if (!enabled)
            {
                return;
            }

            Smtp.SmtpServer smtpServer = new Smtp.SmtpServer(
                _serviceProvider,
                smtpserverOptions.GetValue<string>("Ports"),
                smtpserverOptions.GetValue<string>("ServerName"),
                smtpserverOptions.GetValue<string>("SupportedSslProtocols"),
                smtpserverOptions.GetValue<bool>("Secure"),
                smtpserverOptions.GetValue<string>("CertificateFilePath"),
                smtpserverOptions.GetValue<string>("CertificatePasswordFilePath"),
                _serviceProvider.GetRequiredService<ILogger<Smtp.SmtpServer>>());

            await smtpServer.Run();
        }
    }
}

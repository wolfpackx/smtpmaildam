using Microsoft.Extensions.Hosting;
using SmtpServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Net.Security;
using System.Net;

namespace SmtpMailDam.Worker.Smtp
{
    public class SmtpServer
    {
        IServiceProvider serviceProvider;
        private string ports;
        private string serverName;
        private string sslProtocols;
        private bool secure;
        private string certificateFilePath;
        private string certificatePasswordFilePath;
        private readonly ILogger<SmtpServer> _logger;

        public SmtpServer(IServiceProvider serviceProvider, string ports, string serverName, string sslProtocols, bool secure, string certificateFilePath, string certificatePasswordFilePath, ILogger<SmtpServer> logger)
        {
            this.serviceProvider = serviceProvider;
            this.ports = ports;
            this.serverName = serverName;
            this.sslProtocols = sslProtocols;
            this.secure = secure;
            this.certificateFilePath = certificateFilePath;
            this.certificatePasswordFilePath = certificatePasswordFilePath;
            this._logger = logger;
        }

        public async Task Run()
        {
            int[] ports = this.ports.Split(",").Select(p => int.Parse(p)).ToArray();

            SslProtocols sslProtocols = (SslProtocols)Enum.Parse(typeof(SslProtocols), this.sslProtocols);

            var optionsBuilder = new SmtpServerOptionsBuilder()
            .SupportedSslProtocols(sslProtocols)
            .ServerName(this.serverName)
            .Endpoint(builder =>
                    builder
                        .AllowUnsecureAuthentication()
                        .AuthenticationRequired()
                        .Port(ports[0]))
            .MessageStore(new MessageStore())
            .MailboxFilter(new MailboxFilter())
            .UserAuthenticator(new UserAuthenticator());

            if (secure)
            {
                // this is important when dealing with a certificate that isnt valid
                ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidationFailureForTestingOnly;

                optionsBuilder.Certificate(CreateCertificate(this.certificateFilePath, this.certificatePasswordFilePath));
            }

            var options = optionsBuilder.Build();

            var smtpServer = new global::SmtpServer.SmtpServer(options);

            smtpServer.SessionCreated += OnSessionCreated;
            smtpServer.SessionCompleted += OnSessionCompleted;
            smtpServer.SessionFaulted += OnSessionFaulted;
            smtpServer.SessionCancelled += OnSessionCancelled;

            await smtpServer.StartAsync(CancellationToken.None);
        }

        private void OnSessionCreated(object sender, SessionEventArgs e)
        {
            var sessionId = Guid.NewGuid();
            e.Context.Properties[SmtpServerConstants.SessionId] = sessionId;

            e.Context.Properties[SmtpServerConstants.Scope] = this.serviceProvider.CreateScope();

            this._logger.LogDebug($"Session created with id {sessionId}");

            e.Context.CommandExecuting += OnCommandExecuting;
        }

        private void OnCommandExecuting(object sender, SmtpCommandExecutingEventArgs e)
        {
            var sessionId = (Guid)e.Context.Properties[SmtpServerConstants.SessionId];

            this._logger.LogDebug($"Command in session {sessionId}: {e.Command}");
        }

        private void OnSessionCompleted(object sender, SessionEventArgs e)
        {
            e.Context.CommandExecuting -= OnCommandExecuting;

            var sessionId = (Guid)e.Context.Properties[SmtpServerConstants.SessionId];

            this._logger.LogDebug($"Session completed with id {sessionId}");

            e.Context.Properties.Remove(SmtpServerConstants.Scope);
        }

        private void OnSessionFaulted(object sender, SessionFaultedEventArgs e)
        {
            var sessionId = (Guid)e.Context.Properties[SmtpServerConstants.SessionId];

            this._logger.LogDebug($"Session with id {sessionId} faulted with: {e.Exception.Message}");

            e.Context.Properties.Remove(SmtpServerConstants.Scope);
        }

        private void OnSessionCancelled(object sender, SessionEventArgs e)
        {
            var sessionId = (Guid)e.Context.Properties[SmtpServerConstants.SessionId];

            this._logger.LogDebug($"Session cancelled with id {sessionId}");

            e.Context.Properties.Remove(SmtpServerConstants.Scope);
        }

        static bool IgnoreCertificateValidationFailureForTestingOnly(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private static X509Certificate2 CreateCertificate(string certificateFilePath, string passwordFilePath)
        {
            // to create an X509Certificate for testing you need to run MAKECERT.EXE and then PVK2PFX.EXE
            // http://www.digitallycreated.net/Blog/38/using-makecert-to-create-certificates-for-development

            var certificate = File.ReadAllBytes(certificateFilePath);
            var password = File.ReadAllText(passwordFilePath);

            return new X509Certificate2(certificate, password);
        }
    }
}

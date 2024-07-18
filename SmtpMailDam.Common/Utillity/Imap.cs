using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;
using SmtpMailDam.Common.Models;
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using MailKit;

namespace SmtpMailDam.Common.Utillity
{
    public class Imap
    {
        public static void SendMessageToImap(Mailbox mailbox, MimeMessage message)
        {
            using (var client = new ImapClient())
            {
                client.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
                client.Connect(mailbox.ImapHost, mailbox.ImapPort, mailbox.ImapSSLEnabled ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto);

                client.Authenticate(mailbox.ImapUsername, mailbox.ImapPassword);

                client.Inbox.Append(message);

                client.Disconnect(true);
            }
        }

        public static bool TestImapLogin(string host, int port, bool ssl, string username, string password)
        {
            bool result = false;

            try
            {
                using (var client = new ImapClient())
                {
                    client.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
                    client.Connect(host, port, ssl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto);

                    client.Authenticate(username, password);

                    client.Disconnect(true);
                }

                result = true;
            } 
            catch (Exception e)
            {
                result = false;
            }

            return result;
        }

        private static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}

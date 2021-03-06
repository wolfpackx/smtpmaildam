﻿using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;
using SmtpMailDam.Common.Models;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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

        public static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}

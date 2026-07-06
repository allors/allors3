// <copyright file="Mailer.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using Domain;
    using MailKit.Net.Smtp;
    using MimeKit;

    public class MailKitMailer : IMailer
    {
        public string Smtp { get; set; }

        public string DefaultSender { get; set; }

        public string DefaultSenderName { get; set; }

        public void Send(EmailMessage emailMesssage, string defaultSender)
        {
            if (!string.IsNullOrEmpty(defaultSender))
            {
                this.DefaultSender = defaultSender;

                if (string.IsNullOrEmpty(this.DefaultSenderName))
                {
                    this.DefaultSenderName = defaultSender;
                }
            }

            var message = new MimeMessage
            {
                Subject = emailMesssage.Subject,
                Body = new TextPart("html") { Text = emailMesssage.Body },
            };

            var sender = emailMesssage.Sender?.UserEmail ?? this.DefaultSender;
            var senderName = emailMesssage.Sender?.UserName ?? this.DefaultSenderName;

            message.From.Add(new MimeKit.MailboxAddress(senderName, sender));

            if (emailMesssage.ExistRecipientEmailAddress)
            {
                var address = new MimeKit.MailboxAddress(emailMesssage.RecipientEmailAddress, emailMesssage.RecipientEmailAddress);
                message.To.Add(address);
            }

            foreach (var recipient in emailMesssage.Recipients)
            {
                var address = new MimeKit.MailboxAddress(recipient.UserName, recipient.UserEmail);
                message.To.Add(address);
            }

            var (host, port) = ParseSmtp(this.Smtp);

            using var client = new SmtpClient();
            client.Connect(host, port);
            client.Send(message);
        }

        // Splits an "host" or "host:port" SMTP setting; defaults to port 25 when none is given.
        public static (string Host, int Port) ParseSmtp(string smtp)
        {
            if (string.IsNullOrWhiteSpace(smtp))
            {
                throw new System.InvalidOperationException(
                    "Mail:Smtp is not configured. Set it to the SMTP host (e.g. \"mail.example.com\") or host:port (e.g. \"localhost:1025\").");
            }

            var separator = smtp.LastIndexOf(':');
            if (separator > 0 && int.TryParse(smtp[(separator + 1)..], out var port))
            {
                return (smtp[..separator], port);
            }

            return (smtp, 25);
        }

        public void Dispose()
        {
        }
    }
}

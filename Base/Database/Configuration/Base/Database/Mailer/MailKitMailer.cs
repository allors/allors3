// <copyright file="Mailer.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using Domain;
    using MailKit.Net.Smtp;
    using MimeKit;

    public class MailKitMailer : IMailer
    {
        public string DefaultSender { get; set; }

        public string DefaultSenderName { get; set; }

        public void Send(EmailMessage emailMesssage)
        {
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
                var address = new MimeKit.MailboxAddress(emailMesssage.RecipientEmailAddress);
                message.To.Add(address);
            }

            foreach (User recipient in emailMesssage.Recipients)
            {
                var address = new MimeKit.MailboxAddress(recipient.UserEmail);
                message.To.Add(address);
            }

            using var client = new SmtpClient();
            client.Connect("smtp");
            client.Send(message);
        }

        public void Dispose()
        {
        }
    }
}

// <copyright file="MailService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.State
{
    using Domain;
    using MailKit.Net.Smtp;
    using MimeKit;
    using MailboxAddress = MimeKit.MailboxAddress;

    public class MailService : IMailService
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

            message.From.Add(new MailboxAddress(senderName, sender));

            if (emailMesssage.ExistRecipientEmailAddress)
            {
                message.To.Add(new MailboxAddress(emailMesssage.RecipientEmailAddress));
            }

            foreach (User recipient in emailMesssage.Recipients)
            {
                message.To.Add(new MailboxAddress(recipient.UserEmail));
            }

            using (var client = new SmtpClient())
            {
                client.Connect("smtp");
                client.Send(message);
            }
        }

        public void Dispose()
        {
        }
    }
}

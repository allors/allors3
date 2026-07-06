// <copyright file="AllorsEmailSender.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using Allors.Services;
    using Database.Domain;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Task = System.Threading.Tasks.Task;

    // Bridges the ASP.NET Core Identity UI (ForgotPassword, admin-triggered resets, ...) to the
    // Allors mail pipeline: each e-mail is persisted as an EmailMessage in the send queue, which
    // the EmailQueueDrainerService later transmits through the configured mailer. This lives in the
    // Base layer folder, so it is compiled into the Base and Apps servers but never into Core
    // (which has no EmailMessage model and keeps Identity's NoOp sender).
    public class AllorsEmailSender : IEmailSender
    {
        private readonly IDatabaseService databaseService;

        public AllorsEmailSender(IDatabaseService databaseService) => this.databaseService = databaseService;

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Read the database fresh each call: the e2e Test/Restart endpoint swaps it out.
            using var transaction = this.databaseService.Database.CreateTransaction();

            new EmailMessageBuilder(transaction)
                .WithRecipientEmailAddress(email)
                .WithSubject(subject)
                .WithBody(htmlMessage)
                .Build();

            transaction.Derive();
            transaction.Commit();

            return Task.CompletedTask;
        }
    }
}

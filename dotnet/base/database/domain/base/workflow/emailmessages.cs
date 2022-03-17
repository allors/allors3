// <copyright file="EmailMessages.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;


    public partial class EmailMessages
    {
        public void Send(string defaultSender)
        {
            var transaction = this.Transaction;

            var mailer = transaction.Database.Services.Get<IMailer>();
            var emailMessages = this.Extent();
            emailMessages.Filter.AddNot().AddExists(this.Meta.DateSending);
            emailMessages.Filter.AddNot().AddExists(this.Meta.DateSent);

            foreach (EmailMessage emailMessage in emailMessages)
            {
                try
                {
                    emailMessage.DateSending = transaction.Now();

                    transaction.Derive();
                    transaction.Commit();

                    mailer.Send(emailMessage, defaultSender);
                    emailMessage.DateSent = transaction.Now();

                    transaction.Derive();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                    transaction.Rollback();
                    break;
                }
            }
        }
    }
}

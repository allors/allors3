// <copyright file="Mailing.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Send queued e-mail messages")]
    public class Mailing
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            using var transaction = this.Parent.Database.CreateTransaction();
            this.Logger.Info("Begin");

            var system = new AutomatedAgents(transaction).System;
            transaction.Services.Get<IUserService>().User = system;

            // Pass no explicit sender: the configured mailer supplies the default sender
            // (Mail:DefaultSender). EmailMessages.Send drains every unsent message, marking each
            // DateSending then DateSent, and parks a message whose transport throws.
            new EmailMessages(transaction).Send(null);

            this.Logger.Info("End");

            return ExitCode.Success;
        }
    }
}

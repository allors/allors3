// <copyright file="Custom.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System.Linq;
    using Allors.Database.Derivations;
    using Allors.Database.Domain;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Execute custom code")]
    public class Custom
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            using var transaction = this.Parent.Database.CreateTransaction();
            this.Logger.Info("Begin");

            var m = this.Parent.M;

            //var scheduler = new AutomatedAgents(transaction).System;
            //transaction.Services.Get<IUserService>().User = scheduler;

            // Custom code

            return this.DoSomething();

            // Custom code
            transaction.Derive();
            transaction.Commit();

            this.Logger.Info("End");

            return ExitCode.Success;
        }

        private int DoSomething()
        {
            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                this.Logger.Info("Begin");

                var m = this.Parent.M;

                User user = new People(transaction).FindBy(m.Person.UserName, "jane@example.com");

                transaction.Services.Get<IUserService>().User = user;
                var derivation = transaction.Database.Services.Get<IDerivationService>().CreateDerivation(transaction);

                //var acl = new DatabaseAccessControl(user)[@this];
                //var result = acl.CanExecute(m.PurchaseOrder.Return);

                user.UserEmail = "sender@aa.com";

                var all = new People(transaction).Extent().ToArray();
                all[0].UserEmail = "recipient@aa.com";
                var recipient = all[0];

                new EmailMessageBuilder(transaction)
                    .WithDateCreated(transaction.Now().AddDays(-1).Date)
                    .WithSender(recipient)
                    .WithRecipient(user)
                    .WithSubject("hallo2")
                    .WithBody("body")
                    .Build();


                transaction.Derive();
                transaction.Commit();

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }
    }
}

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

                var @this = new PurchaseOrders(transaction).FindBy(m.PurchaseOrder.OrderNumber, "purchase orderno: 2");

                var item = (PurchaseOrderItem)@this.ValidOrderItems.First();
                item.DerivePurchaseOrderItemDisplayName(derivation.Validation);

                var acl = new DatabaseAccessControl(user)[@this];
                var result = acl.CanExecute(m.PurchaseOrder.Return);

                transaction.Commit();

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }
    }
}

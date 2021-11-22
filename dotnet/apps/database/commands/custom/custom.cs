// <copyright file="Custom.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
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

                User user = new People(transaction).FindBy(m.Person.LastName, "Kuhic");

                transaction.Services.Get<IUserService>().User = user;

                var wo = new WorkTasks(transaction).FindBy(m.WorkEffort.WorkEffortNumber, "a-WO-2");

                var acl = new DatabaseAccessControl(user)[wo];
                var result = acl.CanRead(m.WorkEffort.WorkEffortNumber);

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }
    }
}

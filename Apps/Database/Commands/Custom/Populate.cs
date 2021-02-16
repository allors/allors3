// <copyright file="Populate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using Allors;
    using Allors.Database;
    using Allors.Database.Domain;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Add file contents to the index")]
    public class Populate
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            this.Logger.Info("Begin");

            var database = this.Parent.Database;

            database.Init();

            using (var transaction = database.CreateTransaction())
            {
                var config = new Config { DataPath = this.Parent.DataPath };
                new Setup(transaction, config).Apply();

                transaction.Derive();
                transaction.Commit();

                var scheduler = new AutomatedAgents(transaction).System;
                transaction.Context().User = scheduler;

                new Allors.Database.Domain.Upgrade(transaction, this.Parent.DataPath).Execute();

                transaction.Derive();
                transaction.Commit();
            }

            this.Logger.Info("End");

            return ExitCode.Success;
        }
    }
}

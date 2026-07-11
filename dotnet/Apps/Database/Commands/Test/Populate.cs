// <copyright file="Populate.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using Allors;
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Creates a new test population")]
    public class Populate
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            this.Logger.Info("Begin");

            var database = this.Parent.Database;

            database.Init();

            var config = new Config { DataPath = this.Parent.DataPath };
            new Setup(database, config).Apply();

            using (var transaction = database.CreateTransaction())
            {
                transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;

                new TestPopulation(transaction, config).Populate(database);

                transaction.Derive();
                transaction.Commit();

                new Allors.Database.Domain.Upgrade(transaction, this.Parent.DataPath).Execute();

                transaction.Derive();
                transaction.Commit();
            }

            this.Logger.Info("End");

            return ExitCode.Success;
        }
    }
}

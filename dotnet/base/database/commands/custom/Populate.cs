// <copyright file="Populate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using Allors.Database.Domain;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;
    using Setup = Allors.Database.Domain.Setup;

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

            var config = new Config { DataPath = this.Parent.DataPath };
            new Setup(database, config).Apply();

            using (var session = database.CreateTransaction())
            {
                var scheduler = new AutomatedAgents(session).System;
                session.Services.Get<IUserService>().User = scheduler;

                new Allors.Database.Domain.Upgrade(session, this.Parent.DataPath).Execute();

                session.Derive();
                session.Commit();
            }

            this.Logger.Info("End");

            return ExitCode.Success;
        }
    }
}

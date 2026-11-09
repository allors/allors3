// <copyright file="Init.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using Allors.Database.Adapters;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Drop and create the configured database and its login from the admin connection")]
    public class Init
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            this.Logger.Info("Begin");

            var configuration = this.Parent.Configuration;
            var adapter = configuration["adapter"];

            if (!DatabaseBuilder.HasDatabaseServer(adapter))
            {
                this.Logger.Info("The {adapter} adapter keeps its data in memory; there is no database to drop or create", adapter);
            }
            else
            {
                var connectionString = configuration["ConnectionStrings:DefaultConnection"];

                this.Logger.Info("Dropping and creating the database {database} ({adapter})", DatabaseProvisioning.DatabaseName(adapter, connectionString), adapter);
                DatabaseProvisioning.DropCreate(adapter, connectionString);
            }

            this.Logger.Info("End");

            return ExitCode.Success;
        }
    }
}

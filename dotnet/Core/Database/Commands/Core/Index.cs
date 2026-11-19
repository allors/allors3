// <copyright file="Index.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Ensure the indexes of the database")]
    public class Index
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            this.Logger.Info("Begin");

            if (this.Parent.Database is Allors.Database.Adapters.Sql.Database database)
            {
                database.EnsureIndexes();
            }
            else
            {
                this.Logger.Info("The adapter of this database keeps no indexes");
            }

            this.Logger.Info("End");
            return ExitCode.Success;
        }
    }
}

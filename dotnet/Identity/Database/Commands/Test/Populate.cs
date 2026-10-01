// <copyright file="Populate.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using Allors.Database.Domain;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Initialize the configured database and set up its population")]
    public class Populate
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            this.Logger.Info("Begin");

            var database = this.Parent.Database;

            database.Init();

            new Setup(database, new Config()).Apply();

            this.Logger.Info("End");

            return ExitCode.Success;
        }
    }
}

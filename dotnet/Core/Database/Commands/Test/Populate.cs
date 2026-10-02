// <copyright file="Populate.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using Allors.Database.Domain;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Extensions.Logging;

    [Command(Description = "Add file contents to the index")]
    public partial class Populate
    {
        private readonly ILogger<Populate> logger;

        public Populate(ILogger<Populate> logger) => this.logger = logger;

        public Program Parent { get; set; }

        public int OnExecute(CommandLineApplication app)
        {
            this.LogBegin();

            var database = this.Parent.Database;

            database.Init();

            var config = new Config { DataPath = this.Parent.DataPath };
            new Setup(database, config).Apply();

            using (var session = database.CreateTransaction())
            {
                new Allors.Database.Domain.Upgrade(session, this.Parent.DataPath).Execute();

                session.Derive();
                session.Commit();
            }

            this.LogEnd();

            return ExitCode.Success;
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Begin")]
        private partial void LogBegin();

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "End")]
        private partial void LogEnd();
    }
}

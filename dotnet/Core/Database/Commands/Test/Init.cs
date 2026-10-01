// <copyright file="Init.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using Allors.Database.Adapters;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Extensions.Logging;

    [Command(Description = "Drop and (re)create the configured database from the admin connection")]
    public partial class Init
    {
        private readonly ILogger<Init> logger;

        public Init(ILogger<Init> logger) => this.logger = logger;

        public Program Parent { get; set; }

        public int OnExecute(CommandLineApplication app)
        {
            this.LogBegin();

            var configuration = this.Parent.Configuration;
            var adapter = configuration["adapter"];
            var connectionString = configuration["ConnectionStrings:DefaultConnection"];
            var database = DatabaseProvisioning.DatabaseName(adapter, connectionString);

            this.LogDropCreate(database, adapter);
            DatabaseProvisioning.DropCreate(adapter, database);

            this.LogEnd();

            return ExitCode.Success;
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Begin")]
        private partial void LogBegin();

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Drop/create database '{Database}' ({Adapter})")]
        private partial void LogDropCreate(string database, string adapter);

        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "End")]
        private partial void LogEnd();
    }
}

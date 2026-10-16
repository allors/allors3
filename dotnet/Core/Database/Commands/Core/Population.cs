// <copyright file="Population.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System;
    using Allors.Database;
    using Allors.Database.Adapters.Sql;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Extensions.Logging;

    [Command(Name = "population", Description = "Inspect or maintain the SQL population identity offline. Validation checks schema, not committed derivations or authentication. See docs/database-maintenance.md.")]
    public partial class Population
    {
        private readonly ILogger<Population> logger;

        public Population(ILogger<Population> logger) => this.logger = logger;

        public Program Parent { get; set; }

        [Option("--action <ACTION>", Description = "status (default), enroll an existing population, or renew after restore/recovery")]
        public string Action { get; set; } = "status";

        [Option("--offline", Description = "Confirm all hosts/writers are stopped and recovery, setup and committed derivations have been checked; required for enroll/renew")]
        public bool Offline { get; set; }

        public int OnExecute(CommandLineApplication app)
        {
            var action = this.Action?.Trim().ToLowerInvariant();
            if (action is not ("status" or "enroll" or "renew"))
            {
                throw new ArgumentException("Choose population --action status, enroll, or renew.");
            }

            if (action != "status" && !this.Offline)
            {
                throw new InvalidOperationException(
                    "Population enrollment and renewal require --offline. Stop every serving host and writer, finish or validate recovery/setup and committed derivations, " +
                    "then retry. Before reopening, recreate database instances/caches and retire or revalidate population-bound identities. See docs/database-maintenance.md.");
            }

            var lifecycle = For(this.Parent.Database);
            switch (action)
            {
                case "enroll":
                    lifecycle.Enroll();
                    break;
                case "renew":
                    lifecycle.Renew();
                    break;
            }

            var metadata = lifecycle.Read();
            if (metadata == null)
            {
                this.LogMissing();
            }
            else
            {
                this.LogState(metadata.Incarnation, metadata.State);
            }

            return ExitCode.Success;
        }

        internal static PopulationLifecycle For(IDatabase database) => database is Database sql
            ? sql.PopulationLifecycle
            : throw new InvalidOperationException("Population maintenance requires the npgsql or sqlclient adapter. Configure a supported SQL adapter before retrying.");

        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Population metadata is missing; the population has not been enrolled")]
        private partial void LogMissing();

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Population {Incarnation} is {State}")]
        private partial void LogState(Guid incarnation, PopulationState state);
    }
}

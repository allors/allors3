// <copyright file="Workspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Ranges;

    public abstract class Workspace : IWorkspace
    {
        protected Workspace(DatabaseConnection database, IWorkspaceServices services, IRanges<long> recordRanges)
        {
            this.DatabaseConnection = database;
            this.Services = services;
            this.RecordRanges = recordRanges;
            this.StrategyRanges = new DefaultClassRanges<Strategy>();
        }

        public DatabaseConnection DatabaseConnection { get; }

        public IConfiguration Configuration => this.DatabaseConnection.Configuration;

        public IWorkspaceServices Services { get; }

        public IRanges<long> RecordRanges { get; }

        public IRanges<Strategy> StrategyRanges { get; }

        public abstract ISession CreateSession();
    }
}

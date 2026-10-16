// <copyright file="DatabaseConnection.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.SystemTextJson;
    using Ranges;

    /// <summary>A ready in-process connection using protected reads and mutations with one committed SQL identity.</summary>
    public class DatabaseConnection : Remote.DatabaseConnection
    {
        private DatabaseConnection(Remote.Configuration configuration, Func<IWorkspaceServices> servicesBuilder,
            IDatabase database, LocalReadIdentity identity, IdGenerator idGenerator, IRanges<long> ranges)
            : base(configuration, idGenerator, servicesBuilder, ranges)
        {
            if (database is not Allors.Database.Adapters.Sql.Database)
                throw new NotSupportedException("Workspace operations require PostgreSQL or SQL Server. Memory remains available for database and domain tests.");
            this.Database = database;
            this.UserId = identity.UserId;
            this.UnitConvert = new UnitConvert();
        }

        public static async Task<DatabaseConnection> CreateAsync(Remote.Configuration configuration, Func<IWorkspaceServices> servicesBuilder,
            IDatabase database, LocalReadIdentity identity, IdGenerator idGenerator, IRanges<long> ranges,
            string revision, string context = null, CancellationToken cancellationToken = default)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            cancellationToken.ThrowIfCancellationRequested();
            var connection = new DatabaseConnection(configuration, servicesBuilder, database, identity, idGenerator, ranges);
            try
            {
                var readConfiguration = new ReadConfiguration(new System.Collections.Generic.Dictionary<string, string> { [configuration.Name] = revision });
                var scopeFactory = new ReadScopeFactory(database, readConfiguration);
                using (var scope = scopeFactory.Open(configuration.Name, identity, context)) { scope.Complete(); }
                await connection.InitializeAsync(new ReadTransport(new ReadExecutor(database, readConfiguration), configuration.Name, identity),
                    revision, context, new PushReadTransport(new PushReadExecutor(database, readConfiguration), configuration.Name, identity),
                    new InvokeReadTransport(new InvokeReadExecutor(database, readConfiguration), configuration.Name, identity), cancellationToken).ConfigureAwait(false);
                return connection;
            }
            catch { await connection.DisposeAsync().ConfigureAwait(false); throw; }
        }

        public IDatabase Database { get; }
        public long UserId { get; }
        public override IUnitConvert UnitConvert { get; }
    }
}

// <copyright file="DatabaseContainer.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Threading.Tasks;
    using Testcontainers.PostgreSql;
    using Xunit;

    public class DatabaseContainer : IAsyncLifetime
    {
        private readonly PostgreSqlContainer container;

        public DatabaseContainer()
        {
            this.container = new PostgreSqlBuilder().Build();
        }

        public string ConnectionString => this.container.GetConnectionString();

        public async Task InitializeAsync()
        {
            await this.container.StartAsync();
            Config.ConnectionString = this.ConnectionString;

            // TODO: replace timestamp with timestamp with time zone
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // TODO: https://www.npgsql.org/doc/release-notes/7.0.html#a-namecommandtypestoredprocedure-commandtypestoredprocedure-now-invokes-procedures-instead-of-functions
            AppContext.SetSwitch("Npgsql.EnableStoredProcedureCompatMode", true);
        }

        public async Task DisposeAsync()
        {
            await this.container.DisposeAsync();
        }
    }

    [CollectionDefinition("Database")]
    public class DatabaseCollection : ICollectionFixture<DatabaseContainer>
    {
    }
}

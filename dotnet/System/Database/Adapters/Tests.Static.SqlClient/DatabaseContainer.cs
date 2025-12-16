// <copyright file="DatabaseContainer.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System.Threading.Tasks;
    using Testcontainers.MsSql;
    using Xunit;

    public class DatabaseContainer : IAsyncLifetime
    {
        private readonly MsSqlContainer container;

        public DatabaseContainer()
        {
            this.container = new MsSqlBuilder().Build();
        }

        public string ConnectionString => this.container.GetConnectionString();

        public async Task InitializeAsync()
        {
            await this.container.StartAsync();
            Config.ConnectionString = this.ConnectionString;
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

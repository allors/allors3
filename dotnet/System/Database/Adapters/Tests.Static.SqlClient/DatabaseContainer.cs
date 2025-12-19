// <copyright file="DatabaseContainer.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class DatabaseContainer : IAsyncLifetime
    {
        public Task InitializeAsync()
        {
            Config.ConnectionString = Environment.GetEnvironmentVariable("allors_sqclient")
                ?? throw new InvalidOperationException("Environment variable 'allors_sqclient' is not set");

            return Task.CompletedTask;
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }

    [CollectionDefinition("Database")]
    public class DatabaseCollection : ICollectionFixture<DatabaseContainer>
    {
    }
}

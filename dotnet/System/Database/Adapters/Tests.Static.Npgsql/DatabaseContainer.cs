// <copyright file="DatabaseContainer.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class DatabaseContainer : IAsyncLifetime
    {
        public Task InitializeAsync()
        {
            Config.ConnectionString = Environment.GetEnvironmentVariable("ALLORS_NPGSQL")
                ?? throw new InvalidOperationException("Environment variable 'ALLORS_NPGSQL' is not set");

            // TODO: replace timestamp with timestamp with time zone
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // TODO: https://www.npgsql.org/doc/release-notes/7.0.html#a-namecommandtypestoredprocedure-commandtypestoredprocedure-now-invokes-procedures-instead-of-functions
            AppContext.SetSwitch("Npgsql.EnableStoredProcedureCompatMode", true);

            return Task.CompletedTask;
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }

    [CollectionDefinition("Database")]
    public class DatabaseCollection : ICollectionFixture<DatabaseContainer>
    {
    }
}

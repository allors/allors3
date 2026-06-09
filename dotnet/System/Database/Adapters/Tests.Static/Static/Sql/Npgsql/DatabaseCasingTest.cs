// <copyright file="DatabaseCasingTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using Caching;
    using global::Npgsql;
    using Meta;
    using Xunit;
    using C1 = Domain.C1;
    using ObjectFactory = ObjectFactory;

    public class DatabaseCasingTest
    {
        // PostgreSQL folds unquoted identifiers to lower-case and Provisioning creates databases that
        // way, so the database name derived from a connection string must be lower-cased too. Otherwise
        // 'Commands Init' creates 'allorscore' while the server keeps trying to connect to 'AllorsCore'.
        [Fact]
        public void DatabaseNameIsLowerCased()
        {
            var databaseName = Provisioning.DatabaseName("Server=localhost;Port=5432;Database=AllorsCore");

            Assert.Equal("allorscore", databaseName);
        }

        // The server reads a connection string verbatim from configuration; if it carries a non-lower-case
        // Database (e.g. a deployed 'Database=AllorsCore'), the adapter must normalize it to match the
        // database Provisioning actually created.
        [Fact]
        public void AdapterNormalizesDatabaseNameToLowerCase()
        {
            var configuredConnectionString = new NpgsqlConnectionStringBuilder
            {
                Host = "localhost",
                Port = 5432,
                Username = "allors",
                Password = "allors",
                Database = "AllorsMixedCaseTest",
            }.ConnectionString;

            var metaPopulation = new MetaBuilder().Build();
            var database = new Database(new DefaultDomainDatabaseServices(), new Configuration
            {
                ObjectFactory = new ObjectFactory(metaPopulation, typeof(C1)),
                ConnectionString = configuredConnectionString,
                CacheFactory = new DefaultCacheFactory(),
            });

            var connectDatabaseName = new NpgsqlConnectionStringBuilder(database.ConnectionString).Database;

            Assert.Equal("allorsmixedcasetest", connectDatabaseName);
        }
    }
}

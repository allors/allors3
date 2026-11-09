// <copyright file="DatabaseBuilderTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using Allors.Database;
    using Allors.Database.Adapters;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Microsoft.Extensions.Configuration;
    using Xunit;
    using C1 = Allors.Database.Domain.C1;

    public class DatabaseBuilderTests
    {
        [Fact]
        public void BuildNamesTheAdapterWithoutAValue()
        {
            var databaseBuilder = Builder(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=core",
            });

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => databaseBuilder.Build());

            Assert.Contains("The setting 'adapter' has no value", exception.Message);
            Assert.Contains("'snapshot', 'sqlclient' or 'npgsql'", exception.Message);
        }

        [Fact]
        public void BuildNamesTheUnknownAdapter()
        {
            var databaseBuilder = Builder(new Dictionary<string, string>
            {
                ["adapter"] = "mssql",
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=core",
            });

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => databaseBuilder.Build());

            Assert.Contains("The setting 'adapter' is 'mssql'", exception.Message);
            Assert.Contains("'snapshot', 'sqlclient' or 'npgsql'", exception.Message);
        }

        [Theory]
        [InlineData("sqlclient")]
        [InlineData("npgsql")]
        public void BuildNamesTheConnectionStringWithoutAValue(string adapter)
        {
            var databaseBuilder = Builder(new Dictionary<string, string>
            {
                ["adapter"] = adapter,
            });

            var exception = Assert.Throws<InvalidOperationException>(() => databaseBuilder.Build());

            Assert.Contains("The setting 'ConnectionStrings:DefaultConnection' has no value", exception.Message);
        }

        [Fact]
        public void BuildSnapshotWithoutAConnectionString()
        {
            var metaPopulation = new MetaBuilder().Build();
            var databaseBuilder = new DatabaseBuilder(
                new DefaultDatabaseServices(new Engine(Rules.Create(metaPopulation))),
                Configuration(new Dictionary<string, string>
                {
                    ["adapter"] = "snapshot",
                }),
                new ObjectFactory(metaPopulation, typeof(C1)));

            var database = databaseBuilder.Build();

            Assert.IsType<Allors.Database.Adapters.Snapshot.Database>(database);
            Assert.True(database.IsShared);
        }

        [Theory]
        [InlineData("snapshot", false)]
        [InlineData("sqlclient", true)]
        [InlineData("npgsql", true)]
        public void HasDatabaseServer(string adapter, bool expected) => Assert.Equal(expected, DatabaseBuilder.HasDatabaseServer(adapter));

        [Fact]
        public void HasDatabaseServerNamesTheUnknownAdapter()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => DatabaseBuilder.HasDatabaseServer("mssql"));

            Assert.Contains("The setting 'adapter' is 'mssql'", exception.Message);
        }

        private static IConfiguration Configuration(Dictionary<string, string> settings) =>
            new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        private static DatabaseBuilder Builder(Dictionary<string, string> settings) =>
            new DatabaseBuilder(null, Configuration(settings), null);
    }
}

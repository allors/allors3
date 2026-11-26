// <copyright file="DomainTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Collections.Generic;
    using Adapters;
    using Configuration;
    using Meta;
    using Microsoft.Extensions.Configuration;
    using MemoryConfiguration = Adapters.Memory.Configuration;
    using MemoryDatabase = Adapters.Memory.Database;
    using User = Domain.User;

    public class DomainTest : IDisposable
    {
        // The adapter that the tests run on: memory, the default, snapshot, sqlclient or npgsql. The tests read it
        // from this variable alone and never from the configuration root, so that a test run cannot initialize the
        // database of the application's programs.
        private const string AdapterVariable = "ALLORS_TEST_ADAPTER";

        // On SQL Server and PostgreSQL the tests drop and create this database once per run through the admin
        // connection in ALLORS_SQLCLIENT or ALLORS_NPGSQL, and connect to it with the same credentials. It stays
        // after the run.
        private const string DatabaseName = "sample_domain_tests";

        private static readonly string AdapterSetting = Environment.GetEnvironmentVariable(AdapterVariable);

        private static readonly Lazy<string> ServerConnectionString = new Lazy<string>(DropCreateServerDatabase);

        public DomainTest(Fixture fixture)
        {
            var services = new DefaultDatabaseServices(fixture.Engine);
            var objectFactory = new ObjectFactory(fixture.MetaPopulation, typeof(User));
            var database = CreateDatabase(services, objectFactory);

            this.M = database.Services.Get<MetaPopulation>();

            database.Init();

            // The setup of Core, with its security, through the hooks of this application.
            new Setup(database, new Config()).Apply();

            this.Transaction = database.CreateTransaction();
        }

        public MetaPopulation M { get; }

        public ITransaction Transaction { get; private set; }

        private static string Adapter => string.IsNullOrWhiteSpace(AdapterSetting) ? "memory" : AdapterSetting.Trim().ToLowerInvariant();

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;
        }

        private static IDatabase CreateDatabase(IDatabaseServices services, ObjectFactory objectFactory)
        {
            switch (Adapter)
            {
                case "memory":
                    return new MemoryDatabase(services, new MemoryConfiguration { ObjectFactory = objectFactory });

                case "snapshot":
                    return Build(services, objectFactory, new Dictionary<string, string> { ["adapter"] = "snapshot" });

                case "sqlclient":
                case "npgsql":
                    return Build(services, objectFactory, new Dictionary<string, string>
                    {
                        ["adapter"] = Adapter,
                        ["ConnectionStrings:DefaultConnection"] = ServerConnectionString.Value,
                    });

                default:
                    throw new InvalidOperationException($"{AdapterVariable} is '{AdapterSetting}'. Set it to memory, snapshot, sqlclient or npgsql, or leave it unset for the Memory adapter.");
            }
        }

        private static IDatabase Build(IDatabaseServices services, ObjectFactory objectFactory, Dictionary<string, string> settings)
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            return new DatabaseBuilder(services, configuration, objectFactory).Build();
        }

        private static string DropCreateServerDatabase()
        {
            if (Adapter == "sqlclient")
            {
                Adapters.Sql.SqlClient.Provisioning.DropCreate(DatabaseName);
                return Adapters.Sql.SqlClient.Provisioning.ConnectionString(DatabaseName);
            }

            Adapters.Sql.Npgsql.Provisioning.DropCreate(DatabaseName);
            return Adapters.Sql.Npgsql.Provisioning.ConnectionString(DatabaseName);
        }
    }
}

// <copyright file="DatabaseFixture.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using Adapters;
    using global::Npgsql;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using NpgsqlProvisioning = Adapters.Sql.Npgsql.Provisioning;
    using SqlClientProvisioning = Adapters.Sql.SqlClient.Provisioning;

    public sealed class DatabaseFixture : IDisposable
    {
        // Server runs recreate this database under a lock and leave it available for inspection.
        private const string DatabaseName = "sample_domain_tests";

        private readonly DbConnection lockConnection;
        private readonly IConfiguration configuration;

        public DatabaseFixture()
        {
            // Select the adapter independently of the application's configuration root and connection string.
            const string adapterVariable = "ALLORS_TEST_ADAPTER";
            var setting = Environment.GetEnvironmentVariable(adapterVariable);
            var adapter = string.IsNullOrWhiteSpace(setting) ? "memory" : setting.Trim().ToLowerInvariant();
            var settings = new Dictionary<string, string> { ["adapter"] = adapter };

            try
            {
                switch (adapter)
                {
                    case "memory":
                        return;

                    case "snapshot":
                        break;

                    case "sqlclient":
                        RequireAdminConnection("ALLORS_SQLCLIENT");
                        var sqlServer = new SqlConnectionStringBuilder(SqlClientProvisioning.ConnectionString("master"))
                        {
                            Pooling = false,
                            ConnectRetryCount = 0,
                        };
                        this.lockConnection = new SqlConnection(sqlServer.ConnectionString);
                        this.AcquireLock($@"DECLARE @result int;
EXEC @result = sys.sp_getapplock @Resource = N'{DatabaseName}', @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = -1;
IF @result < 0 THROW 50000, 'Could not lock {DatabaseName} for the domain tests.', 1;");
                        SqlClientProvisioning.DropCreate(DatabaseName);
                        settings["ConnectionStrings:DefaultConnection"] = SqlClientProvisioning.ConnectionString(DatabaseName);
                        break;

                    case "npgsql":
                        RequireAdminConnection("ALLORS_NPGSQL");
                        // Before PostgreSQL 13 the next run can only drop the database after every session closes.
                        var postgres = new NpgsqlConnectionStringBuilder(NpgsqlProvisioning.ConnectionString(DatabaseName))
                        {
                            Pooling = false,
                            Multiplexing = false,
                        };
                        settings["ConnectionStrings:DefaultConnection"] = postgres.ConnectionString;
                        postgres.Database = "postgres";
                        this.lockConnection = new NpgsqlConnection(postgres.ConnectionString);
                        this.AcquireLock($"SELECT pg_advisory_lock(hashtext('{DatabaseName}'))");
                        NpgsqlProvisioning.DropCreate(DatabaseName);
                        break;

                    default:
                        throw new InvalidOperationException($"{adapterVariable} is '{setting}'. Set it to memory, snapshot, sqlclient or npgsql, or leave it unset for the Memory adapter.");
                }

                this.configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            }
            catch
            {
                this.Dispose();
                throw;
            }
        }

        public void Dispose() => this.lockConnection?.Dispose();

        internal IDatabase CreateDatabase(IDatabaseServices services, ObjectFactory objectFactory) =>
            this.configuration == null
                ? new Adapters.Memory.Database(services, new Adapters.Memory.Configuration { ObjectFactory = objectFactory })
                : new DatabaseBuilder(services, this.configuration, objectFactory).Build();

        private static void RequireAdminConnection(string variable)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(variable)))
            {
                throw new InvalidOperationException($"{variable} is not set. Point it at an admin connection that can create the domain test database.");
            }
        }

        // Keep the maintenance session open until xUnit disposes the collection, after the last domain test.
        private void AcquireLock(string sql)
        {
            this.lockConnection.Open();
            using var command = this.lockConnection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 0;
            command.ExecuteNonQuery();
        }
    }
}

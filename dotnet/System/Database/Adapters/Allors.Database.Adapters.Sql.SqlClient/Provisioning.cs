// <copyright file="Provisioning.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Creates and drops SQL Server databases from an admin connection. The admin connection is read from
    /// the <c>ALLORS_SQLCLIENT</c> environment variable (with <c>ALLORS_TEST_SQLCLIENT_CONNECTION</c>
    /// accepted as an alias) and must be allowed to create databases.
    /// </summary>
    public static class Provisioning
    {
        public const string EnvironmentVariable = "ALLORS_SQLCLIENT";
        private const string LegacyEnvironmentVariable = "ALLORS_TEST_SQLCLIENT_CONNECTION";

        /// <summary>The admin connection string, pointed at the <c>master</c> database.</summary>
        public static string AdminConnectionString() =>
            new SqlConnectionStringBuilder(RawConnectionString())
            {
                InitialCatalog = "master",
            }.ConnectionString;

        /// <summary>A connection string for <paramref name="database"/>, derived from the admin connection.</summary>
        public static string ConnectionString(string database) =>
            new SqlConnectionStringBuilder(RawConnectionString())
            {
                InitialCatalog = database,
            }.ConnectionString;

        /// <summary>The database name contained in <paramref name="connectionString"/>.</summary>
        public static string DatabaseName(string connectionString) =>
            new SqlConnectionStringBuilder(connectionString).InitialCatalog;

        /// <summary>Drops <paramref name="database"/> if it exists and creates it fresh.</summary>
        public static void DropCreate(string database)
        {
            using var connection = new SqlConnection(AdminConnectionString());
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandTimeout = 300;

            // SINGLE_USER ... ROLLBACK IMMEDIATE evicts active connections so the drop cannot block.
            command.CommandText = $@"
IF DB_ID('{database}') IS NOT NULL
BEGIN
    ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{database}];
END";
            command.ExecuteNonQuery();

            command.CommandText = $"CREATE DATABASE [{database}]";
            command.ExecuteNonQuery();
        }

        private static string RawConnectionString()
        {
            var raw = Environment.GetEnvironmentVariable(EnvironmentVariable);
            if (string.IsNullOrWhiteSpace(raw))
            {
                raw = Environment.GetEnvironmentVariable(LegacyEnvironmentVariable);
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new InvalidOperationException(
                    $"{EnvironmentVariable} is not set. Point it at an admin SQL Server connection that can create databases, for example:{Environment.NewLine}" +
                    $"  export {EnvironmentVariable}=\"Server=localhost;User Id=sa;Password=Passw0rd!;TrustServerCertificate=true\"");
            }

            return raw;
        }
    }
}

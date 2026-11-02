// <copyright file="Provisioning.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Creates test databases from an admin SQL Server connection.
    /// </summary>
    public static class Provisioning
    {
        private const string EnvironmentVariable = "ALLORS_SQLCLIENT";
        private const string LegacyEnvironmentVariable = "ALLORS_TEST_SQLCLIENT_CONNECTION";

        public static string ConnectionString(string database) =>
            new SqlConnectionStringBuilder(RawConnectionString())
            {
                InitialCatalog = database,
            }.ConnectionString;

        public static void DropCreate(string database)
        {
            using var connection = new SqlConnection(new SqlConnectionStringBuilder(RawConnectionString())
            {
                InitialCatalog = "master",
            }.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandTimeout = 300;
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
            var connectionString = Environment.GetEnvironmentVariable(EnvironmentVariable);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = Environment.GetEnvironmentVariable(LegacyEnvironmentVariable);
            }

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            if (OperatingSystem.IsWindows())
            {
                return @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true";
            }

            throw new InvalidOperationException(
                $"{EnvironmentVariable} is not set. Point it at an admin SQL Server connection that can create databases.");
        }
    }
}

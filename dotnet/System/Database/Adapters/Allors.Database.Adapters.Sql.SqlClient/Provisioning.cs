// <copyright file="Provisioning.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Creates databases from an admin SQL Server connection.
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

        /// <summary>The database name contained in <paramref name="connectionString"/>.</summary>
        public static string DatabaseName(string connectionString) =>
            new SqlConnectionStringBuilder(connectionString).InitialCatalog;

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

        /// <summary>
        /// Drops and creates the database of a program's connection string from the admin connection and
        /// makes the login that the connection string names the owner of the database. A login that the
        /// server does not know is created with the password of the connection string, without the
        /// password policy check so that a development password is accepted; an existing login is left
        /// as it is. A connection string with integrated security names no login: the database is
        /// created and nothing else.
        /// </summary>
        public static void DropCreateFor(string connectionString)
        {
            var program = new SqlConnectionStringBuilder(connectionString);
            var database = program.InitialCatalog;
            if (string.IsNullOrWhiteSpace(database))
            {
                throw new InvalidOperationException("The connection string names no database. Add 'Database=<name>' to it.");
            }

            DropCreate(database);

            if (program.IntegratedSecurity || string.IsNullOrWhiteSpace(program.UserID))
            {
                return;
            }

            using var connection = new SqlConnection(new SqlConnectionStringBuilder(RawConnectionString())
            {
                InitialCatalog = "master",
            }.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandTimeout = 300;
            command.CommandText = $@"
IF SUSER_ID({Literal(program.UserID)}) IS NULL
BEGIN
    CREATE LOGIN {Identifier(program.UserID)} WITH PASSWORD = {Literal(program.Password)}, CHECK_POLICY = OFF;
END";
            command.ExecuteNonQuery();
            command.CommandText = $"ALTER AUTHORIZATION ON DATABASE::{Identifier(database)} TO {Identifier(program.UserID)}";
            command.ExecuteNonQuery();
        }

        private static string Identifier(string name) => "[" + name.Replace("]", "]]") + "]";

        private static string Literal(string value) => "N'" + value.Replace("'", "''") + "'";

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

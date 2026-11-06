// <copyright file="Provisioning.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Linq;
    using global::Npgsql;

    /// <summary>
    /// Creates and drops PostgreSQL databases from an admin connection. The admin connection is read from
    /// the <c>ALLORS_NPGSQL</c> environment variable (with <c>ALLORS_TEST_NPGSQL_CONNECTION</c> accepted as
    /// an alias) and must be allowed to create databases.
    /// </summary>
    public static class Provisioning
    {
        public const string EnvironmentVariable = "ALLORS_NPGSQL";
        private const string LegacyEnvironmentVariable = "ALLORS_TEST_NPGSQL_CONNECTION";

        /// <summary>The admin connection string, pointed at the <c>postgres</c> maintenance database.</summary>
        public static string AdminConnectionString() =>
            new NpgsqlConnectionStringBuilder(RawConnectionString())
            {
                Database = "postgres",
                CommandTimeout = 300,
            }.ConnectionString;

        /// <summary>A connection string for <paramref name="database"/>, derived from the admin connection.</summary>
        public static string ConnectionString(string database) =>
            new NpgsqlConnectionStringBuilder(RawConnectionString())
            {
                Database = database.ToLowerInvariant(),
                Pooling = false,
                Enlist = false,
                CommandTimeout = 300,
            }.ConnectionString;

        /// <summary>The database name contained in <paramref name="connectionString"/>, lower-cased to
        /// match how <see cref="DropCreate"/> and PostgreSQL fold unquoted identifiers.</summary>
        public static string DatabaseName(string connectionString) =>
            new NpgsqlConnectionStringBuilder(connectionString).Database?.ToLowerInvariant();

        /// <summary>Drops <paramref name="database"/> if it exists and creates it fresh.</summary>
        public static void DropCreate(string database)
        {
            database = database.ToLowerInvariant();

            // Quote the identifier so names that require it (e.g. 'allors-core' or a reserved word) are valid
            // SQL; double any embedded quote. The name is lower-cased to match the adapter's connect name.
            var quoted = "\"" + database.Replace("\"", "\"\"") + "\"";
            var adminConnectionString = AdminConnectionString();

            int version;
            using (var connection = new NpgsqlConnection(adminConnectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SHOW server_version";
                var full = command.ExecuteScalar()?.ToString() ?? string.Empty;
                var major = new string(full.TakeWhile(char.IsDigit).ToArray());
                version = int.TryParse(major, out var parsed) ? parsed : 0;
            }

            using (var connection = new NpgsqlConnection(adminConnectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                // FORCE (PostgreSQL 13+) terminates active connections so the drop cannot block.
                var withForce = version >= 13 ? "WITH (FORCE)" : string.Empty;
                command.CommandText = $"DROP DATABASE IF EXISTS {quoted} {withForce}";
                command.ExecuteNonQuery();
            }

            using (var connection = new NpgsqlConnection(adminConnectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = $"CREATE DATABASE {quoted}";
                command.ExecuteNonQuery();
            }
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
                    $"{EnvironmentVariable} is not set. Point it at an admin PostgreSQL connection that can create databases, for example:{Environment.NewLine}" +
                    $"  export {EnvironmentVariable}=\"Host=localhost;Port=5432;Username=allors;Password=allors\"");
            }

            return raw;
        }
    }
}

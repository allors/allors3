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
                Enlist = false,
                CommandTimeout = 300,
            }.ConnectionString;

        /// <summary>The database name contained in <paramref name="connectionString"/>, lower-cased to
        /// match how <see cref="DropCreate"/> and PostgreSQL fold unquoted identifiers.</summary>
        public static string DatabaseName(string connectionString) =>
            new NpgsqlConnectionStringBuilder(connectionString).Database?.ToLowerInvariant();

        /// <summary>Drops <paramref name="database"/> if it exists and creates it fresh.</summary>
        public static void DropCreate(string database) => DropCreate(database, null);

        /// <summary>
        /// Drops and creates the database of a program's connection string from the admin connection and
        /// makes the role that the connection string names the owner of the database. A role that the
        /// server does not know is created with the password of the connection string; an existing role
        /// is left as it is.
        /// </summary>
        public static void DropCreateFor(string connectionString)
        {
            var program = new NpgsqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(program.Database))
            {
                throw new InvalidOperationException("The connection string names no database. Add 'Database=<name>' to it.");
            }

            var role = string.IsNullOrWhiteSpace(program.Username) ? null : program.Username;
            if (role != null)
            {
                using var connection = new NpgsqlConnection(AdminConnectionString());
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1 FROM pg_roles WHERE rolname = @role";
                command.Parameters.AddWithValue("role", role);
                var exists = command.ExecuteScalar() != null;
                command.Parameters.Clear();

                if (!exists)
                {
                    var password = string.IsNullOrEmpty(program.Password) ? string.Empty : $" PASSWORD {Literal(program.Password)}";
                    command.CommandText = $"CREATE ROLE {Identifier(role)} LOGIN{password}";
                    command.ExecuteNonQuery();
                }
            }

            DropCreate(program.Database, role);
        }

        // Quotes an identifier so that names that require it (e.g. 'allors-core' or a reserved word) are
        // valid SQL; an embedded quote is doubled.
        private static string Identifier(string name) => "\"" + name.Replace("\"", "\"\"") + "\"";

        private static string Literal(string value) => "'" + value.Replace("'", "''") + "'";

        private static void DropCreate(string database, string owner)
        {
            // The name is lower-cased to match the adapter's connect name.
            database = database.ToLowerInvariant();

            var quoted = Identifier(database);
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
                command.CommandText = owner == null ? $"CREATE DATABASE {quoted}" : $"CREATE DATABASE {quoted} OWNER {Identifier(owner)}";
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

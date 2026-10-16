// <copyright file="PopulationMetadataStore.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using global::Npgsql;

    internal sealed class PopulationMetadataStore : IPopulationMetadataStore
    {
        private readonly Database database;

        internal PopulationMetadataStore(Database database) => this.database = database;

        private string Schema => Quote(this.database.SchemaName);

        private string Table => this.Schema + ".\"_population\"";

        public PopulationMetadata Read()
        {
            using var connection = new NpgsqlConnection(this.database.ConnectionString);
            connection.Open();
            return this.Read(connection);
        }

        public void BeginMaintenance(Guid incarnation)
        {
            try
            {
                using var connection = new NpgsqlConnection(this.database.ConnectionString);
                connection.Open();
                using var transaction = connection.BeginTransaction();
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = $@"CREATE SCHEMA IF NOT EXISTS {this.Schema};
CREATE TABLE IF NOT EXISTS {this.Table} (
    singleton SMALLINT PRIMARY KEY CHECK (singleton = 1),
    incarnation UUID NOT NULL CHECK (incarnation <> '00000000-0000-0000-0000-000000000000'),
    state VARCHAR(11) NOT NULL CHECK (state IN ('Maintenance', 'Ready'))
);
INSERT INTO {this.Table} (singleton, incarnation, state) VALUES (1, @incarnation, 'Maintenance')
ON CONFLICT (singleton) DO UPDATE SET incarnation = EXCLUDED.incarnation, state = EXCLUDED.state;";
                command.Parameters.AddWithValue("incarnation", incarnation);
                command.ExecuteNonQuery();
                this.Read(connection, transaction);
                transaction.Commit();
            }
            catch (PostgresException exception)
            {
                throw this.Malformed(exception);
            }
        }

        public bool CompleteMaintenance(Guid expectedIncarnation)
        {
            try
            {
                using var connection = new NpgsqlConnection(this.database.ConnectionString);
                connection.Open();
                using var transaction = connection.BeginTransaction();
                var metadata = this.Read(connection, transaction);
                if (metadata.Incarnation != expectedIncarnation || metadata.State != PopulationState.Maintenance)
                {
                    return false;
                }

                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = $@"UPDATE {this.Table} SET state = 'Ready'
WHERE singleton = 1 AND incarnation = @incarnation AND state = 'Maintenance'";
                command.Parameters.AddWithValue("incarnation", expectedIncarnation);
                var completed = command.ExecuteNonQuery() == 1;
                if (completed)
                {
                    transaction.Commit();
                }

                return completed;
            }
            catch (PostgresException exception)
            {
                throw this.Malformed(exception);
            }
        }

        public void Validate()
        {
            var validation = new Validation(this.database);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException("The population schema is invalid. Complete offline initialization or restore before marking it ready. " + validation.Message);
            }
        }

        private static string Quote(string identifier) => "\"" + identifier.Replace("\"", "\"\"") + "\"";

        internal PopulationMetadata Read(NpgsqlConnection connection, NpgsqlTransaction transaction = null, bool fence = false)
        {
            // The fence is the first data read of an admitted write transaction.
            // Missing metadata fails closed through the same error path as malformed metadata.
            if (!fence)
            {
                using (var existence = connection.CreateCommand())
                {
                    existence.Transaction = transaction;
                    existence.CommandText = "SELECT to_regclass(@table)::text";
                    existence.Parameters.AddWithValue("table", this.Table);
                    if (existence.ExecuteScalar() is null or DBNull)
                    {
                        return null;
                    }
                }
            }

            try
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = $"SELECT singleton, incarnation, state FROM {this.Table}" + (fence ? " FOR SHARE" : string.Empty);
                using var reader = command.ExecuteReader();
                // Npgsql permits numeric conversion through GetInt16, including rounding.
                // Require the stored key's native CLR type instead of a converted value.
                if (!reader.Read() || reader.IsDBNull(0) || reader.IsDBNull(1) || reader.IsDBNull(2)
                    || reader.GetValue(0) is not short singleton || singleton != 1)
                {
                    throw this.Malformed();
                }

                var incarnation = reader.GetGuid(1);
                var state = reader.GetString(2) switch
                {
                    "Maintenance" => PopulationState.Maintenance,
                    "Ready" => PopulationState.Ready,
                    _ => throw this.Malformed(),
                };
                if (incarnation == Guid.Empty || reader.Read())
                {
                    throw this.Malformed();
                }

                return new PopulationMetadata(incarnation, state);
            }
            catch (Exception exception) when (exception is PostgresException or InvalidCastException or FormatException or OverflowException)
            {
                throw this.Malformed(exception);
            }
        }

        private InvalidOperationException Malformed(Exception inner = null) =>
            new InvalidOperationException($"Population lifecycle metadata in {this.Table} is malformed. Stop all hosts and repair or restore the metadata before reopening service.", inner);
    }
}

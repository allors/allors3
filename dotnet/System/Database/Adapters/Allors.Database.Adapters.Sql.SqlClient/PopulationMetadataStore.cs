// <copyright file="PopulationMetadataStore.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Data;
    using Microsoft.Data.SqlClient;

    internal sealed class PopulationMetadataStore : IPopulationMetadataStore
    {
        private readonly Database database;

        internal PopulationMetadataStore(Database database) => this.database = database;

        private string TableName => $"{Quote(this.database.SchemaName)}.[_population]";

        public PopulationMetadata Read()
        {
            using var connection = new SqlConnection(this.database.ConnectionString);
            connection.Open();
            return this.Read(connection, null);
        }

        internal PopulationMetadata Read(SqlConnection connection, SqlTransaction transaction, bool fence = false)
        {
            // The fence is the first data read of an admitted write transaction.
            // Missing metadata fails closed through the same error path as malformed metadata.
            if (!fence)
            {
                using (var exists = connection.CreateCommand())
                {
                    exists.Transaction = transaction;
                    exists.CommandText = "SELECT OBJECT_ID(@table, 'U')";
                    exists.Parameters.Add("@table", SqlDbType.NVarChar, 776).Value = this.TableName;
                    if (exists.ExecuteScalar() is DBNull)
                    {
                        return null;
                    }
                }
            }

            try
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = $"SELECT [singleton], [incarnation], [state] FROM {this.TableName}" + (fence ? " WITH (HOLDLOCK)" : string.Empty);
                using var reader = command.ExecuteReader();
                if (!reader.Read() || reader.IsDBNull(0) || reader.IsDBNull(1) || reader.IsDBNull(2))
                {
                    throw this.InvalidMetadata();
                }

                var singleton = reader.GetByte(0);
                var incarnation = reader.GetGuid(1);
                var stateText = reader.GetString(2);
                if (singleton != 1 || incarnation == Guid.Empty ||
                    (stateText != nameof(PopulationState.Maintenance) && stateText != nameof(PopulationState.Ready)) || reader.Read())
                {
                    throw this.InvalidMetadata();
                }

                return new PopulationMetadata(incarnation, stateText == nameof(PopulationState.Ready) ? PopulationState.Ready : PopulationState.Maintenance);
            }
            catch (Exception exception) when (exception is SqlException or InvalidCastException or FormatException or OverflowException)
            {
                throw this.InvalidMetadata(exception);
            }
        }

        public void BeginMaintenance(Guid incarnation)
        {
            new Initialization(this.database).AllowSnapshotIsolation();
            using var connection = new SqlConnection(this.database.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction(IsolationLevel.Snapshot);
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            var createSchema = $"CREATE SCHEMA {Quote(this.database.SchemaName)}".Replace("'", "''");
            command.CommandText = $@"
IF SCHEMA_ID(@schema) IS NULL EXEC(N'{createSchema}');
IF OBJECT_ID(@table, 'U') IS NULL
BEGIN
    CREATE TABLE {this.TableName}
    (
        [singleton] tinyint NOT NULL PRIMARY KEY CHECK ([singleton] = 1),
        [incarnation] uniqueidentifier NOT NULL,
        [state] nvarchar(16) NOT NULL CHECK ([state] IN (N'Maintenance', N'Ready'))
    );
END;
UPDATE {this.TableName} SET [incarnation] = @incarnation, [state] = N'Maintenance' WHERE [singleton] = 1;
IF @@ROWCOUNT = 0 INSERT INTO {this.TableName} ([singleton], [incarnation], [state]) VALUES (1, @incarnation, N'Maintenance');";
            command.Parameters.Add("@schema", SqlDbType.NVarChar, 128).Value = this.database.SchemaName;
            command.Parameters.Add("@table", SqlDbType.NVarChar, 776).Value = this.TableName;
            command.Parameters.Add("@incarnation", SqlDbType.UniqueIdentifier).Value = incarnation;
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        public bool CompleteMaintenance(Guid expectedIncarnation)
        {
            using var connection = new SqlConnection(this.database.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction(IsolationLevel.Snapshot);
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"UPDATE {this.TableName} SET [state] = N'Ready' WHERE [singleton] = 1 AND [incarnation] = @incarnation AND [state] = N'Maintenance'";
            command.Parameters.Add("@incarnation", SqlDbType.UniqueIdentifier).Value = expectedIncarnation;
            var affected = command.ExecuteNonQuery();
            if (affected > 1)
            {
                throw this.InvalidMetadata();
            }

            transaction.Commit();
            return affected == 1;
        }

        public void Validate()
        {
            _ = this.Read() ?? throw this.InvalidMetadata();
            var validation = new Validation(this.database);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException($"The SQL Server population schema is invalid. Complete initialization or recovery before publishing Ready. {validation.Message}");
            }
        }

        private static string Quote(string identifier) => $"[{identifier.Replace("]", "]]")}]";

        private InvalidOperationException InvalidMetadata(Exception inner = null) =>
            new InvalidOperationException($"The population metadata in {this.TableName} is malformed. Keep serving hosts stopped and repair or renew the population through the offline maintenance workflow.", inner);
    }
}

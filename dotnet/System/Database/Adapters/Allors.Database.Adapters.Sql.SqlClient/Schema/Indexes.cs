// <copyright file="Indexes.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System.Collections.Generic;
    using System.Data;
    using Microsoft.Data.SqlClient;

    public class Indexes : Sql.Indexes
    {
        private readonly Mapping mapping;

        internal Indexes(Database database) : base(database) => this.mapping = (Mapping)database.Mapping;

        // A connection of its own, that returns to no pool: it sets a lock timeout.
        private string ConnectionString => new SqlConnectionStringBuilder(this.Database.ConnectionString) { Pooling = false }.ConnectionString;

        // The name of an index is unique in its table.
        protected override bool TakesNameOf(SchemaIndex existing, SchemaIndex index) => existing.HasNameOf(index) && existing.IsOnTableOf(index);

        protected override IReadOnlyList<SchemaIndex> Read()
        {
            using (var connection = new SqlConnection(this.Database.ConnectionString))
            {
                connection.Open();
                return this.Read(connection, null);
            }
        }

        protected override void Create(IReadOnlyList<SchemaIndex> indexes)
        {
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();
                foreach (var index in indexes)
                {
                    // A transaction per index: the writes to a table wait only while its own index is built.
                    using (var transaction = connection.BeginTransaction())
                    {
                        this.Lock(connection, transaction);
                        if (IsMissing(index, this.Read(connection, transaction)))
                        {
                            Execute(connection, transaction, this.CreateStatement(index));
                        }

                        transaction.Commit();
                    }
                }
            }
        }

        protected override void Drop(IReadOnlyList<SchemaIndex> indexes)
        {
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();

                // A drop does not queue behind a long statement on the table, with every other statement
                // behind it.
                Execute(connection, null, "SET LOCK_TIMEOUT 2000");

                foreach (var index in indexes)
                {
                    try
                    {
                        using (var transaction = connection.BeginTransaction())
                        {
                            this.Lock(connection, transaction, 2000);
                            Execute(connection, transaction, $"DROP INDEX IF EXISTS {index.Name} ON {index.TableName}");
                            transaction.Commit();
                        }
                    }
                    catch (SqlException)
                    {
                        // The index stays. The next database object tries again.
                    }
                }
            }
        }

        private static void Execute(SqlConnection connection, SqlTransaction transaction, string sql)
        {
            // An index on a table with rows takes as long as it takes.
            using (var command = new SqlCommand(sql, connection, transaction) { CommandTimeout = 0 })
            {
                command.ExecuteNonQuery();
            }
        }

        // Held until the transaction ends.
        private void Lock(SqlConnection connection, SqlTransaction transaction, int lockTimeout = -1)
        {
            const string Sql = @"
DECLARE @result int;
EXEC @result = sp_getapplock @Resource = @name, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = @lockTimeout;
IF @result < 0 THROW 50000, 'The lock for the indexes of the schema was not granted.', 1;";

            using (var command = new SqlCommand(Sql, connection, transaction) { CommandTimeout = 0 })
            {
                command.Parameters.Add("@name", SqlDbType.NVarChar, 255).Value = this.LockName;
                command.Parameters.Add("@lockTimeout", SqlDbType.Int).Value = lockTimeout;
                command.ExecuteNonQuery();
            }
        }

        private List<SchemaIndex> Read(SqlConnection connection, SqlTransaction transaction)
        {
            // An index is complete when it is a rowstore index that is enabled and has no filter. One that leads
            // with an expression has no leading column.
            const string Sql = @"
SELECT i.name, t.name, c.name, CAST(CASE WHEN i.type IN (1, 2) AND i.is_disabled = 0 AND i.is_hypothetical = 0 AND i.has_filter = 0 THEN 1 ELSE 0 END AS bit)
FROM sys.indexes i
JOIN sys.tables t ON t.object_id = i.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
LEFT JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.key_ordinal = 1
LEFT JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE s.name = @schema AND i.type > 0";

            var indexes = new List<SchemaIndex>();
            using (var command = new SqlCommand(Sql, connection, transaction))
            {
                command.Parameters.Add("@schema", SqlDbType.NVarChar).Value = this.Database.SchemaName;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        indexes.Add(new SchemaIndex(
                            reader.GetString(0),
                            this.Database.SchemaName + "." + this.mapping.NormalizeName(reader.GetString(1)),
                            reader.IsDBNull(2) ? null : this.mapping.NormalizeName(reader.GetString(2)),
                            reader.GetBoolean(3)));
                    }
                }
            }

            return indexes;
        }
    }
}

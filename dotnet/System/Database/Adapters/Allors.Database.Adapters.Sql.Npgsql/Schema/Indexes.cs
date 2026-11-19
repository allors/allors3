// <copyright file="Indexes.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System.Collections.Generic;
    using global::Npgsql;
    using NpgsqlTypes;

    public class Indexes : Sql.Indexes
    {
        private readonly Mapping mapping;

        internal Indexes(Database database) : base(database) => this.mapping = (Mapping)database.Mapping;

        // The name of an index is unique in its schema.
        protected override bool TakesNameOf(SchemaIndex existing, SchemaIndex index) => existing.HasNameOf(index);

        protected override IReadOnlyList<SchemaIndex> Read()
        {
            using (var connection = new NpgsqlConnection(this.Database.ConnectionString))
            {
                connection.Open();
                return this.Read(connection, null);
            }
        }

        protected override void Create(IReadOnlyList<SchemaIndex> indexes)
        {
            using (var connection = new NpgsqlConnection(this.Database.ConnectionString))
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
            using (var connection = new NpgsqlConnection(this.Database.ConnectionString))
            {
                connection.Open();
                foreach (var index in indexes)
                {
                    try
                    {
                        using (var transaction = connection.BeginTransaction())
                        {
                            this.Lock(connection, transaction);

                            // The drop does not queue behind a long statement on the table, with every other
                            // statement behind it.
                            Execute(connection, transaction, "SET LOCAL lock_timeout = '2s'");
                            Execute(connection, transaction, $"DROP INDEX IF EXISTS {this.Database.SchemaName}.{index.Name}");
                            transaction.Commit();
                        }
                    }
                    catch (PostgresException)
                    {
                        // The index stays. The next database object tries again.
                    }
                }
            }
        }

        private static void Execute(NpgsqlConnection connection, NpgsqlTransaction transaction, string sql)
        {
            // An index on a table with rows takes as long as it takes.
            using (var command = new NpgsqlCommand(sql, connection, transaction) { CommandTimeout = 0 })
            {
                command.ExecuteNonQuery();
            }
        }

        // Held until the transaction ends. A lock of the transaction also works behind a connection pooler.
        private void Lock(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            using (var command = new NpgsqlCommand("SELECT pg_advisory_xact_lock(hashtext(@name))", connection, transaction) { CommandTimeout = 0 })
            {
                command.Parameters.Add("@name", NpgsqlDbType.Text).Value = this.LockName;
                command.ExecuteNonQuery();
            }
        }

        private List<SchemaIndex> Read(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            // An index is complete when it can be used and has no predicate. One that leads with an expression
            // has no leading column.
            const string Sql = @"
SELECT c.relname, t.relname, a.attname, i.indisvalid AND i.indisready AND i.indislive AND i.indpred IS NULL
FROM pg_index i
JOIN pg_class c ON c.oid = i.indexrelid
JOIN pg_class t ON t.oid = i.indrelid
JOIN pg_namespace n ON n.oid = t.relnamespace
LEFT JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = i.indkey[0]
WHERE n.nspname = @schema";

            var indexes = new List<SchemaIndex>();
            using (var command = new NpgsqlCommand(Sql, connection, transaction))
            {
                command.Parameters.Add("@schema", NpgsqlDbType.Text).Value = this.Database.SchemaName;
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

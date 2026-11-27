// <copyright file="Schema.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Collections.Generic;
    using global::Npgsql;

    using NpgsqlTypes;

    public class Schema
    {
        public readonly bool Exists;

        public Schema(Database database)
        {
            var mapping = (Mapping)database.Mapping;

            this.Database = database;
            this.TableByName = new Dictionary<string, SchemaTable>();

            using (var connection = new NpgsqlConnection(database.ConnectionString))
            {
                connection.Open();
                try
                {
                    // The schema is read from the catalogue of PostgreSQL, under the conditions of the views
                    // schemata, tables and columns of information_schema: what is read is what those views
                    // show. A query that joins the views can take PostgreSQL through the columns of the whole
                    // schema once for every table.

                    // Schema: one that the login owns or may use.
                    var cmdText = @"
SELECT  count(*)
FROM    pg_catalog.pg_namespace n
WHERE   n.nspname = @schemaName
AND     (pg_has_role(n.nspowner, 'USAGE') OR has_schema_privilege(n.oid, 'CREATE, USAGE'))";
                    using (var command = new NpgsqlCommand(cmdText, connection))
                    {
                        command.Parameters.Add("@schemaName", NpgsqlDbType.Varchar).Value = database.SchemaName;
                        var schemaCount = (long)command.ExecuteScalar();
                        this.Exists = schemaCount != 0;
                    }

                    // Tables: the base tables with their columns, where the login owns the table or may touch
                    // the column. In tm are the type of a column and its modifier, and for a column of a domain
                    // those of the type that the domain is made of: such a column shows that type. An array
                    // and a type outside pg_catalog show their kind only. The numbers are the ids of the types
                    // that have a length, a precision or a scale: 1042 char, 1043 varchar, 1560 bit, 1562
                    // varbit, 21 smallint, 23 integer, 20 bigint, 1700 numeric, 700 real, 701 double precision.
                    // The primary key lists its key columns in order. Included columns have no position in it.
                    cmdText = @"
SELECT c.relname AS table_name,
       a.attname AS column_name,
       COALESCE(cardinality(pk.conkey), 0) AS primary_key_column_count,
       COALESCE(array_position(pk.conkey, a.attnum), 0) AS primary_key_ordinal,
       CASE WHEN t.typtype = 'd' THEN
                CASE WHEN bt.typelem <> 0 AND bt.typlen = -1 THEN 'ARRAY'
                     WHEN nbt.nspname = 'pg_catalog' THEN format_type(t.typbasetype, NULL)
                     ELSE 'USER-DEFINED' END
            ELSE
                CASE WHEN t.typelem <> 0 AND t.typlen = -1 THEN 'ARRAY'
                     WHEN nt.nspname = 'pg_catalog' THEN format_type(a.atttypid, NULL)
                     ELSE 'USER-DEFINED' END
       END AS data_type,
       CASE WHEN tm.typmod = -1 THEN NULL
            WHEN tm.typid IN (1042, 1043) THEN tm.typmod - 4
            WHEN tm.typid IN (1560, 1562) THEN tm.typmod
       END AS character_maximum_length,
       CASE tm.typid
            WHEN 21 THEN 16
            WHEN 23 THEN 32
            WHEN 20 THEN 64
            WHEN 1700 THEN CASE WHEN tm.typmod = -1 THEN NULL ELSE ((tm.typmod - 4) >> 16) & 65535 END
            WHEN 700 THEN 24
            WHEN 701 THEN 53
       END AS numeric_precision,
       CASE WHEN tm.typid IN (21, 23, 20) THEN 0
            WHEN tm.typid = 1700 THEN CASE WHEN tm.typmod = -1 THEN NULL ELSE (tm.typmod - 4) & 65535 END
       END AS numeric_scale
FROM pg_catalog.pg_namespace nc
JOIN pg_catalog.pg_class c ON c.relnamespace = nc.oid
JOIN pg_catalog.pg_attribute a ON a.attrelid = c.oid
LEFT JOIN pg_catalog.pg_constraint pk ON pk.conrelid = c.oid AND pk.contype = 'p'
JOIN pg_catalog.pg_type t ON t.oid = a.atttypid
JOIN pg_catalog.pg_namespace nt ON nt.oid = t.typnamespace
LEFT JOIN (pg_catalog.pg_type bt JOIN pg_catalog.pg_namespace nbt ON nbt.oid = bt.typnamespace)
    ON t.typtype = 'd' AND t.typbasetype = bt.oid
CROSS JOIN LATERAL (SELECT CASE WHEN t.typtype = 'd' THEN t.typbasetype ELSE a.atttypid END AS typid,
                           CASE WHEN t.typtype = 'd' THEN t.typtypmod ELSE a.atttypmod END AS typmod) tm
WHERE nc.nspname = @tableSchema
AND c.relkind IN ('r', 'p')
AND nc.oid <> pg_my_temp_schema()
AND NOT pg_is_other_temp_schema(nc.oid)
AND (pg_has_role(c.relowner, 'USAGE')
     OR has_table_privilege(c.oid, 'SELECT, INSERT, UPDATE, DELETE, TRUNCATE, REFERENCES, TRIGGER')
     OR has_any_column_privilege(c.oid, 'SELECT, INSERT, UPDATE, REFERENCES'))
AND a.attnum > 0
AND NOT a.attisdropped
AND (pg_has_role(c.relowner, 'USAGE')
     OR has_column_privilege(c.oid, a.attnum, 'SELECT, INSERT, UPDATE, REFERENCES'))";

                    using (var command = new NpgsqlCommand(cmdText, connection))
                    {
                        command.Parameters.Add("@tableSchema", NpgsqlDbType.Varchar).Value = database.SchemaName;
                        using (var reader = command.ExecuteReader())
                        {
                            var tableNameOrdinal = reader.GetOrdinal("table_name");
                            var columnNameOrdinal = reader.GetOrdinal("column_name");
                            var primaryKeyColumnCountOrdinal = reader.GetOrdinal("primary_key_column_count");
                            var primaryKeyOrdinal = reader.GetOrdinal("primary_key_ordinal");
                            var dataTypeOrdinal = reader.GetOrdinal("data_type");
                            var characterMaximumLengthOrdinal = reader.GetOrdinal("character_maximum_length");
                            var numericPrecisionOrdinal = reader.GetOrdinal("numeric_precision");
                            var numericScaleOrdinal = reader.GetOrdinal("numeric_scale");

                            while (reader.Read())
                            {
                                var tableName = reader.GetString(tableNameOrdinal);
                                var columnName = reader.GetString(columnNameOrdinal);
                                var dataType = reader.GetString(dataTypeOrdinal).Trim().ToLowerInvariant();
                                var characterMaximumLength = reader.IsDBNull(characterMaximumLengthOrdinal) ? (int?)null : Convert.ToInt32(reader.GetValue(characterMaximumLengthOrdinal));
                                var numericPrecision = reader.IsDBNull(numericPrecisionOrdinal) ? (int?)null : Convert.ToInt32(reader.GetValue(numericPrecisionOrdinal));
                                var numericScale = reader.IsDBNull(numericScaleOrdinal) ? (int?)null : Convert.ToInt32(reader.GetValue(numericScaleOrdinal));

                                tableName = tableName.Trim().ToLowerInvariant();
                                tableName = mapping.NormalizeName(tableName);
                                var fullyQualifiedTableName = database.SchemaName + "." + tableName;

                                columnName = mapping.NormalizeName(columnName);

                                if (!this.TableByName.TryGetValue(fullyQualifiedTableName, out var table))
                                {
                                    table = new SchemaTable(this, fullyQualifiedTableName)
                                    {
                                        PrimaryKeyColumnCount = reader.GetInt32(primaryKeyColumnCountOrdinal),
                                    };
                                    this.TableByName[fullyQualifiedTableName] = table;
                                }

                                if (!reader.IsDBNull(columnNameOrdinal))
                                {
                                    var column = new SchemaTableColumn(table, columnName, dataType, characterMaximumLength, numericPrecision, numericScale)
                                    {
                                        PrimaryKeyOrdinal = reader.GetInt32(primaryKeyOrdinal),
                                    };
                                    table.ColumnByLowercaseColumnName[column.LowercaseName] = column;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public Database Database { get; }

        public Dictionary<string, SchemaTable> TableByName { get; }

        public SchemaTable GetTable(string tableName)
        {
            this.TableByName.TryGetValue(tableName.ToLowerInvariant(), out var table);
            return table;
        }
    }
}

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
                    // Schema
                    var cmdText = @"
SELECT  count(schema_name)
FROM    information_schema.schemata
WHERE   schema_name = @schemaName";
                    using (var command = new NpgsqlCommand(cmdText, connection))
                    {
                        command.Parameters.Add("@schemaName", NpgsqlDbType.Varchar).Value = database.SchemaName;
                        var schemaCount = (long)command.ExecuteScalar();
                        this.Exists = schemaCount != 0;
                    }

                    // Tables
                    cmdText = @"
SELECT T.table_name,
       C.column_name,
       C.data_type,
       C.character_maximum_length,
       C.numeric_precision,
       C.numeric_scale
FROM information_schema.tables AS T
FULL OUTER JOIN information_schema.columns AS C
ON T.table_name = C.table_name
WHERE T.table_type = 'BASE TABLE'
AND T.table_schema = @tableSchema
AND C.table_schema = @tableSchema";

                    using (var command = new NpgsqlCommand(cmdText, connection))
                    {
                        command.Parameters.Add("@tableSchema", NpgsqlDbType.Varchar).Value = database.SchemaName;
                        using (var reader = command.ExecuteReader())
                        {
                            var tableNameOrdinal = reader.GetOrdinal("table_name");
                            var columnNameOrdinal = reader.GetOrdinal("column_name");
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
                                    table = new SchemaTable(this, fullyQualifiedTableName);
                                    this.TableByName[fullyQualifiedTableName] = table;
                                }

                                if (!reader.IsDBNull(columnNameOrdinal))
                                {
                                    var column = new SchemaTableColumn(table, columnName, dataType, characterMaximumLength, numericPrecision, numericScale);
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

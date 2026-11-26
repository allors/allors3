// <copyright file="Initialization.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System.Collections.Generic;
    using System.Text;
    using global::Npgsql;
    using NpgsqlTypes;

    public class Initialization
    {
        private readonly Database database;
        private readonly Mapping mapping;

        private Validation validation;

        internal Initialization(Database database)
        {
            this.database = database;
            this.mapping = (Mapping)database.Mapping;
        }

        internal bool Execute(bool ensureIndexes)
        {
            this.validation = new Validation(this.database);

            if (this.validation.IsValid)
            {
                this.TruncateTables(ensureIndexes);
            }
            else
            {
                try
                {
                    this.CreateSchema();

                    this.DropTables();

                    this.CreateTables();
                }
                finally
                {
                    // A rebuild can change the result types of prepared statements, even if it fails part way through.
                    using (var connection = new NpgsqlConnection(this.database.ConnectionString))
                    {
                        NpgsqlConnection.ClearPool(connection);
                    }
                }
            }

            return !this.validation.IsValid;
        }

        private void CreateSchema()
        {
            if (!this.validation.Schema.Exists)
            {
                // CREATE SCHEMA must be in its own batch
                using (var connection = new NpgsqlConnection(this.database.ConnectionString))
                {
                    connection.Open();
                    try
                    {
                        var cmdText = @"
CREATE SCHEMA " + this.database.SchemaName;
                        using (var command = new NpgsqlCommand(cmdText, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        // Empties the tables in one transaction: when it fails, it has changed nothing. A truncate gives a
        // table a new file, also when the table is empty, so only the tables that may hold data are truncated.
        private void TruncateTables(bool ensureIndexes)
        {
            // The tables that are not known to be empty. The size of its file says that a table holds no row
            // only for a table of the heap access method without children: the rows of a partitioned table
            // and of a table with a child are in the files of other tables, and another access method keeps
            // its rows where it chooses.
            const string MayHoldData = @"
SELECT t.name
FROM unnest(@names) AS t(name)
WHERE NOT EXISTS (
    SELECT 1
    FROM pg_class c
    WHERE c.oid = t.name::regclass
    AND NOT c.relhassubclass
    AND c.relam = (SELECT oid FROM pg_am WHERE amname = 'heap')
    AND pg_relation_size(c.oid) = 0)";

            var tableNames = new List<string> { this.mapping.TableNameForObjects };

            foreach (var @class in this.mapping.Database.MetaPopulation.DatabaseClasses)
            {
                tableNames.Add(this.mapping.TableNameForObjectByClass[@class]);
            }

            foreach (var relationType in this.mapping.Database.MetaPopulation.DatabaseRelationTypes)
            {
                var associationType = relationType.AssociationType;
                var roleType = relationType.RoleType;

                if (!roleType.ObjectType.IsUnit && ((associationType.IsMany && roleType.IsMany) || !relationType.ExistExclusiveDatabaseClasses))
                {
                    tableNames.Add(this.mapping.TableNameForRelationByRelationType[relationType]);
                }
            }

            using (var connection = new NpgsqlConnection(this.database.ConnectionString))
            {
                connection.Open();
                try
                {
                    var indexes = ensureIndexes ? new Indexes(this.database) : null;
                    SchemaIndex[] leftovers;
                    using (var transaction = connection.BeginTransaction())
                    {
                        // Every table is locked before the look at its size: no other transaction then holds a
                        // row in it that is not committed, or writes to it before the truncate.
                        var lockText = "LOCK TABLE " + string.Join(", ", tableNames) + " IN ACCESS EXCLUSIVE MODE";
                        using (var command = new NpgsqlCommand(lockText, connection, transaction))
                        {
                            indexes?.Lock(connection, transaction, commandTimeout: command.CommandTimeout);
                            command.ExecuteNonQuery();
                        }

                        // The objects table numbers the objects. It is truncated whatever it holds: the numbering
                        // starts again, as it does when the table is created.
                        var truncate = new List<string> { this.mapping.TableNameForObjects };

                        using (var command = new NpgsqlCommand(MayHoldData, connection, transaction))
                        {
                            command.Parameters.Add("@names", NpgsqlDbType.Array | NpgsqlDbType.Text).Value = tableNames.GetRange(1, tableNames.Count - 1).ToArray();
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    truncate.Add(reader.GetString(0));
                                }
                            }
                        }

                        var truncateText = "TRUNCATE TABLE " + string.Join(", ", truncate) + " RESTART IDENTITY";
                        using (var command = new NpgsqlCommand(truncateText, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        leftovers = indexes?.CreateMissing(connection, transaction);
                        transaction.Commit();
                    }

                    if (leftovers is { Length: > 0 })
                    {
                        indexes.Drop(connection, leftovers);
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void DropTables()
        {
            using (var connection = new NpgsqlConnection(this.database.ConnectionString))
            {
                connection.Open();
                try
                {
                    this.DropTable(connection, this.mapping.TableNameForObjects);

                    foreach (var @class in this.mapping.Database.MetaPopulation.DatabaseClasses)
                    {
                        var tableName = this.mapping.TableNameForObjectByClass[@class];

                        this.DropTable(connection, tableName);
                    }

                    foreach (var relationType in this.mapping.Database.MetaPopulation.DatabaseRelationTypes)
                    {
                        var associationType = relationType.AssociationType;
                        var roleType = relationType.RoleType;

                        if (!roleType.ObjectType.IsUnit && ((associationType.IsMany && roleType.IsMany) || !relationType.ExistExclusiveDatabaseClasses))
                        {
                            var tableName = this.mapping.TableNameForRelationByRelationType[relationType];
                            this.DropTable(connection, tableName);
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void CreateTables()
        {
            using (var connection = new NpgsqlConnection(this.database.ConnectionString))
            {
                connection.Open();
                try
                {
                    {
                        var sql = new StringBuilder();
                        sql.Append("CREATE TABLE " + this.mapping.TableNameForObjects + "\n");
                        sql.Append("(\n");
                        sql.Append(Mapping.ColumnNameForObject + " BIGINT GENERATED BY DEFAULT AS IDENTITY CONSTRAINT " + Mapping.PrimaryKeyNameForObjects + " PRIMARY KEY,\n");
                        sql.Append(Mapping.ColumnNameForClass + " " + Mapping.SqlTypeForClass + ",\n");
                        sql.Append(Mapping.ColumnNameForVersion + " " + Mapping.SqlTypeForVersion + "\n");
                        sql.Append(")\n");

                        using (var command = new NpgsqlCommand(sql.ToString(), connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    foreach (var @class in this.mapping.Database.MetaPopulation.DatabaseClasses)
                    {
                        var tableName = this.mapping.TableNameForObjectByClass[@class];

                        var sql = new StringBuilder();
                        sql.Append("CREATE TABLE " + tableName + "\n");
                        sql.Append("(\n");
                        sql.Append(Mapping.ColumnNameForObject + " " + Mapping.SqlTypeForObject + " CONSTRAINT " + Mapping.PrimaryKeyNameFor(@class) + " PRIMARY KEY,\n");
                        sql.Append(Mapping.ColumnNameForClass + " " + Mapping.SqlTypeForClass);

                        foreach (var associationType in @class.DatabaseAssociationTypes)
                        {
                            var relationType = associationType.RelationType;
                            var roleType = relationType.RoleType;
                            if (!(associationType.IsMany && roleType.IsMany) && relationType.ExistExclusiveDatabaseClasses && roleType.IsMany)
                            {
                                sql.Append(",\n" + this.mapping.ColumnNameByRelationType[relationType] + " " + Mapping.SqlTypeForObject);
                            }
                        }

                        foreach (var roleType in @class.DatabaseRoleTypes)
                        {
                            var relationType = roleType.RelationType;
                            var associationType3 = relationType.AssociationType;
                            if (roleType.ObjectType.IsUnit)
                            {
                                sql.Append(",\n" + this.mapping.ColumnNameByRelationType[relationType] + " " + this.mapping.GetSqlType(roleType));
                            }
                            else if (!(associationType3.IsMany && roleType.IsMany) && relationType.ExistExclusiveDatabaseClasses && !roleType.IsMany)
                            {
                                sql.Append(",\n" + this.mapping.ColumnNameByRelationType[relationType] + " " + Mapping.SqlTypeForObject);
                            }
                        }

                        sql.Append(")\n");

                        using (var command = new NpgsqlCommand(sql.ToString(), connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    foreach (var relationType in this.mapping.Database.MetaPopulation.DatabaseRelationTypes)
                    {
                        var associationType = relationType.AssociationType;
                        var roleType = relationType.RoleType;

                        if (!roleType.ObjectType.IsUnit && ((associationType.IsMany && roleType.IsMany) || !relationType.ExistExclusiveDatabaseClasses))
                        {
                            var tableName = this.mapping.TableNameForRelationByRelationType[relationType];

                            var primaryKeyName = Mapping.PrimaryKeyNameFor(relationType);

                            var sql =
$@"CREATE TABLE {tableName}(
    {Mapping.ColumnNameForAssociation} {Mapping.SqlTypeForObject},
    {Mapping.ColumnNameForRole} {Mapping.SqlTypeForObject},
    {(relationType.RoleType.IsOne
          ? $"CONSTRAINT {primaryKeyName} PRIMARY KEY ({Mapping.ColumnNameForAssociation})\n"
          : $"CONSTRAINT {primaryKeyName} PRIMARY KEY ({Mapping.ColumnNameForAssociation}, {Mapping.ColumnNameForRole})\n")}

)";

                            using (var command = new NpgsqlCommand(sql, connection))
                            {
                                command.ExecuteNonQuery();
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

        private void DropTable(NpgsqlConnection connection, string tableName)
        {
            if (!this.validation.MissingTableNames.Contains(tableName))
            {
                using (var command = new NpgsqlCommand("DROP TABLE " + tableName, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}

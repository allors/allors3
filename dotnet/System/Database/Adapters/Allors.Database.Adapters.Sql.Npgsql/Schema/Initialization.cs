// <copyright file="Initialization.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System.Text;
    using global::Npgsql;

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

        internal void Execute()
        {
            this.validation = new Validation(this.database);

            if (this.validation.IsValid)
            {
                this.TruncateTables();
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

        private void TruncateTables()
        {
            using (var connection = new NpgsqlConnection(this.database.ConnectionString))
            {
                connection.Open();
                try
                {
                    // The object table numbers the objects. The numbering starts again, as it does when the
                    // table is created.
                    this.TruncateTable(connection, this.mapping.TableNameForObjects, restartIdentity: true);

                    foreach (var @class in this.mapping.Database.MetaPopulation.DatabaseClasses)
                    {
                        var tableName = this.mapping.TableNameForObjectByClass[@class];

                        this.TruncateTable(connection, tableName);
                    }

                    foreach (var relationType in this.mapping.Database.MetaPopulation.DatabaseRelationTypes)
                    {
                        var associationType = relationType.AssociationType;
                        var roleType = relationType.RoleType;

                        if (!roleType.ObjectType.IsUnit && ((associationType.IsMany && roleType.IsMany) || !relationType.ExistExclusiveDatabaseClasses))
                        {
                            var tableName = this.mapping.TableNameForRelationByRelationType[relationType];
                            this.TruncateTable(connection, tableName);
                        }
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
                        sql.Append(Mapping.ColumnNameForObject + " BIGINT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,\n");
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
                        sql.Append(Mapping.ColumnNameForObject + " " + Mapping.SqlTypeForObject + " PRIMARY KEY,\n");
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

                            var primaryKeyName = $"pk_{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

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

        private void TruncateTable(NpgsqlConnection connection, string tableName, bool restartIdentity = false)
        {
            var cmdText = @"TRUNCATE TABLE " + tableName + (restartIdentity ? " RESTART IDENTITY" : string.Empty) + @";";
            using (var command = new NpgsqlCommand(cmdText, connection))
            {
                command.ExecuteNonQuery();
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

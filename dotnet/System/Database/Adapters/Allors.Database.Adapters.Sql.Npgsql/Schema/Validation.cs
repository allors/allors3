// <copyright file="Validation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System.Collections.Generic;
    using System.Text;
    using static Mapping;

    public class Validation : Sql.Validation
    {
        public HashSet<string> MissingTableNames { get; }
        public HashSet<SchemaTable> InvalidTables { get; }

        private readonly Mapping mapping;

        public Validation(Database database)
        {
            this.Database = database;
            this.mapping = (Mapping)database.Mapping;
            this.Schema = new Schema(database);

            this.MissingTableNames = new HashSet<string>();
            this.InvalidTables = new HashSet<SchemaTable>();

            this.Validate();

            this.IsValid =
                this.MissingTableNames.Count == 0 &&
                this.InvalidTables.Count == 0;
        }

        public override bool IsValid { get; }

        public Database Database { get; }

        public Schema Schema { get; }

        public override string Message
        {
            get
            {
                var message = new StringBuilder();

                if (this.MissingTableNames.Count > 0)
                {
                    message.Append("Missing Tables:\n");
                    foreach (var missingTable in this.MissingTableNames)
                    {
                        message.Append("- " + missingTable + ": " + this.mapping.Describe(missingTable) + "\n");
                    }
                }

                if (this.InvalidTables.Count > 0)
                {
                    message.Append("Invalid Tables:\n");
                    foreach (var invalidTable in this.InvalidTables)
                    {
                        message.Append("- " + invalidTable.Name + ": " + this.mapping.Describe(invalidTable.Name) + "\n");
                    }
                }

                return message.ToString();
            }
        }

        private void Validate()
        {
            // Objects Table
            var objectsTable = this.Schema.GetTable(this.Database.Mapping.TableNameForObjects);
            if (objectsTable == null)
            {
                this.MissingTableNames.Add(this.mapping.TableNameForObjects);
            }
            else
            {
                if (objectsTable.ColumnByLowercaseColumnName.Count != 3)
                {
                    this.InvalidTables.Add(objectsTable);
                }

                this.ValidateColumn(objectsTable, Sql.Mapping.ColumnNameForObject, SqlTypeForObject);
                this.ValidateColumn(objectsTable, Sql.Mapping.ColumnNameForClass, SqlTypeForClass);
                this.ValidateColumn(objectsTable, Sql.Mapping.ColumnNameForVersion, SqlTypeForVersion);
            }

            // Object Tables
            foreach (var @class in this.Database.MetaPopulation.DatabaseClasses)
            {
                var tableName = this.mapping.TableNameForObjectByClass[@class];
                var table = this.Schema.GetTable(tableName);

                if (table == null)
                {
                    this.MissingTableNames.Add(tableName);
                }
                else
                {
                    this.ValidateColumn(table, Sql.Mapping.ColumnNameForObject, SqlTypeForObject);
                    this.ValidateColumn(table, Sql.Mapping.ColumnNameForClass, SqlTypeForClass);

                    foreach (var associationType in @class.DatabaseAssociationTypes)
                    {
                        var relationType = associationType.RelationType;
                        var roleType = relationType.RoleType;
                        if (!(associationType.IsMany && roleType.IsMany) && relationType.ExistExclusiveDatabaseClasses
                            && roleType.IsMany)
                        {
                            this.ValidateColumn(
                                table,
                                this.Database.Mapping.ColumnNameByRelationType[relationType],
                                SqlTypeForObject);
                        }
                    }

                    foreach (var roleType in @class.DatabaseRoleTypes)
                    {
                        var relationType = roleType.RelationType;
                        var associationType = relationType.AssociationType;
                        if (roleType.ObjectType.IsUnit)
                        {
                            this.ValidateColumn(
                                table,
                                this.Database.Mapping.ColumnNameByRelationType[relationType],
                                this.mapping.GetSqlType(relationType.RoleType));
                        }
                        else if (!(associationType.IsMany && roleType.IsMany) && relationType.ExistExclusiveDatabaseClasses && !roleType.IsMany)
                        {
                            this.ValidateColumn(
                                table,
                                this.Database.Mapping.ColumnNameByRelationType[relationType],
                                SqlTypeForObject);
                        }
                    }
                }
            }

            // Relation Tables
            foreach (var relationType in this.Database.MetaPopulation.DatabaseRelationTypes)
            {
                var associationType = relationType.AssociationType;
                var roleType = relationType.RoleType;

                if (!roleType.ObjectType.IsUnit &&
                    ((associationType.IsMany && roleType.IsMany) || !relationType.ExistExclusiveDatabaseClasses))
                {
                    var tableName = this.mapping.TableNameForRelationByRelationType[relationType];
                    var table = this.Schema.GetTable(tableName);

                    if (table == null)
                    {
                        this.MissingTableNames.Add(tableName);
                    }
                    else
                    {
                        var associationColumn = this.ValidateColumn(table, Sql.Mapping.ColumnNameForAssociation, SqlTypeForObject);
                        var roleSqlType = relationType.RoleType.ObjectType.IsComposite ? SqlTypeForObject : this.mapping.GetSqlType(relationType.RoleType);
                        var roleColumn = this.ValidateColumn(table, Sql.Mapping.ColumnNameForRole, roleSqlType);
                        if (table.ColumnByLowercaseColumnName.Count != 2 ||
                            !HasRelationPrimaryKey(roleType, table.PrimaryKeyColumnCount, associationColumn?.PrimaryKeyOrdinal, roleColumn?.PrimaryKeyOrdinal))
                        {
                            this.InvalidTables.Add(table);
                        }
                    }
                }
            }

            // TODO: Validate the primary keys of the objects table and the class tables.
        }

        private SchemaTableColumn ValidateColumn(SchemaTable table, string columnName, string sqlType)
        {
            var normalizedColumnName = this.mapping.NormalizeName(columnName);
            var objectColumn = table.GetColumn(normalizedColumnName);

            if (objectColumn?.SqlType.Equals(sqlType) != true)
            {
                this.InvalidTables.Add(table);
            }

            return objectColumn;
        }
    }
}

// <copyright file="Validation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System.Collections.Generic;
    using System.Text;
    using static Mapping;

    public class Validation
    {
        public HashSet<string> MissingTableNames { get; }
        public HashSet<SchemaTable> InvalidTables { get; }

        public HashSet<string> MissingTableTypeNames { get; }
        public HashSet<SchemaTableType> InvalidTableTypes { get; }

        public HashSet<string> MissingProcedureNames { get; }
        public HashSet<SchemaProcedure> InvalidProcedures { get; }

        private readonly Mapping mapping;

        public Validation(Database database)
        {
            this.Database = database;
            this.mapping = (Mapping)database.Mapping;
            this.Schema = new Schema(database);

            this.MissingTableNames = new HashSet<string>();
            this.InvalidTables = new HashSet<SchemaTable>();

            this.MissingTableTypeNames = new HashSet<string>();
            this.InvalidTableTypes = new HashSet<SchemaTableType>();

            this.MissingProcedureNames = new HashSet<string>();
            this.InvalidProcedures = new HashSet<SchemaProcedure>();

            this.Validate();

            this.IsValid =
                this.MissingTableNames.Count == 0 &&
                this.InvalidTables.Count == 0 &&
                this.MissingTableTypeNames.Count == 0 &&
                this.InvalidTableTypes.Count == 0 &&
                this.MissingProcedureNames.Count == 0 &&
                this.InvalidProcedures.Count == 0;
        }

        public bool IsValid { get; }

        public Database Database { get; }

        public Schema Schema { get; }

        public string Message
        {
            get
            {
                var message = new StringBuilder();

                if (this.MissingTableNames.Count > 0)
                {
                    message.Append("Missing Tables:\n");
                    foreach (var missingTable in this.MissingTableNames)
                    {
                        message.Append("- " + missingTable + ":\n");
                    }
                }

                if (this.InvalidTables.Count > 0)
                {
                    message.Append("Invalid Tables:\n");
                    foreach (var invalidTable in this.InvalidTables)
                    {
                        message.Append("- " + invalidTable.Name + ":\n");
                    }
                }

                if (this.MissingTableTypeNames.Count > 0)
                {
                    message.Append("Missing Table Types:\n");
                    foreach (var missingTableType in this.MissingTableTypeNames)
                    {
                        message.Append("- " + missingTableType + ":\n");
                    }
                }

                if (this.InvalidTableTypes.Count > 0)
                {
                    message.Append("Invalid Table Types:\n");
                    foreach (var invalidTableType in this.InvalidTableTypes)
                    {
                        message.Append("- " + invalidTableType.Name + ":\n");
                    }
                }

                if (this.MissingProcedureNames.Count > 0)
                {
                    message.Append("Missing Procedures:\n");
                    foreach (var missingProcedure in this.MissingProcedureNames)
                    {
                        message.Append("- " + missingProcedure + ":\n");
                    }
                }

                if (this.InvalidProcedures.Count > 0)
                {
                    message.Append("Invalid Procedures:\n");
                    foreach (var invalidProcedure in this.InvalidProcedures)
                    {
                        message.Append("- " + invalidProcedure.Name + ":\n");
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
                        if (table.ColumnByLowercaseColumnName.Count != 2)
                        {
                            this.InvalidTables.Add(table);
                        }

                        this.ValidateColumn(table, Sql.Mapping.ColumnNameForAssociation, SqlTypeForObject);

                        var roleSqlType = relationType.RoleType.ObjectType.IsComposite ? SqlTypeForObject : this.mapping.GetSqlType(relationType.RoleType);
                        this.ValidateColumn(table, Sql.Mapping.ColumnNameForRole, roleSqlType);
                    }
                }
            }

            // Procedures Tables
            foreach (var dictionaryEntry in this.mapping.ProcedureDefinitionByName)
            {
                var procedureName = dictionaryEntry.Key;
                var procedureDefinition = dictionaryEntry.Value;

                var procedure = this.Schema.GetProcedure(procedureName);

                if (procedure == null)
                {
                    this.MissingProcedureNames.Add(procedureName);
                }
                else if (!procedure.IsDefinitionCompatible(procedureDefinition))
                {
                    this.InvalidProcedures.Add(procedure);
                }
            }

            // TODO: Primary Keys and Indeces
        }

        private void ValidateColumn(SchemaTable table, string columnName, string sqlType)
        {
            var normalizedColumnName = this.mapping.NormalizeName(columnName);
            var objectColumn = table.GetColumn(normalizedColumnName);

            if (objectColumn?.SqlType.Equals(sqlType) != true)
            {
                this.InvalidTables.Add(table);
            }
        }

        private void ValidateColumn(SchemaTableType tableType, string columnName, string sqlType)
        {
            var objectColumn = tableType.GetColumn(columnName);

            if (objectColumn?.SqlType.Equals(sqlType) != true)
            {
                this.InvalidTableTypes.Add(tableType);
            }
        }
    }
}

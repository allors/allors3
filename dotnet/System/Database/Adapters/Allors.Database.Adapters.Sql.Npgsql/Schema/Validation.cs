// <copyright file="Validation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using static Mapping;

    public class Validation : Sql.Validation
    {
        public HashSet<string> MissingTableNames { get; }
        public HashSet<SchemaTable> InvalidTables { get; }

        /// <summary>
        /// The tables of the schema whose names start with the prefix of the adapter and that the model lacks, by
        /// their names as PostgreSQL stores them, without the schema.
        /// </summary>
        public HashSet<string> ObsoleteTableNames { get; }

        private readonly Mapping mapping;

        // The lines of the message for each invalid table: its columns, and its primary key.
        private readonly Dictionary<SchemaTable, List<string>> linesByInvalidTable;

        public Validation(Database database)
        {
            this.Database = database;
            this.mapping = (Mapping)database.Mapping;
            this.Schema = new Schema(database);

            this.MissingTableNames = new HashSet<string>();
            this.InvalidTables = new HashSet<SchemaTable>();
            this.ObsoleteTableNames = new HashSet<string>(StringComparer.Ordinal);
            this.linesByInvalidTable = new Dictionary<SchemaTable, List<string>>();

            this.Validate();

            this.IsValid =
                this.MissingTableNames.Count == 0 &&
                this.InvalidTables.Count == 0 &&
                this.ObsoleteTableNames.Count == 0;
        }

        public override bool IsValid { get; }

        public Database Database { get; }

        public Schema Schema { get; }

        public override string Message
        {
            get
            {
                var message = new StringBuilder();

                Append(message, "Missing Tables", this.MissingTableNames.Select(v => (v, this.mapping.Describe(v), Enumerable.Empty<string>())));
                Append(message, "Obsolete Tables", this.ObsoleteTableNames.OrderBy(v => v, StringComparer.Ordinal).Select(v => (this.Database.SchemaName + "." + v, DescribeObsoleteTable(this.Database.MetaPopulation, v), Enumerable.Empty<string>())));
                Append(message, "Invalid Tables", this.InvalidTables.Select(v => (v.Name, this.mapping.Describe(v.Name), (IEnumerable<string>)this.linesByInvalidTable[v])));

                return message.ToString();
            }
        }

        private void Validate()
        {
            var metaPopulation = this.Database.MetaPopulation;

            // The names of the tables of the model, without the schema.
            var tableNames = new HashSet<string>(StringComparer.Ordinal) { Mapping.ObjectsTableName };

            // Objects Table
            var objectsTable = this.Schema.GetTable(this.mapping.TableNameForObjects);
            if (objectsTable == null)
            {
                this.MissingTableNames.Add(this.mapping.TableNameForObjects);
            }
            else
            {
                this.ValidateColumns(
                    objectsTable,
                    new[]
                    {
                        (Sql.Mapping.ColumnNameForObject, SqlTypeForObject, DescribeObjectsTableColumn(Sql.Mapping.ColumnNameForObject)),
                        (Sql.Mapping.ColumnNameForClass, SqlTypeForClass, DescribeObjectsTableColumn(Sql.Mapping.ColumnNameForClass)),
                        (Sql.Mapping.ColumnNameForVersion, SqlTypeForVersion, DescribeObjectsTableColumn(Sql.Mapping.ColumnNameForVersion)),
                    },
                    DescribeObjectsTableColumn);
            }

            // Object Tables
            foreach (var @class in metaPopulation.DatabaseClasses)
            {
                var tableName = this.mapping.TableNameForObjectByClass[@class];
                tableNames.Add(Mapping.TableNameFor(@class));
                var table = this.Schema.GetTable(tableName);

                if (table == null)
                {
                    this.MissingTableNames.Add(tableName);
                }
                else
                {
                    var columns = new List<(string, string, string)>
                    {
                        (Sql.Mapping.ColumnNameForObject, SqlTypeForObject, DescribeObjectsTableColumn(Sql.Mapping.ColumnNameForObject)),
                        (Sql.Mapping.ColumnNameForClass, SqlTypeForClass, DescribeObjectsTableColumn(Sql.Mapping.ColumnNameForClass)),
                    };

                    foreach (var associationType in @class.DatabaseAssociationTypes)
                    {
                        var relationType = associationType.RelationType;
                        var roleType = relationType.RoleType;
                        if (!(associationType.IsMany && roleType.IsMany) && relationType.ExistExclusiveDatabaseClasses
                            && roleType.IsMany)
                        {
                            columns.Add((this.mapping.ColumnNameByRelationType[relationType], SqlTypeForObject, DescribeAssociation(relationType)));
                        }
                    }

                    foreach (var roleType in @class.DatabaseRoleTypes)
                    {
                        var relationType = roleType.RelationType;
                        var associationType = relationType.AssociationType;
                        if (roleType.ObjectType.IsUnit)
                        {
                            columns.Add((this.mapping.ColumnNameByRelationType[relationType], this.mapping.GetSqlType(relationType.RoleType), DescribeRole(relationType)));
                        }
                        else if (!(associationType.IsMany && roleType.IsMany) && relationType.ExistExclusiveDatabaseClasses && !roleType.IsMany)
                        {
                            columns.Add((this.mapping.ColumnNameByRelationType[relationType], SqlTypeForObject, DescribeRole(relationType)));
                        }
                    }

                    this.ValidateColumns(table, columns, v => DescribeObsoleteClassTableColumn(metaPopulation, v));
                }
            }

            // Relation Tables
            foreach (var relationType in metaPopulation.DatabaseRelationTypes)
            {
                var associationType = relationType.AssociationType;
                var roleType = relationType.RoleType;

                if (!roleType.ObjectType.IsUnit &&
                    ((associationType.IsMany && roleType.IsMany) || !relationType.ExistExclusiveDatabaseClasses))
                {
                    var tableName = this.mapping.TableNameForRelationByRelationType[relationType];
                    tableNames.Add(Mapping.TableNameFor(relationType));
                    var table = this.Schema.GetTable(tableName);

                    if (table == null)
                    {
                        this.MissingTableNames.Add(tableName);
                    }
                    else
                    {
                        var associationColumnName = this.mapping.NormalizeName(Mapping.RelationTableAssociationColumnNameFor(relationType));
                        var roleColumnName = this.mapping.NormalizeName(Mapping.RelationTableRoleColumnNameFor(relationType));
                        this.ValidateColumns(
                            table,
                            new[]
                            {
                                (associationColumnName, SqlTypeForObject, DescribeRelationTableAssociation(relationType)),
                                (roleColumnName, SqlTypeForObject, DescribeRelationTableRole(relationType)),
                            },
                            v => DescribeObsoleteRelationTableColumn(metaPopulation, v));

                        var associationColumn = table.GetColumn(associationColumnName);
                        var roleColumn = table.GetColumn(roleColumnName);
                        if (associationColumn != null && roleColumn != null &&
                            !HasRelationPrimaryKey(roleType, table.PrimaryKeyColumnCount, associationColumn.PrimaryKeyOrdinal, roleColumn.PrimaryKeyOrdinal))
                        {
                            this.AddLines(table, new[] { PrimaryKeyLine(roleType, table.ColumnByLowercaseColumnName.Values.Select(v => (v.Name, v.PrimaryKeyOrdinal)), associationColumnName, roleColumnName) });
                        }
                    }
                }
            }

            // The tables of the adapter that the model lacks, by the names that PostgreSQL stores: a table whose
            // name differs from the prefix only in case is not one of them.
            foreach (var storedTableName in this.Schema.StoredTableNames)
            {
                if (storedTableName.StartsWith(Mapping.Prefix, StringComparison.Ordinal) && !tableNames.Contains(storedTableName))
                {
                    this.ObsoleteTableNames.Add(storedTableName);
                }
            }

            // TODO: Validate the primary keys of the objects table and the class tables.
        }

        // The table is invalid when its columns are not those of the model: the full set, with their types.
        private void ValidateColumns(SchemaTable table, IEnumerable<(string Name, string SqlType, string Description)> columns, Func<string, string> describeObsolete)
        {
            var sqlTypeByColumn = table.ColumnByLowercaseColumnName.ToDictionary(v => v.Key, v => v.Value.SqlType);
            var expected = columns.Select(v => (this.mapping.NormalizeName(v.Name), v.SqlType, v.Description));
            this.AddLines(table, CompareColumns(expected, sqlTypeByColumn, describeObsolete));
        }

        private void AddLines(SchemaTable table, IReadOnlyCollection<string> lines)
        {
            if (lines.Count == 0)
            {
                return;
            }

            this.InvalidTables.Add(table);
            if (!this.linesByInvalidTable.TryGetValue(table, out var tableLines))
            {
                tableLines = new List<string>();
                this.linesByInvalidTable.Add(table, tableLines);
            }

            tableLines.AddRange(lines);
        }
    }
}

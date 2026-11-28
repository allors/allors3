// <copyright file="Mapping.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Meta;

    [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
    public class Mapping : Sql.Mapping
    {
        public override string ParamInvocationFormat => ParamFormat;

        public override string ParamInvocationNameForObject => this.ParamNameForObject;
        public override string ParamInvocationNameForClass => this.ParamNameForClass;

        public override string TableNameForObjects { get; }
        public override IDictionary<IClass, string> TableNameForObjectByClass => this.tableNameForObjectByClass;

        public override IDictionary<IRelationType, string> ColumnNameByRelationType => this.columnNameByRelationType;

        public override IDictionary<IRoleType, string> ParamInvocationNameByRoleType => this.ParamNameByRoleType;

        public override IDictionary<IClass, string> CommandTextForDeleteObjectByClass => this.commandTextForDeleteObjectByClass;

        public override IDictionary<IClass, string> CommandTextForCreateObjectsByClass => this.commandTextForCreateObjectsByClass;

        public override IDictionary<IClass, string> CommandTextForGetUnitRolesByClass => this.commandTextForGetUnitRolesByClass;

        public override IDictionary<IClass, IDictionary<IRelationType, string>> CommandTextForSetUnitRoleByRelationTypeByClass => this.commandTextForSetUnitRoleByRelationTypeByClass;

        public override IDictionary<IRelationType, string> CommandTextForGetRoleByRelationType => this.commandTextForGetRoleByRelationType;

        public override IDictionary<IRelationType, string> CommandTextForSetRoleByRelationType => this.commandTextForSetRoleByRelationType;

        public override IDictionary<IRelationType, string> CommandTextForAddRoleByRelationType => this.commandTextForAddRoleByRelationType;

        public override IDictionary<IRelationType, string> CommandTextForRemoveRoleByRelationType => this.commandTextForRemoveRoleByRelationType;

        public override IDictionary<IRelationType, string> CommandTextForClearRoleByRelationType => this.commandTextForClearRoleByRelationType;

        public override IDictionary<IRelationType, string> CommandTextForGetAssociationByRelationType => this.commandTextForGetAssociationByRelationType;

        public override IDictionary<IClass, string> CommandTextForCreateObjectByClass => this.commandTextForCreateObjectByClass;

        public override string CommandTextForInstantiate => this.commandTextForInstantiate;

        public override string CommandTextForGetVersion => this.commandTextForGetVersion;

        public override string CommandTextForUpdateVersion => this.commandTextForUpdateVersion;

        public override IDictionary<IClass, string> CommandTextForPrefetchUnitRolesByClass => this.commandTextForPrefetchUnitRolesByClass;

        public override IDictionary<IRelationType, string> CommandTextForPrefetchRoleByRelationType => this.commandTextForPrefetchRoleByRelationType;

        public override IDictionary<IRelationType, string> CommandTextForPrefetchAssociationByRelationType => this.commandTextForPrefetchAssociationByRelationType;

        public override string StringCollation => "COLLATE Latin1_General_100_BIN2";
        public override string Ascending => "ASC";
        public override string Descending => "DESC";

        // T-SQL LIKE treats '[' as the start of a character class; '[[]' matches a literal '['. Escaping '['
        // keeps '%' and '_' as wildcards (ANSI) while disabling char-class/range syntax, matching Npgsql and Memory.
        public override string EscapeLikePattern(string like) => like.Replace("[", "[[]");

        public override IDictionary<IRelationType, string> TableNameForRelationByRelationType => this.tableNameForRelationByRelationType;

        internal const string SqlTypeForClass = "uniqueidentifier";
        internal const string SqlTypeForObject = "bigint";
        internal const string SqlTypeForVersion = "bigint";

        internal const SqlDbType SqlDbTypeForClass = SqlDbType.UniqueIdentifier;
        internal const SqlDbType SqlDbTypeForObject = SqlDbType.BigInt;
        internal const SqlDbType SqlDbTypeForCount = SqlDbType.Int;

        internal string ParamNameForAssociation { get; }
        internal string ParamNameForCompositeRole { get; }
        internal string ParamNameForCount { get; }
        internal string ParamNameForTableType { get; }
        internal IDictionary<IRoleType, string> ParamNameByRoleType { get; }

        /// <summary>
        /// The start of the name of every table type of the adapter. Init drops the table types whose names start
        /// with it when it builds the schema again, and leaves the others.
        /// </summary>
        internal const string TableTypePrefix = Prefix + "t_";

        internal string TableTypeNameForObject { get; }

        internal string TableTypeColumnNameForObject { get; }

        internal string TableTypeNameForCompositeRelation { get; }

        internal string TableTypeNameForStringRelation { get; }
        internal string TableTypeNameForIntegerRelation { get; }
        internal string TableTypeNameForFloatRelation { get; }
        internal string TableTypeNameForBooleanRelation { get; }
        internal string TableTypeNameForDateTimeRelation { get; }
        internal string TableTypeNameForUniqueRelation { get; }
        internal string TableTypeNameForBinaryRelation { get; }
        private string TableTypeNamePrefixForDecimalRelation { get; }

        internal string TableTypeColumnNameForAssociation { get; }
        internal string TableTypeColumnNameForRole { get; }

        internal Dictionary<int, Dictionary<int, string>> TableTypeNameForDecimalRelationByScaleByPrecision { get; }

        private string ParamNameForObject { get; }
        private string ParamNameForClass { get; }

        private const string ParamFormat = "@{0}";






        private const string TenRows = "(VALUES (0),(0),(0),(0),(0),(0),(0),(0),(0),(0))";

        // A source of rows to take a count from: ten lists of ten rows in every combination, which is more rows
        // than a count can ask for.
        private const string RowsForCount =
            TenRows + " AS d1 (n)\nCROSS JOIN " +
            TenRows + " AS d2 (n)\nCROSS JOIN " +
            TenRows + " AS d3 (n)\nCROSS JOIN " +
            TenRows + " AS d4 (n)\nCROSS JOIN " +
            TenRows + " AS d5 (n)\nCROSS JOIN " +
            TenRows + " AS d6 (n)\nCROSS JOIN " +
            TenRows + " AS d7 (n)\nCROSS JOIN " +
            TenRows + " AS d8 (n)\nCROSS JOIN " +
            TenRows + " AS d9 (n)\nCROSS JOIN " +
            TenRows + " AS d10 (n)";

        private readonly IDictionary<IRelationType, string> tableNameForRelationByRelationType;
        private readonly IDictionary<IRelationType, string> commandTextForPrefetchAssociationByRelationType;
        private readonly IDictionary<IRelationType, string> commandTextForPrefetchRoleByRelationType;
        private readonly IDictionary<IClass, string> commandTextForPrefetchUnitRolesByClass;
        private readonly IDictionary<IClass, string> commandTextForCreateObjectByClass;
        private readonly IDictionary<IRelationType, string> commandTextForGetAssociationByRelationType;
        private readonly IDictionary<IRelationType, string> commandTextForClearRoleByRelationType;
        private readonly IDictionary<IRelationType, string> commandTextForRemoveRoleByRelationType;
        private readonly IDictionary<IRelationType, string> commandTextForAddRoleByRelationType;
        private readonly IDictionary<IRelationType, string> commandTextForSetRoleByRelationType;
        private readonly IDictionary<IRelationType, string> commandTextForGetRoleByRelationType;
        private readonly IDictionary<IClass, IDictionary<IRelationType, string>> commandTextForSetUnitRoleByRelationTypeByClass;
        private readonly IDictionary<IClass, string> commandTextForGetUnitRolesByClass;
        private readonly IDictionary<IClass, string> commandTextForCreateObjectsByClass;
        private readonly IDictionary<IClass, string> commandTextForDeleteObjectByClass;
        private string commandTextForInstantiate;
        private string commandTextForGetVersion;
        private string commandTextForUpdateVersion;
        private readonly IDictionary<IRelationType, string> columnNameByRelationType;
        private readonly IDictionary<IClass, string> tableNameForObjectByClass;

        public Mapping(Database database)
        {
            this.Database = database;

            this.ParamNameForAssociation = string.Format(ParamFormat, AliasForAssociation);
            this.ParamNameForCompositeRole = string.Format(ParamFormat, AliasForRole);
            this.ParamNameForCount = string.Format(ParamFormat, "count");
            this.ParamNameForTableType = string.Format(ParamFormat, "table");

            this.ParamNameForObject = string.Format(ParamFormat, ColumnNameForObject);
            this.ParamNameForClass = string.Format(ParamFormat, ColumnNameForClass);

            // TableTypes
            // ----------
            this.TableTypeNameForObject = $"{database.SchemaName}.{TableTypePrefix}o";
            this.TableTypeNameForCompositeRelation = $"{database.SchemaName}.{TableTypePrefix}c";

            this.TableTypeNameForStringRelation = $"{database.SchemaName}.{TableTypePrefix}s";
            this.TableTypeNameForIntegerRelation = $"{database.SchemaName}.{TableTypePrefix}i";
            this.TableTypeNameForFloatRelation = $"{database.SchemaName}.{TableTypePrefix}f";
            this.TableTypeNameForBooleanRelation = $"{database.SchemaName}.{TableTypePrefix}bo";
            this.TableTypeNameForDateTimeRelation = $"{database.SchemaName}.{TableTypePrefix}da";
            this.TableTypeNameForUniqueRelation = $"{database.SchemaName}.{TableTypePrefix}u";
            this.TableTypeNameForBinaryRelation = $"{database.SchemaName}.{TableTypePrefix}bi";
            this.TableTypeNamePrefixForDecimalRelation = $"{database.SchemaName}.{TableTypePrefix}de";

            this.TableTypeColumnNameForObject = "_o";
            this.TableTypeColumnNameForAssociation = "_a";
            this.TableTypeColumnNameForRole = "_r";

            this.TableTypeNameForDecimalRelationByScaleByPrecision = new Dictionary<int, Dictionary<int, string>>();
            foreach (var relationType in database.MetaPopulation.DatabaseRelationTypes)
            {
                var roleType = relationType.RoleType;
                if (roleType.ObjectType.IsUnit && ((IUnit)roleType.ObjectType).IsDecimal)
                {
                    var precision = roleType.Precision.Value;
                    var scale = roleType.Scale.Value;

                    {
                        // Table
                        var tableName = $"{this.TableTypeNamePrefixForDecimalRelation}{precision}_{scale}";

                        if (!this.TableTypeNameForDecimalRelationByScaleByPrecision.TryGetValue(precision, out var decimalRelationTableByScale))
                        {
                            decimalRelationTableByScale = new Dictionary<int, string>();
                            this.TableTypeNameForDecimalRelationByScaleByPrecision[precision] = decimalRelationTableByScale;
                        }

                        if (!decimalRelationTableByScale.ContainsKey(scale))
                        {
                            decimalRelationTableByScale[scale] = tableName;
                        }
                    }
                }
            }

            this.TableTypeDefinitionByName = new Dictionary<string, string>
             {
                 // Table
                 {
                     this.TableTypeNameForObject,
                     $"CREATE TYPE {this.TableTypeNameForObject} AS TABLE ({this.TableTypeColumnNameForObject} {SqlTypeForObject})"
                 },
                 {
                     this.TableTypeNameForCompositeRelation,
                     $"CREATE TYPE {this.TableTypeNameForCompositeRelation} AS TABLE ({this.TableTypeColumnNameForAssociation} {SqlTypeForObject}, {this.TableTypeColumnNameForRole} {SqlTypeForObject})"
                 },
                 {
                     this.TableTypeNameForStringRelation,
                     $"CREATE TYPE {this.TableTypeNameForStringRelation} AS TABLE ({this.TableTypeColumnNameForAssociation} {SqlTypeForObject}, {this.TableTypeColumnNameForRole} nvarchar(max))"
                 },
                 {
                     this.TableTypeNameForIntegerRelation,
                     $"CREATE TYPE {this.TableTypeNameForIntegerRelation} AS TABLE ({this.TableTypeColumnNameForAssociation} {SqlTypeForObject}, {this.TableTypeColumnNameForRole} int)"
                 },
                 {
                     this.TableTypeNameForFloatRelation,
                     $"CREATE TYPE {this.TableTypeNameForFloatRelation} AS TABLE ({this.TableTypeColumnNameForAssociation} {SqlTypeForObject}, {this.TableTypeColumnNameForRole} float)"
                 },
                 {
                     this.TableTypeNameForDateTimeRelation,
                     $"CREATE TYPE {this.TableTypeNameForDateTimeRelation} AS TABLE ({this.TableTypeColumnNameForAssociation} {SqlTypeForObject}, {this.TableTypeColumnNameForRole} datetime2)"
                 },
                 {
                     this.TableTypeNameForBooleanRelation,
                     $"CREATE TYPE {this.TableTypeNameForBooleanRelation} AS TABLE ({this.TableTypeColumnNameForAssociation} {SqlTypeForObject}, {this.TableTypeColumnNameForRole} bit)"
                 },
                 {
                     this.TableTypeNameForUniqueRelation,
                     $"CREATE TYPE {this.TableTypeNameForUniqueRelation} AS TABLE ({this.TableTypeColumnNameForAssociation} {SqlTypeForObject}, {this.TableTypeColumnNameForRole} uniqueidentifier)"
                 },
                 {
                     this.TableTypeNameForBinaryRelation,
                     $"CREATE TYPE {this.TableTypeNameForBinaryRelation} AS TABLE ({this.TableTypeColumnNameForAssociation} {SqlTypeForObject}, {this.TableTypeColumnNameForRole} varbinary(max))"
                 },
             };

            foreach (var precisionEntry in this.TableTypeNameForDecimalRelationByScaleByPrecision)
            {
                var precision = precisionEntry.Key;
                foreach (var scaleEntry in precisionEntry.Value)
                {
                    var scale = scaleEntry.Key;
                    var decimalRelationTable = scaleEntry.Value;

                    var sql =
@$"CREATE TYPE {decimalRelationTable} AS TABLE
({this.TableTypeColumnNameForAssociation} {SqlTypeForObject},
{this.TableTypeColumnNameForRole} DECIMAL({precision},{scale}) )
";

                    this.TableTypeDefinitionByName.Add(decimalRelationTable, sql);
                }
            }

            // Tables
            // ------
            this.TableNameForObjects = $"{database.SchemaName}.{ObjectsTableName}";
            this.tableNameForObjectByClass = new Dictionary<IClass, string>();
            this.columnNameByRelationType = new Dictionary<IRelationType, string>();
            this.ParamNameByRoleType = new Dictionary<IRoleType, string>();

            foreach (var @class in this.Database.MetaPopulation.DatabaseClasses)
            {
                this.tableNameForObjectByClass.Add(@class, $"{this.Database.SchemaName}.{TableNameFor(@class)}");

                foreach (var associationType in @class.DatabaseAssociationTypes)
                {
                    var relationType = associationType.RelationType;
                    var roleType = relationType.RoleType;
                    if (!(associationType.IsMany && roleType.IsMany) && relationType.ExistExclusiveDatabaseClasses && roleType.IsMany)
                    {
                        this.columnNameByRelationType[relationType] = AssociationColumnNameFor(relationType);
                    }
                }

                foreach (var roleType in @class.DatabaseRoleTypes)
                {
                    var relationType = roleType.RelationType;
                    var associationType3 = relationType.AssociationType;
                    if (roleType.ObjectType.IsUnit)
                    {
                        this.columnNameByRelationType[relationType] = RoleColumnNameFor(relationType);

                        // The parameter carries the id of the relation type. The full names of two roles of one
                        // class can differ only in case, and a parameter is found by its name without case: one
                        // statement that sets both roles then gave both columns one value.
                        this.ParamNameByRoleType[roleType] = string.Format(ParamFormat, "p_" + ModelId.Encode(relationType.Id));
                    }
                    else if (!(associationType3.IsMany && roleType.IsMany) && relationType.ExistExclusiveDatabaseClasses && !roleType.IsMany)
                    {
                        this.columnNameByRelationType[relationType] = RoleColumnNameFor(relationType);
                    }
                }
            }

            this.tableNameForRelationByRelationType = new Dictionary<IRelationType, string>();

            foreach (var relationType in this.Database.MetaPopulation.DatabaseRelationTypes)
            {
                var associationType = relationType.AssociationType;
                var roleType = relationType.RoleType;

                if (!roleType.ObjectType.IsUnit && ((associationType.IsMany && roleType.IsMany) || !relationType.ExistExclusiveDatabaseClasses))
                {
                    this.tableNameForRelationByRelationType.Add(relationType, $"{this.Database.SchemaName}.{TableNameFor(relationType)}");
                }
            }

            // Commands
            // --------
            this.commandTextForCreateObjectByClass = new Dictionary<IClass, string>();
            this.commandTextForCreateObjectsByClass = new Dictionary<IClass, string>();
            this.commandTextForDeleteObjectByClass = new Dictionary<IClass, string>();

            this.commandTextForGetUnitRolesByClass = new Dictionary<IClass, string>();
            this.commandTextForPrefetchUnitRolesByClass = new Dictionary<IClass, string>();
            this.commandTextForSetUnitRoleByRelationTypeByClass = new Dictionary<IClass, IDictionary<IRelationType, string>>();

            this.commandTextForGetRoleByRelationType = new Dictionary<IRelationType, string>();
            this.commandTextForPrefetchRoleByRelationType = new Dictionary<IRelationType, string>();
            this.commandTextForSetRoleByRelationType = new Dictionary<IRelationType, string>();
            this.commandTextForAddRoleByRelationType = new Dictionary<IRelationType, string>();
            this.commandTextForRemoveRoleByRelationType = new Dictionary<IRelationType, string>();
            this.commandTextForClearRoleByRelationType = new Dictionary<IRelationType, string>();
            this.commandTextForGetAssociationByRelationType = new Dictionary<IRelationType, string>();
            this.commandTextForPrefetchAssociationByRelationType = new Dictionary<IRelationType, string>();

            this.Instantiate();
            this.GetVersionIds();
            this.UpdateVersionIds();

            foreach (var @class in this.Database.MetaPopulation.DatabaseClasses)
            {
                this.CreateObject(@class);
                this.CreateObjects(@class);
                this.DeleteObject(@class);

                if (this.Database.GetSortedUnitRolesByObjectType(@class).Length > 0)
                {
                    this.GetUnitRoles(@class);
                    this.PrefetchUnitRoles(@class);
                }

                foreach (var associationType in @class.DatabaseAssociationTypes)
                {
                    if (!(associationType.IsMany && associationType.RoleType.IsMany) && associationType.RelationType.ExistExclusiveDatabaseClasses && associationType.RoleType.IsMany)
                    {
                        this.GetCompositesRoleObjectTable(associationType, @class);
                        this.PrefetchCompositesRoleObjectTable(associationType, @class);

                        if (associationType.IsOne)
                        {
                            this.GetCompositeAssociationObjectTable(associationType, @class);
                            this.PrefetchCompositeAssociationObjectTable(associationType, @class);
                        }

                        this.AddCompositeRoleObjectTable(associationType, @class);
                        this.RemoveCompositeRoleObjectTable(associationType, @class);
                        this.ClearCompositeRoleObjectTable(associationType, @class);
                    }
                }

                foreach (var roleType in @class.DatabaseRoleTypes)
                {
                    if (roleType.ObjectType.IsUnit)
                    {
                        this.SetUnitRoleType(roleType, @class);
                    }
                    else if (!(roleType.AssociationType.IsMany && roleType.IsMany) && roleType.RelationType.ExistExclusiveDatabaseClasses && roleType.IsOne)
                    {
                        this.GetCompositeRoleObjectTable(roleType, @class);
                        this.PrefetchCompositeRoleObjectTable(roleType, @class);

                        if (roleType.AssociationType.IsOne)
                        {
                            this.GetCompositeAssociationOne2OneObjectTable(roleType, @class);
                            this.PrefetchCompositeAssociationObjectTable(roleType, @class);
                        }
                        else
                        {
                            this.GetCompositesAssociationMany2OneObjectTable(roleType, @class);
                            this.PrefetchCompositesAssociationMany2OneObjectTable(roleType, @class);
                        }

                        this.SetCompositeRole(roleType, @class);
                        this.ClearCompositeRole(roleType, @class);
                    }
                }
            }

            foreach (var relationType in this.Database.MetaPopulation.DatabaseRelationTypes)
            {
                if (!relationType.RoleType.ObjectType.IsUnit && ((relationType.AssociationType.IsMany && relationType.RoleType.IsMany) || !relationType.ExistExclusiveDatabaseClasses))
                {
                    if (relationType.RoleType.IsMany)
                    {
                        this.GetCompositesRoleRelationTable(relationType);
                        this.PrefetchCompositesRoleRelationTable(relationType);
                        this.AddCompositeRoleRelationTable(relationType);
                        this.RemoveCompositeRoleRelationTable(relationType);
                    }
                    else
                    {
                        this.GetCompositeRoleRelationTable(relationType);
                        this.PrefetchCompositeRoleRelationType(relationType);
                        this.SetCompositeRoleRelationType(relationType);
                    }

                    if (relationType.AssociationType.IsOne)
                    {
                        this.GetCompositeAssociationRelationTable(relationType);
                        this.PrefetchCompositeAssociationRelationTable(relationType);
                    }
                    else
                    {
                        this.GetCompositesAssociationRelationTable(relationType);
                        this.PrefetchCompositesAssociationRelationTable(relationType);
                    }

                    this.ClearCompositeRoleRelationTable(relationType);
                }
            }
        }

        public Dictionary<string, string> TableTypeDefinitionByName { get; }

        protected internal Database Database { get; }

        public string GetTableTypeNameForRelation(IRoleType roleType)
        {
            var unitTypeTag = ((IUnit)roleType.ObjectType).Tag;
            return unitTypeTag switch
            {
                UnitTags.String => this.TableTypeNameForStringRelation,
                UnitTags.Integer => this.TableTypeNameForIntegerRelation,
                UnitTags.Float => this.TableTypeNameForFloatRelation,
                UnitTags.Boolean => this.TableTypeNameForBooleanRelation,
                UnitTags.DateTime => this.TableTypeNameForDateTimeRelation,
                UnitTags.Unique => this.TableTypeNameForUniqueRelation,
                UnitTags.Binary => this.TableTypeNameForBinaryRelation,
                UnitTags.Decimal => this.TableTypeNameForDecimalRelationByScaleByPrecision[roleType.Precision.Value][roleType.Scale.Value],
                _ => throw new ArgumentException($"Unknown Unit ObjectType: {unitTypeTag}")
            };
        }

        public string NormalizeName(string name)
        {
            name = name.ToLowerInvariant();
            if (ReservedWords.Names.Contains(name))
            {
                return $"[{name}]";
            }

            return name;
        }

        internal string GetSqlType(IRoleType roleType)
        {
            var unit = (IUnit)roleType.ObjectType;
            switch (unit.Tag)
            {
                case UnitTags.String:
                    if (roleType.Size == -1 || roleType.Size > 4000)
                    {
                        return "nvarchar(max)";
                    }

                    return $"nvarchar({roleType.Size})";

                case UnitTags.Integer:
                    return "int";

                case UnitTags.Decimal:
                    return $"decimal({roleType.Precision},{roleType.Scale})";

                case UnitTags.Float:
                    return "float";

                case UnitTags.Boolean:
                    return "bit";

                case UnitTags.DateTime:
                    return "datetime2";

                case UnitTags.Unique:
                    return "uniqueidentifier";

                case UnitTags.Binary:
                    if (roleType.Size == -1 || roleType.Size > 8000)
                    {
                        return "varbinary(max)";
                    }

                    return $"varbinary({roleType.Size})";

                default:
                    return "!UNKNOWN VALUE TYPE!";
            }
        }

        internal SqlDbType GetSqlDbType(IRoleType roleType)
        {
            var unit = (IUnit)roleType.ObjectType;
            return unit.Tag switch
            {
                UnitTags.String => SqlDbType.NVarChar,
                UnitTags.Integer => SqlDbType.Int,
                UnitTags.Decimal => SqlDbType.Decimal,
                UnitTags.Float => SqlDbType.Float,
                UnitTags.Boolean => SqlDbType.Bit,
                UnitTags.DateTime => SqlDbType.DateTime2,
                UnitTags.Unique => SqlDbType.UniqueIdentifier,
                UnitTags.Binary => SqlDbType.VarBinary,
                _ => throw new Exception("Unknown Unit Type")
            };
        }

        private void CreateObject(IClass @class)
        {
            var table = this.tableNameForObjectByClass[@class];

            // CreateObject
            var sql = $@"DECLARE  {this.ParamNameForObject} AS {SqlTypeForObject}

INSERT INTO {this.TableNameForObjects} ({ColumnNameForClass}, {ColumnNameForVersion})
VALUES ({this.ParamNameForClass}, {(long)Allors.Version.DatabaseInitial});

SELECT {this.ParamNameForObject} = SCOPE_IDENTITY();

INSERT INTO {table} ({ColumnNameForObject},{ColumnNameForClass})
VALUES ({this.ParamNameForObject},{this.ParamNameForClass});

SELECT {this.ParamNameForObject};";
            this.commandTextForCreateObjectByClass.Add(@class, sql);
        }

        private void CreateObjects(IClass @class)
        {
            var table = this.tableNameForObjectByClass[@class.ExclusiveDatabaseClass];

            // CreateObjects
            // One statement creates the objects. The rows go into the objects table, the first OUTPUT gives their
            // ids to the table of the class and the second gives them to the caller. The rows are taken from
            // RowsForCount. A count below zero creates nothing.
            var sql = $@"INSERT INTO {this.TableNameForObjects} ({ColumnNameForClass}, {ColumnNameForVersion})
OUTPUT inserted.{ColumnNameForObject}, inserted.{ColumnNameForClass} INTO {table} ({ColumnNameForObject}, {ColumnNameForClass})
OUTPUT inserted.{ColumnNameForObject}
SELECT TOP (CASE WHEN {this.ParamNameForCount} > 0 THEN {this.ParamNameForCount} ELSE 0 END) {this.ParamNameForClass}, {(long)Allors.Version.DatabaseInitial}
FROM {RowsForCount};";
            this.commandTextForCreateObjectsByClass.Add(@class, sql);
        }

        private void DeleteObject(IClass @class)
        {
            var table = this.tableNameForObjectByClass[@class.ExclusiveDatabaseClass];

            var sql = $@"DELETE FROM {this.TableNameForObjects}
WHERE {ColumnNameForObject}={this.ParamNameForObject};

DELETE FROM {table}
WHERE {ColumnNameForObject}={this.ParamNameForObject};";
            this.commandTextForDeleteObjectByClass.Add(@class, sql);
        }

        private void GetUnitRoles(IClass @class)
        {
            var sortedUnitRoleTypes = this.Database.GetSortedUnitRolesByObjectType(@class);
            var table = this.tableNameForObjectByClass[@class.ExclusiveDatabaseClass];

            // Get Unit Roles
            var sql = $@"SELECT {string.Join(", ", sortedUnitRoleTypes.Select(v => this.columnNameByRelationType[v.RelationType]))}
FROM {table}
WHERE {ColumnNameForObject}={this.ParamNameForObject}";
            this.commandTextForGetUnitRolesByClass.Add(@class, sql);
        }

        private void PrefetchUnitRoles(IClass @class)
        {
            var sortedUnitRoleTypes = this.Database.GetSortedUnitRolesByObjectType(@class);
            var table = this.tableNameForObjectByClass[@class.ExclusiveDatabaseClass];

            // Prefetch Unit Roles
            var sql = $@"SELECT {ColumnNameForObject}, {string.Join(", ", sortedUnitRoleTypes.Select(v => this.columnNameByRelationType[v.RelationType]))}
FROM {table}
WHERE {ColumnNameForObject} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchUnitRolesByClass.Add(@class, sql);
        }

        private void GetCompositesRoleObjectTable(IAssociationType associationType, IClass @class)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Get Composites Role (1-*) [object table]
            var sql = $@"SELECT {ColumnNameForObject}
FROM {table}
WHERE {this.columnNameByRelationType[relationType]}={this.ParamNameForAssociation}";
            this.commandTextForGetRoleByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositesRoleObjectTable(IAssociationType associationType, IClass @class)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Prefetch Composites Role (1-*) [object table]
            var sql = $@"SELECT {this.columnNameByRelationType[relationType]}, {ColumnNameForObject}
FROM {table}
WHERE {this.columnNameByRelationType[relationType]} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchRoleByRelationType.Add(relationType, sql);
        }

        private void GetCompositeAssociationObjectTable(IAssociationType associationType, IClass @class)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Get Composite Association (1-*) [object table]
            var sql = $@"SELECT {this.columnNameByRelationType[relationType]}
FROM {table}
WHERE {ColumnNameForObject}={this.ParamNameForCompositeRole}";
            this.commandTextForGetAssociationByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositeAssociationObjectTable(IAssociationType associationType, IClass @class)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Prefetch Composite Association (1-*) [object table]
            var sql = $@"SELECT {this.columnNameByRelationType[relationType]}, {ColumnNameForObject}
FROM {table}
WHERE {ColumnNameForObject} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, sql);
        }

        private void AddCompositeRoleObjectTable(IAssociationType associationType, IClass @class)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Add Composite Role (1-*) [object table]
            var sql = $@"UPDATE {table}
SET {this.columnNameByRelationType[relationType]} = r.{this.TableTypeColumnNameForAssociation}
FROM {table}
INNER JOIN {this.ParamNameForTableType} AS r
ON {ColumnNameForObject} = r.{this.TableTypeColumnNameForRole}";
            this.commandTextForAddRoleByRelationType.Add(relationType, sql);
        }

        private void RemoveCompositeRoleObjectTable(IAssociationType associationType, IClass @class)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Remove Composite Role (1-*) [object table]
            var sql = $@"UPDATE {table}
SET {this.columnNameByRelationType[relationType]} = null
FROM {table}
INNER JOIN {this.ParamNameForTableType} AS r
ON
    {this.columnNameByRelationType[relationType]} = r.{this.TableTypeColumnNameForAssociation} AND
    {ColumnNameForObject} = r.{this.TableTypeColumnNameForRole}";
            this.commandTextForRemoveRoleByRelationType.Add(relationType, sql);
        }

        private void ClearCompositeRoleObjectTable(IAssociationType associationType, IClass @class)
        {
            var relationType = associationType.RelationType;

            // Clear Composites Role (1-*) [object table]
            var sql = $@"UPDATE {this.tableNameForObjectByClass[@class]}
SET {this.columnNameByRelationType[relationType]} = null
FROM {this.tableNameForObjectByClass[@class]}
INNER JOIN {this.ParamNameForTableType} AS a
ON {this.columnNameByRelationType[relationType]} = a.{this.TableTypeColumnNameForObject}";
            this.commandTextForClearRoleByRelationType.Add(relationType, sql);
        }

        private void SetUnitRoleType(IRoleType roleType, IClass @class)
        {
            if (!this.commandTextForSetUnitRoleByRelationTypeByClass.TryGetValue(@class, out var commandTextForSetUnitRoleByRelationType))
            {
                commandTextForSetUnitRoleByRelationType = new Dictionary<IRelationType, string>();
                this.commandTextForSetUnitRoleByRelationTypeByClass.Add(@class, commandTextForSetUnitRoleByRelationType);
            }

            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            var sql = $@"UPDATE {table}
SET {this.columnNameByRelationType[relationType]} = r.{this.TableTypeColumnNameForRole}
FROM {table}
INNER JOIN {this.ParamNameForTableType} AS r
ON {ColumnNameForObject} = r.{this.TableTypeColumnNameForAssociation}";
            commandTextForSetUnitRoleByRelationType.Add(relationType, sql);
        }

        private void GetCompositeRoleObjectTable(IRoleType roleType, IClass @class)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Get Composite Role (1-1 and *-1) [object table]
            var sql = $@"SELECT {this.columnNameByRelationType[relationType]}
FROM {table}
WHERE {ColumnNameForObject}={this.ParamNameForAssociation}";
            this.commandTextForGetRoleByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositeRoleObjectTable(IRoleType roleType, IClass @class)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Prefetch Composite Role (1-1 and *-1) [object table]
            var sql = $@"SELECT  {ColumnNameForObject}, {this.columnNameByRelationType[relationType]}
FROM {table}
WHERE {ColumnNameForObject} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchRoleByRelationType.Add(relationType, sql);
        }

        private void GetCompositeAssociationOne2OneObjectTable(IRoleType roleType, IClass @class)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Get Composite Association (1-1) [object table]
            var sql = $@"SELECT {ColumnNameForObject}
FROM {table}
WHERE {this.columnNameByRelationType[relationType]}={this.ParamNameForCompositeRole}";
            this.commandTextForGetAssociationByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositeAssociationObjectTable(IRoleType roleType, IClass @class)
        {
            var table = this.tableNameForObjectByClass[@class];

            // Prefetch Composite Association (1-1) [object table]
            var sql = $@"SELECT {ColumnNameForObject}, {this.columnNameByRelationType[roleType.RelationType]}
FROM {table}
WHERE {this.columnNameByRelationType[roleType.RelationType]} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchAssociationByRelationType.Add(roleType.RelationType, sql);
        }

        private void GetCompositesAssociationMany2OneObjectTable(IRoleType roleType, IClass @class)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Get Composite Association (*-1) [object table]
            var sql = $@"SELECT {ColumnNameForObject}
FROM {table}
WHERE {this.columnNameByRelationType[relationType]}={this.ParamNameForCompositeRole}";
            this.commandTextForGetAssociationByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositesAssociationMany2OneObjectTable(IRoleType roleType, IClass @class)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Prefetch Composite Association (*-1) [object table]
            var sql = $@"SELECT {ColumnNameForObject}, {this.columnNameByRelationType[relationType]}
FROM {table}
WHERE {this.columnNameByRelationType[relationType]} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, sql);
        }

        private void SetCompositeRole(IRoleType roleType, IClass @class)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Set Composite Role (1-1 and *-1) [object table]
            var sql = $@"UPDATE {table}
SET {this.columnNameByRelationType[relationType]} = r.{this.TableTypeColumnNameForRole}
FROM {table}
INNER JOIN {this.ParamNameForTableType} AS r
ON {ColumnNameForObject} = r.{this.TableTypeColumnNameForAssociation}";
            this.commandTextForSetRoleByRelationType.Add(relationType, sql);
        }

        private void ClearCompositeRole(IRoleType roleType, IClass @class)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Clear Composite Role (1-1 and *-1) [object table]
            var sql = $@"UPDATE {table}
SET {this.columnNameByRelationType[relationType]} = null
FROM {table}
INNER JOIN {this.ParamNameForTableType} AS a
ON {ColumnNameForObject} = a.{this.TableTypeColumnNameForObject}";
            this.commandTextForClearRoleByRelationType.Add(relationType, sql);
        }

        private void GetCompositesRoleRelationTable(IRelationType relationType)
        {
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var table = this.tableNameForRelationByRelationType[relationType];

            // Get Composites Role (1-* and *-*) [relation table]
            var sql = $@"SELECT {role}
FROM {table}
WHERE {association}={this.ParamNameForAssociation}";
            this.commandTextForGetRoleByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositesRoleRelationTable(IRelationType relationType)
        {
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var table = this.tableNameForRelationByRelationType[relationType];

            // Prefetch Composites Role (1-* and *-*) [relation table]
            var sql = $@"SELECT {association}, {role}
FROM {table}
WHERE {association} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchRoleByRelationType.Add(relationType, sql);
        }

        private void AddCompositeRoleRelationTable(IRelationType relationType)
        {
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var table = this.tableNameForRelationByRelationType[relationType];

            // Add Composite Role (1-* and *-*) [relation table]
            var sql = $@"INSERT INTO {table} ({association},{role})
SELECT {this.TableTypeColumnNameForAssociation}, {this.TableTypeColumnNameForRole}
FROM {this.ParamNameForTableType}";
            this.commandTextForAddRoleByRelationType.Add(relationType, sql);
        }

        private void RemoveCompositeRoleRelationTable(IRelationType relationType)
        {
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var table = this.tableNameForRelationByRelationType[relationType];

            // Remove Composite Role (1-* and *-*) [relation table]
            var sql = $@"DELETE T
FROM {table} T
INNER JOIN {this.ParamNameForTableType} R
ON T.{association} = R.{this.TableTypeColumnNameForAssociation}
AND T.{role} = R.{this.TableTypeColumnNameForRole};";
            this.commandTextForRemoveRoleByRelationType.Add(relationType, sql);
        }

        private void GetCompositeRoleRelationTable(IRelationType relationType)
        {
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);

            // Get Composite Role (1-1 and *-1) [relation table]
            var sql = $@"SELECT {role}
FROM {this.tableNameForRelationByRelationType[relationType]}
WHERE {association}={this.ParamNameForAssociation}";
            this.commandTextForGetRoleByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositeRoleRelationType(IRelationType relationType)
        {
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var table = this.tableNameForRelationByRelationType[relationType];

            // Prefetch Composite Role (1-1 and *-1) [relation table]
            var sql = $@"SELECT {association}, {role}
FROM {table}
WHERE {association} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchRoleByRelationType.Add(relationType, sql);
        }

        private void SetCompositeRoleRelationType(IRelationType relationType)
        {
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var table = this.tableNameForRelationByRelationType[relationType];

            // Set Composite Role (1-1 and *-1) [relation table]
            var sql = $@"MERGE {table} T
USING {this.ParamNameForTableType} AS r
ON T.{association} = r.{this.TableTypeColumnNameForAssociation}

WHEN MATCHED THEN
UPDATE SET {role}= r.{this.TableTypeColumnNameForRole}

WHEN NOT MATCHED THEN
INSERT ({association},{role})
VALUES (r.{this.TableTypeColumnNameForAssociation}, r.{this.TableTypeColumnNameForRole});";
            this.commandTextForSetRoleByRelationType.Add(relationType, sql);
        }

        private void GetCompositeAssociationRelationTable(IRelationType relationType)
        {
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var table = this.tableNameForRelationByRelationType[relationType];

            // Get Composite Association (1-1) [relation table]
            var sql = $@"SELECT {association}
FROM {table}
WHERE {role}={this.ParamNameForCompositeRole}";
            this.commandTextForGetAssociationByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositeAssociationRelationTable(IRelationType relationType)
        {
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var table = this.tableNameForRelationByRelationType[relationType];

            // Prefetch Composite Association (1-1) [relation table]
            var sql = $@"SELECT {association},{role}
FROM {table}
WHERE {role} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, sql);
        }

        private void GetCompositesAssociationRelationTable(IRelationType relationType)
        {
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var table = this.tableNameForRelationByRelationType[relationType];

            // Get Composite Association (*-1) [relation table]
            var sql = $@"SELECT {association}
FROM {table}
WHERE {role}={this.ParamNameForCompositeRole}";
            this.commandTextForGetAssociationByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositesAssociationRelationTable(IRelationType relationType)
        {
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var table = this.tableNameForRelationByRelationType[relationType];

            // Prefetch Composite Association (*-1) [relation table]
            var sql = $@"SELECT {association},{role}
FROM {table}
WHERE {role} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, sql);
        }

        private void ClearCompositeRoleRelationTable(IRelationType relationType)
        {
            var association = RelationTableAssociationColumnNameFor(relationType);
            var table = this.tableNameForRelationByRelationType[relationType];

            // Clear Composite Role (1-1 and *-1) [relation table]
            var sql = $@"DELETE T
FROM {table} T
INNER JOIN {this.ParamNameForTableType} A
ON T.{association} = A.{this.TableTypeColumnNameForObject}";
            this.commandTextForClearRoleByRelationType.Add(relationType, sql);
        }

        private void UpdateVersionIds()
        {
            // Update Version Ids
            var sql = $@"UPDATE {this.TableNameForObjects}
SET {ColumnNameForVersion} = {ColumnNameForVersion} + 1
FROM {this.TableNameForObjects}
WHERE {ColumnNameForObject} IN ( SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType});";
            this.commandTextForUpdateVersion = sql;
        }

        private void GetVersionIds()
        {
            // Get Version Ids
            var sql = $@"SELECT {ColumnNameForObject}, {ColumnNameForVersion}
FROM {this.TableNameForObjects}
WHERE {ColumnNameForObject} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForGetVersion = sql;
        }

        private void Instantiate()
        {
            // Instantiate
            var sql = $@"SELECT {ColumnNameForObject}, {ColumnNameForClass}, {ColumnNameForVersion}
FROM {this.TableNameForObjects}
WHERE {ColumnNameForObject} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForInstantiate = sql;
        }
    }
}

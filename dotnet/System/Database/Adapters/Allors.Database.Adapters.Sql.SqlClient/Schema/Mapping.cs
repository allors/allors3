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

        public override CommandType CommandType => CommandType.Text;

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

        internal string TableTypeNameForObject { get; }
        private string TableTypeNameForVersionedObject { get; }

        internal string TableTypeColumnNameForObject { get; }
        private string TableTypeColumnNameForVersion { get; }

        internal string TableTypeNameForCompositeRelation { get; }

        internal string TableTypeNameForStringRelation { get; }
        internal string TableTypeNameForIntegerRelation { get; }
        internal string TableTypeNameForFloatRelation { get; }
        internal string TableTypeNameForBooleanRelation { get; }
        internal string TableTypeNameForDateTimeRelation { get; }
        internal string TableTypeNameForUniqueRelation { get; }
        internal string TableTypeNameForBinaryRelation { get; }
        private string TableTypeNamePrefixForDecimalRelation { get; }

        internal string TableTypeNameForStringIn { get; }
        internal string TableTypeNameForIntegerIn { get; }
        internal string TableTypeNameForFloatIn { get; }
        internal string TableTypeNameForBooleanIn { get; }
        internal string TableTypeNameForDateTimeIn { get; }
        internal string TableTypeNameForUniqueIn { get; }
        internal string TableTypeNameForBinaryIn { get; }
        private string TableTypeNamePrefixForDecimalIn { get; }

        internal string TableTypeColumnNameForAssociation { get; }
        internal string TableTypeColumnNameForRole { get; }

        internal Dictionary<int, Dictionary<int, string>> TableTypeNameForDecimalRelationByScaleByPrecision { get; }
        internal Dictionary<int, Dictionary<int, string>> TableTypeNameForDecimalInByScaleByPrecision { get; }

        private string ParamNameForObject { get; }
        private string ParamNameForClass { get; }

        private const string ParamFormat = "@{0}";

        private const string ProcedurePrefixForGetVersion = "gv";
        private const string ProcedurePrefixForUpdateVersion = "uv";

        private const string ProcedurePrefixForCreateObject = "co_";
        private const string ProcedurePrefixForCreateObjects = "cos_";
        private const string ProcedurePrefixForDeleteObject = "do_";
        private const string ProcedurePrefixForLoad = "l_";

        private const string ProcedurePrefixForGetUnits = "gu_";
        private const string ProcedurePrefixForPrefetchUnits = "pu_";

        private const string ProcedurePrefixForGetRole = "gc_";
        private const string ProcedurePrefixForPrefetchRole = "pc_";
        private const string ProcedurePrefixForSetRole = "sc_";
        private const string ProcedurePrefixForClearRole = "cc_";
        private const string ProcedurePrefixForAddRole = "ac_";
        private const string ProcedurePrefixForRemoveRole = "rc_";

        private const string ProcedurePrefixForGetAssociation = "ga_";
        private const string ProcedurePrefixForPrefetchAssociation = "pa_";

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

            this.ParamNameForAssociation = string.Format(ParamFormat, ColumnNameForAssociation);
            this.ParamNameForCompositeRole = string.Format(ParamFormat, ColumnNameForRole);
            this.ParamNameForCount = string.Format(ParamFormat, "count");
            this.ParamNameForTableType = string.Format(ParamFormat, "table");

            this.ParamNameForObject = string.Format(ParamFormat, ColumnNameForObject);
            this.ParamNameForClass = string.Format(ParamFormat, ColumnNameForClass);

            // TableTypes
            // ----------
            this.TableTypeNameForObject = $"{database.SchemaName}._t_o";
            this.TableTypeNameForVersionedObject = $"{database.SchemaName}._t_vo";
            this.TableTypeNameForCompositeRelation = $"{database.SchemaName}._t_c";

            this.TableTypeNameForStringRelation = $"{database.SchemaName}._t_s";
            this.TableTypeNameForIntegerRelation = $"{database.SchemaName}._t_i";
            this.TableTypeNameForFloatRelation = $"{database.SchemaName}._t_f";
            this.TableTypeNameForBooleanRelation = $"{database.SchemaName}._t_bo";
            this.TableTypeNameForDateTimeRelation = $"{database.SchemaName}._t_da";
            this.TableTypeNameForUniqueRelation = $"{database.SchemaName}._t_u";
            this.TableTypeNameForBinaryRelation = $"{database.SchemaName}._t_bi";
            this.TableTypeNamePrefixForDecimalRelation = $"{database.SchemaName}._t_de";

            this.TableTypeNameForStringIn = $"{database.SchemaName}._ti_s";
            this.TableTypeNameForIntegerIn = $"{database.SchemaName}._ti_i";
            this.TableTypeNameForFloatIn = $"{database.SchemaName}._ti_f";
            this.TableTypeNameForBooleanIn = $"{database.SchemaName}._ti_bo";
            this.TableTypeNameForDateTimeIn = $"{database.SchemaName}._ti_da";
            this.TableTypeNameForUniqueIn = $"{database.SchemaName}._ti_u";
            this.TableTypeNameForBinaryIn = $"{database.SchemaName}._ti_bi";
            this.TableTypeNamePrefixForDecimalIn = $"{database.SchemaName}._ti_de";

            this.TableTypeColumnNameForObject = "_o";
            this.TableTypeColumnNameForVersion = "_c";
            this.TableTypeColumnNameForAssociation = "_a";
            this.TableTypeColumnNameForRole = "_r";

            this.TableTypeNameForDecimalRelationByScaleByPrecision = new Dictionary<int, Dictionary<int, string>>();
            this.TableTypeNameForDecimalInByScaleByPrecision = new Dictionary<int, Dictionary<int, string>>();
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

                    {
                        // In
                        var tableName = $"{this.TableTypeNamePrefixForDecimalIn}{precision}_{scale}";

                        if (!this.TableTypeNameForDecimalInByScaleByPrecision.TryGetValue(precision, out var decimalInTableByScale))
                        {
                            decimalInTableByScale = new Dictionary<int, string>();
                            this.TableTypeNameForDecimalInByScaleByPrecision[precision] = decimalInTableByScale;
                        }

                        if (!decimalInTableByScale.ContainsKey(scale))
                        {
                            decimalInTableByScale[scale] = tableName;
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
                     this.TableTypeNameForVersionedObject,
                     $"CREATE TYPE {this.TableTypeNameForVersionedObject} AS TABLE ({this.TableTypeColumnNameForObject} {SqlTypeForObject}, {this.TableTypeColumnNameForVersion} {SqlTypeForVersion})"
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
                 // In
                 {
                     this.TableTypeNameForStringIn,
                     $"CREATE TYPE {this.TableTypeNameForStringIn} AS TABLE ({this.TableTypeColumnNameForRole} nvarchar(max))"
                 },
                 {
                     this.TableTypeNameForIntegerIn,
                     $"CREATE TYPE {this.TableTypeNameForIntegerIn} AS TABLE ({this.TableTypeColumnNameForRole} int)"
                 },
                 {
                     this.TableTypeNameForFloatIn,
                     $"CREATE TYPE {this.TableTypeNameForFloatIn} AS TABLE ({this.TableTypeColumnNameForRole} float)"
                 },
                 {
                     this.TableTypeNameForDateTimeIn,
                     $"CREATE TYPE {this.TableTypeNameForDateTimeIn} AS TABLE ({this.TableTypeColumnNameForRole} datetime2)"
                 },
                 {
                     this.TableTypeNameForBooleanIn,
                     $"CREATE TYPE {this.TableTypeNameForBooleanIn} AS TABLE ({this.TableTypeColumnNameForRole} bit)"
                 },
                 {
                     this.TableTypeNameForUniqueIn,
                     $"CREATE TYPE {this.TableTypeNameForUniqueIn} AS TABLE ({this.TableTypeColumnNameForRole} uniqueidentifier)"
                 },
                 {
                     this.TableTypeNameForBinaryIn,
                     $"CREATE TYPE {this.TableTypeNameForBinaryIn} AS TABLE ({this.TableTypeColumnNameForRole} varbinary(max))"
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

            foreach (var precisionEntry in this.TableTypeNameForDecimalInByScaleByPrecision)
            {
                var precision = precisionEntry.Key;
                foreach (var scaleEntry in precisionEntry.Value)
                {
                    var scale = scaleEntry.Key;
                    var decimalRelationIn = scaleEntry.Value;

                    var sql =
                        @$"CREATE TYPE {decimalRelationIn} AS TABLE
({this.TableTypeColumnNameForRole} DECIMAL({precision},{scale}) )
";

                    this.TableTypeDefinitionByName.Add(decimalRelationIn, sql);
                }
            }

            // Tables
            // ------
            this.TableNameForObjects = $"{database.SchemaName}._o";
            this.tableNameForObjectByClass = new Dictionary<IClass, string>();
            this.columnNameByRelationType = new Dictionary<IRelationType, string>();
            this.ParamNameByRoleType = new Dictionary<IRoleType, string>();

            foreach (var @class in this.Database.MetaPopulation.DatabaseClasses)
            {
                this.tableNameForObjectByClass.Add(@class, $"{this.Database.SchemaName}.{this.NormalizeName(@class.SingularName)}");

                foreach (var associationType in @class.DatabaseAssociationTypes)
                {
                    var relationType = associationType.RelationType;
                    var roleType = relationType.RoleType;
                    if (!(associationType.IsMany && roleType.IsMany) && relationType.ExistExclusiveDatabaseClasses && roleType.IsMany)
                    {
                        this.columnNameByRelationType[relationType] = this.NormalizeName(associationType.SingularName);
                    }
                }

                foreach (var roleType in @class.DatabaseRoleTypes)
                {
                    var relationType = roleType.RelationType;
                    var associationType3 = relationType.AssociationType;
                    if (roleType.ObjectType.IsUnit)
                    {
                        this.columnNameByRelationType[relationType] = this.NormalizeName(roleType.SingularName);
                        this.ParamNameByRoleType[roleType] = string.Format(ParamFormat, roleType.SingularFullName);
                    }
                    else if (!(associationType3.IsMany && roleType.IsMany) && relationType.ExistExclusiveDatabaseClasses && !roleType.IsMany)
                    {
                        this.columnNameByRelationType[relationType] = this.NormalizeName(roleType.SingularName);
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
                    this.tableNameForRelationByRelationType.Add(relationType, $"{this.Database.SchemaName}.{this.NormalizeName(relationType.RoleType.SingularFullName)}");
                }
            }

            // Procedures
            // ----------
            this.ProcedureDefinitionByName = new Dictionary<string, string>();

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
                this.LoadObjects(@class);
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

        public Dictionary<string, string> ProcedureDefinitionByName { get; }

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

        public string GetTableTypeNameForIn(IRoleType roleType)
        {
            var unitTypeTag = ((IUnit)roleType.ObjectType).Tag;
            return unitTypeTag switch
            {
                UnitTags.String => this.TableTypeNameForStringIn,
                UnitTags.Integer => this.TableTypeNameForIntegerIn,
                UnitTags.Float => this.TableTypeNameForFloatIn,
                UnitTags.Boolean => this.TableTypeNameForBooleanIn,
                UnitTags.DateTime => this.TableTypeNameForDateTimeIn,
                UnitTags.Unique => this.TableTypeNameForUniqueIn,
                UnitTags.Binary => this.TableTypeNameForBinaryIn,
                UnitTags.Decimal => this.TableTypeNameForDecimalInByScaleByPrecision[roleType.Precision.Value][roleType.Scale.Value],
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

        private void LoadObjects(IClass @class)
        {
            var table = this.tableNameForObjectByClass[@class];
            var name = $"{this.Database.SchemaName}.{ProcedurePrefixForLoad}{@class.Name.ToLowerInvariant()}";

            // Import Objects
            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForClass} {SqlTypeForClass},
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
    INSERT INTO {table} ({ColumnNameForClass}, {ColumnNameForObject})
    SELECT {this.ParamNameForClass}, {this.TableTypeColumnNameForObject}
    FROM {this.ParamNameForTableType}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void CreateObject(IClass @class)
        {
            var table = this.tableNameForObjectByClass[@class];
            var name = $"{this.Database.SchemaName}.{ProcedurePrefixForCreateObject}{@class.Name.ToLowerInvariant()}";

            // CreateObject
            var sql = $@"DECLARE  {this.ParamNameForObject} AS {SqlTypeForObject}

INSERT INTO {this.TableNameForObjects} ({ColumnNameForClass}, {ColumnNameForVersion})
VALUES ({this.ParamNameForClass}, {(long)Allors.Version.DatabaseInitial});

SELECT {this.ParamNameForObject} = SCOPE_IDENTITY();

INSERT INTO {table} ({ColumnNameForObject},{ColumnNameForClass})
VALUES ({this.ParamNameForObject},{this.ParamNameForClass});

SELECT {this.ParamNameForObject};";
            this.commandTextForCreateObjectByClass.Add(@class, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForClass} {SqlTypeForClass}
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void CreateObjects(IClass @class)
        {
            var table = this.tableNameForObjectByClass[@class.ExclusiveDatabaseClass];
            var name = $"{this.Database.SchemaName}.{ProcedurePrefixForCreateObjects}{@class.Name.ToLowerInvariant()}";

            // CreateObjects
            var sql = $@"DECLARE @IDS TABLE (id {SqlTypeForObject});
DECLARE @COUNTER INT

SET @COUNTER = 0
WHILE @COUNTER < {this.ParamNameForCount}
    BEGIN

    INSERT INTO {this.TableNameForObjects} ({ColumnNameForClass}, {ColumnNameForVersion})
    VALUES ({this.ParamNameForClass}, {(long)Allors.Version.DatabaseInitial} );

    INSERT INTO @IDS(id)
    VALUES (SCOPE_IDENTITY());

    SET @COUNTER = @COUNTER+1;
    END

INSERT INTO {table} ({ColumnNameForObject},{ColumnNameForClass})
SELECT ID, {this.ParamNameForClass} FROM @IDS;

SELECT id FROM @IDS;";
            this.commandTextForCreateObjectsByClass.Add(@class, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForClass} {SqlTypeForClass},
    {this.ParamNameForCount} {"int"}
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void DeleteObject(IClass @class)
        {
            var table = this.tableNameForObjectByClass[@class.ExclusiveDatabaseClass];
            var name = $"{this.Database.SchemaName}.{ProcedurePrefixForDeleteObject}{@class.Name.ToLowerInvariant()}";

            var sql = $@"DELETE FROM {this.TableNameForObjects}
WHERE {ColumnNameForObject}={this.ParamNameForObject};

DELETE FROM {table}
WHERE {ColumnNameForObject}={this.ParamNameForObject};";
            this.commandTextForDeleteObjectByClass.Add(@class, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForObject} {SqlTypeForObject}
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetUnitRoles(IClass @class)
        {
            var sortedUnitRoleTypes = this.Database.GetSortedUnitRolesByObjectType(@class);
            var table = this.tableNameForObjectByClass[@class.ExclusiveDatabaseClass];
            var name = $"{this.Database.SchemaName}.{ProcedurePrefixForGetUnits}{@class.Name.ToLowerInvariant()}";

            // Get Unit Roles
            var sql = $@"SELECT {string.Join(", ", sortedUnitRoleTypes.Select(v => this.columnNameByRelationType[v.RelationType]))}
FROM {table}
WHERE {ColumnNameForObject}={this.ParamNameForObject}";
            this.commandTextForGetUnitRolesByClass.Add(@class, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForObject} AS {SqlTypeForObject}
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchUnitRoles(IClass @class)
        {
            var sortedUnitRoleTypes = this.Database.GetSortedUnitRolesByObjectType(@class);
            var table = this.tableNameForObjectByClass[@class.ExclusiveDatabaseClass];
            var name = $"{this.Database.SchemaName}.{ProcedurePrefixForPrefetchUnits}{@class.Name.ToLowerInvariant()}";

            // Prefetch Unit Roles
            var sql = $@"SELECT {ColumnNameForObject}, {string.Join(", ", sortedUnitRoleTypes.Select(v => this.columnNameByRelationType[v.RelationType]))}
FROM {table}
WHERE {ColumnNameForObject} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchUnitRolesByClass.Add(@class, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositesRoleObjectTable(IAssociationType associationType, IClass @class)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForGetRole}{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Get Composites Role (1-*) [object table]
            var sql = $@"SELECT {ColumnNameForObject}
FROM {table}
WHERE {this.columnNameByRelationType[relationType]}={this.ParamNameForAssociation}";
            this.commandTextForGetRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForAssociation} {SqlTypeForObject}
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositesRoleObjectTable(IAssociationType associationType, IClass @class)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForPrefetchRole}{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Prefetch Composites Role (1-*) [object table]
            var sql = $@"SELECT {this.columnNameByRelationType[relationType]}, {ColumnNameForObject}
FROM {table}
WHERE {this.columnNameByRelationType[relationType]} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositeAssociationObjectTable(IAssociationType associationType, IClass @class)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForGetAssociation}{@class.Name.ToLowerInvariant()}_{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Get Composite Association (1-*) [object table]
            var sql = $@"SELECT {this.columnNameByRelationType[relationType]}
FROM {table}
WHERE {ColumnNameForObject}={this.ParamNameForCompositeRole}";
            this.commandTextForGetAssociationByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForCompositeRole} {SqlTypeForObject}
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositeAssociationObjectTable(IAssociationType associationType, IClass @class)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForPrefetchAssociation}{@class.Name.ToLowerInvariant()}_{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Prefetch Composite Association (1-*) [object table]
            var sql = $@"SELECT {this.columnNameByRelationType[relationType]}, {ColumnNameForObject}
FROM {table}
WHERE {ColumnNameForObject} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void AddCompositeRoleObjectTable(IAssociationType associationType, IClass @class)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForAddRole}{@class.Name.ToLowerInvariant()}_{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Add Composite Role (1-*) [object table]
            var sql = $@"UPDATE {table}
SET {this.columnNameByRelationType[relationType]} = r.{this.TableTypeColumnNameForAssociation}
FROM {table}
INNER JOIN {this.ParamNameForTableType} AS r
ON {ColumnNameForObject} = r.{this.TableTypeColumnNameForRole}";
            this.commandTextForAddRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForCompositeRelation} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void RemoveCompositeRoleObjectTable(IAssociationType associationType, IClass @class)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForRemoveRole}{@class.Name.ToLowerInvariant()}_{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Remove Composite Role (1-*) [object table]
            var sql = $@"UPDATE {table}
SET {this.columnNameByRelationType[relationType]} = null
FROM {table}
INNER JOIN {this.ParamNameForTableType} AS r
ON
    {this.columnNameByRelationType[relationType]} = r.{this.TableTypeColumnNameForAssociation} AND
    {ColumnNameForObject} = r.{this.TableTypeColumnNameForRole}";
            this.commandTextForRemoveRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForCompositeRelation} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void ClearCompositeRoleObjectTable(IAssociationType associationType, IClass @class)
        {
            var relationType = associationType.RelationType;
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForClearRole}{@class.Name.ToLowerInvariant()}_{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Clear Composites Role (1-*) [object table]
            var sql = $@"UPDATE {this.tableNameForObjectByClass[@class]}
SET {this.columnNameByRelationType[relationType]} = null
FROM {this.tableNameForObjectByClass[@class]}
INNER JOIN {this.ParamNameForTableType} AS a
ON {this.columnNameByRelationType[relationType]} = a.{this.TableTypeColumnNameForObject}";
            this.commandTextForClearRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void SetUnitRoleType(IRoleType roleType, IClass @class)
        {
            if (!this.commandTextForSetUnitRoleByRelationTypeByClass.TryGetValue(@class, out var commandTextForSetUnitRoleByRelationType))
            {
                commandTextForSetUnitRoleByRelationType = new Dictionary<IRelationType, string>();
                this.commandTextForSetUnitRoleByRelationTypeByClass.Add(@class, commandTextForSetUnitRoleByRelationType);
            }

            var relationType = roleType.RelationType;
            var unitTypeTag = ((IUnit)relationType.RoleType.ObjectType).Tag;
            var table = this.tableNameForObjectByClass[@class];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForSetRole}{@class.Name.ToLowerInvariant()}_{roleType.SingularFullName.ToLowerInvariant()}";

            var tableTypeName = unitTypeTag switch
            {
                UnitTags.String => this.TableTypeNameForStringRelation,
                UnitTags.Integer => this.TableTypeNameForIntegerRelation,
                UnitTags.Float => this.TableTypeNameForFloatRelation,
                UnitTags.Decimal => this.TableTypeNameForDecimalRelationByScaleByPrecision[roleType.Precision.Value][roleType.Scale.Value],
                UnitTags.Boolean => this.TableTypeNameForBooleanRelation,
                UnitTags.DateTime => this.TableTypeNameForDateTimeRelation,
                UnitTags.Unique => this.TableTypeNameForUniqueRelation,
                UnitTags.Binary => this.TableTypeNameForBinaryRelation,
                _ => throw new ArgumentException($"Unknown Unit ObjectType: {roleType.ObjectType.SingularName}")
            };

            var sql = $@"UPDATE {table}
SET {this.columnNameByRelationType[relationType]} = r.{this.TableTypeColumnNameForRole}
FROM {table}
INNER JOIN {this.ParamNameForTableType} AS r
ON {ColumnNameForObject} = r.{this.TableTypeColumnNameForAssociation}";
            commandTextForSetUnitRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {tableTypeName} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositeRoleObjectTable(IRoleType roleType, IClass @class)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForGetRole}{roleType.SingularFullName.ToLowerInvariant()}";

            // Get Composite Role (1-1 and *-1) [object table]
            var sql = $@"SELECT {this.columnNameByRelationType[relationType]}
FROM {table}
WHERE {ColumnNameForObject}={this.ParamNameForAssociation}";
            this.commandTextForGetRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForAssociation} {SqlTypeForObject}
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositeRoleObjectTable(IRoleType roleType, IClass @class)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForPrefetchRole}{roleType.SingularFullName.ToLowerInvariant()}";

            // Prefetch Composite Role (1-1 and *-1) [object table]
            var sql = $@"SELECT  {ColumnNameForObject}, {this.columnNameByRelationType[relationType]}
FROM {table}
WHERE {ColumnNameForObject} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositeAssociationOne2OneObjectTable(IRoleType roleType, IClass @class)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForGetAssociation}{@class.Name.ToLowerInvariant()}_{roleType.SingularFullName.ToLowerInvariant()}";

            // Get Composite Association (1-1) [object table]
            var sql = $@"SELECT {ColumnNameForObject}
FROM {table}
WHERE {this.columnNameByRelationType[relationType]}={this.ParamNameForCompositeRole}";
            this.commandTextForGetAssociationByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForCompositeRole} {SqlTypeForObject}
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositeAssociationObjectTable(IRoleType roleType, IClass @class)
        {
            var table = this.tableNameForObjectByClass[@class];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForPrefetchAssociation}{@class.Name.ToLowerInvariant()}_{roleType.SingularFullName.ToLowerInvariant()}";

            // Prefetch Composite Association (1-1) [object table]
            var sql = $@"SELECT {ColumnNameForObject}, {this.columnNameByRelationType[roleType.RelationType]}
FROM {table}
WHERE {this.columnNameByRelationType[roleType.RelationType]} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchAssociationByRelationType.Add(roleType.RelationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositesAssociationMany2OneObjectTable(IRoleType roleType, IClass @class)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForGetAssociation}{@class.Name.ToLowerInvariant()}_{roleType.SingularFullName.ToLowerInvariant()}";

            // Get Composite Association (*-1) [object table]
            var sql = $@"SELECT {ColumnNameForObject}
FROM {table}
WHERE {this.columnNameByRelationType[relationType]}={this.ParamNameForCompositeRole}";
            this.commandTextForGetAssociationByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForCompositeRole} {SqlTypeForObject}
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositesAssociationMany2OneObjectTable(IRoleType roleType, IClass @class)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForPrefetchAssociation}{@class.Name.ToLowerInvariant()}_{roleType.SingularFullName.ToLowerInvariant()}";

            // Prefetch Composite Association (*-1) [object table]
            var sql = $@"SELECT {ColumnNameForObject}, {this.columnNameByRelationType[relationType]}
FROM {table}
WHERE {this.columnNameByRelationType[relationType]} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void SetCompositeRole(IRoleType roleType, IClass @class)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForSetRole}{@class.Name.ToLowerInvariant()}_{roleType.SingularFullName.ToLowerInvariant()}";

            // Set Composite Role (1-1 and *-1) [object table]
            var sql = $@"UPDATE {table}
SET {this.columnNameByRelationType[relationType]} = r.{this.TableTypeColumnNameForRole}
FROM {table}
INNER JOIN {this.ParamNameForTableType} AS r
ON {ColumnNameForObject} = r.{this.TableTypeColumnNameForAssociation}";
            this.commandTextForSetRoleByRelationType.Add(relationType, sql);

            var definition = $@"CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForCompositeRelation} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void ClearCompositeRole(IRoleType roleType, IClass @class)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForClearRole}{@class.Name.ToLowerInvariant()}_{roleType.SingularFullName.ToLowerInvariant()}";

            // Clear Composite Role (1-1 and *-1) [object table]
            var sql = $@"UPDATE {table}
SET {this.columnNameByRelationType[relationType]} = null
FROM {table}
INNER JOIN {this.ParamNameForTableType} AS a
ON {ColumnNameForObject} = a.{this.TableTypeColumnNameForObject}";
            this.commandTextForClearRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositesRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForGetRole}{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Get Composites Role (1-* and *-*) [relation table]
            var sql = $@"SELECT {ColumnNameForRole}
FROM {table}
WHERE {ColumnNameForAssociation}={this.ParamNameForAssociation}";
            this.commandTextForGetRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForAssociation} {SqlTypeForObject}
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositesRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForPrefetchRole}{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Prefetch Composites Role (1-* and *-*) [relation table]
            var sql = $@"SELECT {ColumnNameForAssociation}, {ColumnNameForRole}
FROM {table}
WHERE {ColumnNameForAssociation} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void AddCompositeRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForAddRole}{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Add Composite Role (1-* and *-*) [relation table]
            var sql = $@"INSERT INTO {table} ({ColumnNameForAssociation},{ColumnNameForRole})
SELECT {this.TableTypeColumnNameForAssociation}, {this.TableTypeColumnNameForRole}
FROM {this.ParamNameForTableType}";
            this.commandTextForAddRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForCompositeRelation} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void RemoveCompositeRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForRemoveRole}{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Remove Composite Role (1-* and *-*) [relation table]
            var sql = $@"DELETE T
FROM {table} T
INNER JOIN {this.ParamNameForTableType} R
ON T.{ColumnNameForAssociation} = R.{this.TableTypeColumnNameForAssociation}
AND T.{ColumnNameForRole} = R.{this.TableTypeColumnNameForRole};";
            this.commandTextForRemoveRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForCompositeRelation} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositeRoleRelationTable(IRelationType relationType)
        {
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForGetRole}{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Get Composite Role (1-1 and *-1) [relation table]
            var sql = $@"SELECT {ColumnNameForRole}
FROM {this.tableNameForRelationByRelationType[relationType]}
WHERE {ColumnNameForAssociation}={this.ParamNameForAssociation}";
            this.commandTextForGetRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForAssociation} {SqlTypeForObject}
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositeRoleRelationType(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForPrefetchRole}{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Prefetch Composite Role (1-1 and *-1) [relation table]
            var sql = $@"SELECT {ColumnNameForAssociation}, {ColumnNameForRole}
FROM {table}
WHERE {ColumnNameForAssociation} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";
            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void SetCompositeRoleRelationType(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForSetRole}{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Set Composite Role (1-1 and *-1) [relation table]
            var sql = $@"MERGE {table} T
USING {this.ParamNameForTableType} AS r
ON T.{ColumnNameForAssociation} = r.{this.TableTypeColumnNameForAssociation}

WHEN MATCHED THEN
UPDATE SET {ColumnNameForRole}= r.{this.TableTypeColumnNameForRole}

WHEN NOT MATCHED THEN
INSERT ({ColumnNameForAssociation},{ColumnNameForRole})
VALUES (r.{this.TableTypeColumnNameForAssociation}, r.{this.TableTypeColumnNameForRole});";
            this.commandTextForSetRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForCompositeRelation} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositeAssociationRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForGetAssociation}{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Get Composite Association (1-1) [relation table]
            var sql = $@"SELECT {ColumnNameForAssociation}
FROM {table}
WHERE {ColumnNameForRole}={this.ParamNameForCompositeRole}";
            this.commandTextForGetAssociationByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForCompositeRole} {SqlTypeForObject}
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositeAssociationRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var name = $"{this.Database.SchemaName}.{ProcedurePrefixForPrefetchAssociation}{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Prefetch Composite Association (1-1) [relation table]
            var sql = $@"SELECT {ColumnNameForAssociation},{ColumnNameForRole}
FROM {table}
WHERE {ColumnNameForRole} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositesAssociationRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var name =
                $"{this.Database.SchemaName}.{ProcedurePrefixForGetAssociation}{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Get Composite Association (*-1) [relation table]
            var sql = $@"SELECT {ColumnNameForAssociation}
FROM {table}
WHERE {ColumnNameForRole}={this.ParamNameForCompositeRole}";
            this.commandTextForGetAssociationByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForCompositeRole} {SqlTypeForObject}
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositesAssociationRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var name = $"{this.Database.SchemaName}.{ProcedurePrefixForPrefetchAssociation}{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Prefetch Composite Association (*-1) [relation table]
            var sql = $@"SELECT {ColumnNameForAssociation},{ColumnNameForRole}
FROM {table}
WHERE {ColumnNameForRole} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
   {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void ClearCompositeRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var name = $"{this.Database.SchemaName}.{ProcedurePrefixForClearRole}{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

            // Clear Composite Role (1-1 and *-1) [relation table]
            var sql = $@"DELETE T
FROM {table} T
INNER JOIN {this.ParamNameForTableType} A
ON T.{ColumnNameForAssociation} = A.{this.TableTypeColumnNameForObject}";
            this.commandTextForClearRoleByRelationType.Add(relationType, sql);

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void UpdateVersionIds()
        {
            var name = $"{this.Database.SchemaName}.{ProcedurePrefixForUpdateVersion}";

            // Update Version Ids
            var sql = $@"UPDATE {this.TableNameForObjects}
SET {ColumnNameForVersion} = {ColumnNameForVersion} + 1
FROM {this.TableNameForObjects}
WHERE {ColumnNameForObject} IN ( SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType});";
            this.commandTextForUpdateVersion = sql;

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetVersionIds()
        {
            var name = $"{this.Database.SchemaName}.{ProcedurePrefixForGetVersion}";

            // Get Version Ids
            var sql = $@"SELECT {ColumnNameForObject}, {ColumnNameForVersion}
FROM {this.TableNameForObjects}
WHERE {ColumnNameForObject} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForGetVersion = sql;

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void Instantiate()
        {
            var name = $"{this.Database.SchemaName}.i";

            // Instantiate
            var sql = $@"SELECT {ColumnNameForObject}, {ColumnNameForClass}, {ColumnNameForVersion}
FROM {this.TableNameForObjects}
WHERE {ColumnNameForObject} IN (SELECT {this.TableTypeColumnNameForObject} FROM {this.ParamNameForTableType})";
            this.commandTextForInstantiate = sql;

            var definition = $@"
CREATE PROCEDURE {name}
    {this.ParamNameForTableType} {this.TableTypeNameForObject} READONLY
AS
BEGIN
{sql}
END";

            this.ProcedureDefinitionByName.Add(name, definition);
        }
    }
}

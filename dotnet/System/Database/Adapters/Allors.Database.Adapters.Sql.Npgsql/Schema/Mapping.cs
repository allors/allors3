// <copyright file="Mapping.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using NpgsqlTypes;

    public class Mapping : Sql.Mapping
    {
        private readonly IDictionary<IRoleType, string> paramInvocationNameByRoleType;
        private readonly IDictionary<IClass, string> tableNameForObjectByClass;
        private readonly IDictionary<IRelationType, string> columnNameByRelationType;
        private readonly IDictionary<IRelationType, string> tableNameForRelationByRelationType;
        private readonly IDictionary<IClass, string> commandTextForCreateObjectByClass;
        private readonly IDictionary<IClass, string> commandTextForCreateObjectsByClass;
        private readonly IDictionary<IClass, string> commandTextForDeleteObjectByClass;
        private readonly IDictionary<IClass, string> commandTextForGetUnitRolesByClass;
        private readonly IDictionary<IClass, string> commandTextForPrefetchUnitRolesByClass;
        private readonly IDictionary<IClass, IDictionary<IRelationType, string>> commandTextForSetUnitRoleByRelationTypeByClass;
        private readonly IDictionary<IRelationType, string> commandTextForGetRoleByRelationType;
        private readonly IDictionary<IRelationType, string> commandTextForPrefetchRoleByRelationType;
        private readonly IDictionary<IRelationType, string> commandTextForSetRoleByRelationType;
        private readonly IDictionary<IRelationType, string> commandTextForAddRoleByRelationType;
        private readonly IDictionary<IRelationType, string> commandTextForRemoveRoleByRelationType;
        private readonly IDictionary<IRelationType, string> commandTextForClearRoleByRelationType;
        private readonly IDictionary<IRelationType, string> commandTextForGetAssociationByRelationType;
        private readonly IDictionary<IRelationType, string> commandTextForPrefetchAssociationByRelationType;
        private string commandTextForInstantiate;
        private string commandTextForGetVersion;
        private string commandTextForUpdateVersion;

        public override string ParamInvocationFormat => ParameterInvocationFormat;
        public override string ParamInvocationNameForObject { get; }
        public override string ParamInvocationNameForClass { get; }

        public override IDictionary<IRoleType, string> ParamInvocationNameByRoleType => this.paramInvocationNameByRoleType;

        public override string TableNameForObjects { get; }
        public override IDictionary<IClass, string> TableNameForObjectByClass => this.tableNameForObjectByClass;
        public override IDictionary<IRelationType, string> ColumnNameByRelationType => this.columnNameByRelationType;
        public override IDictionary<IRelationType, string> TableNameForRelationByRelationType => this.tableNameForRelationByRelationType;

        public override IDictionary<IClass, string> CommandTextForCreateObjectByClass => this.commandTextForCreateObjectByClass;
        public override IDictionary<IClass, string> CommandTextForCreateObjectsByClass => this.commandTextForCreateObjectsByClass;
        public override IDictionary<IClass, string> CommandTextForDeleteObjectByClass => this.commandTextForDeleteObjectByClass;
        public override IDictionary<IClass, string> CommandTextForGetUnitRolesByClass => this.commandTextForGetUnitRolesByClass;
        public override IDictionary<IClass, string> CommandTextForPrefetchUnitRolesByClass => this.commandTextForPrefetchUnitRolesByClass;
        public override IDictionary<IClass, IDictionary<IRelationType, string>> CommandTextForSetUnitRoleByRelationTypeByClass => this.commandTextForSetUnitRoleByRelationTypeByClass;
        public override IDictionary<IRelationType, string> CommandTextForGetRoleByRelationType => this.commandTextForGetRoleByRelationType;
        public override IDictionary<IRelationType, string> CommandTextForPrefetchRoleByRelationType => this.commandTextForPrefetchRoleByRelationType;
        public override IDictionary<IRelationType, string> CommandTextForSetRoleByRelationType => this.commandTextForSetRoleByRelationType;
        public override IDictionary<IRelationType, string> CommandTextForAddRoleByRelationType => this.commandTextForAddRoleByRelationType;
        public override IDictionary<IRelationType, string> CommandTextForRemoveRoleByRelationType => this.commandTextForRemoveRoleByRelationType;
        public override IDictionary<IRelationType, string> CommandTextForClearRoleByRelationType => this.commandTextForClearRoleByRelationType;
        public override IDictionary<IRelationType, string> CommandTextForGetAssociationByRelationType => this.commandTextForGetAssociationByRelationType;
        public override IDictionary<IRelationType, string> CommandTextForPrefetchAssociationByRelationType => this.commandTextForPrefetchAssociationByRelationType;

        public override string StringCollation => string.Empty;
        public override string Ascending => "ASC NULLS FIRST";
        public override string Descending => "DESC NULLS LAST";

        public override string CommandTextForInstantiate => this.commandTextForInstantiate;
        public override string CommandTextForGetVersion => this.commandTextForGetVersion;
        public override string CommandTextForUpdateVersion => this.commandTextForUpdateVersion;

        internal const string SqlTypeForClass = "uuid";
        internal const string SqlTypeForObject = "bigint";
        internal const string SqlTypeForVersion = "bigint";

        internal const NpgsqlDbType NpgsqlDbTypeForClass = NpgsqlDbType.Uuid;
        internal const NpgsqlDbType NpgsqlDbTypeForObject = NpgsqlDbType.Bigint;
        internal const NpgsqlDbType NpgsqlDbTypeForCount = NpgsqlDbType.Integer;

        internal MappingArrayParameter ObjectArrayParam { get; }
        internal MappingArrayParameter RoleArrayParam { get; }

        internal string ParamInvocationNameForAssociation { get; }
        internal string ParamInvocationNameForCompositeRole { get; }
        internal string ParamInvocationNameForCount { get; }

        private const string ParameterInvocationFormat = ":p_{0}";

        public Mapping(Database database)
        {
            this.Database = database;

            this.ParamInvocationNameForObject = string.Format(ParameterInvocationFormat, ColumnNameForObject);
            this.ParamInvocationNameForClass = string.Format(ParameterInvocationFormat, ColumnNameForClass);

            this.ParamInvocationNameForAssociation = string.Format(ParameterInvocationFormat, AliasForAssociation);
            this.ParamInvocationNameForCompositeRole = string.Format(ParameterInvocationFormat, AliasForRole);
            this.ParamInvocationNameForCount = string.Format(ParameterInvocationFormat, "count");

            this.ObjectArrayParam = new MappingArrayParameter(this, "arr_o");
            this.RoleArrayParam = new MappingArrayParameter(this, "arr_r");

            // Tables
            // ------
            this.TableNameForObjects = database.SchemaName + "." + ObjectsTableName;
            this.tableNameForObjectByClass = new Dictionary<IClass, string>();
            this.columnNameByRelationType = new Dictionary<IRelationType, string>();
            this.paramInvocationNameByRoleType = new Dictionary<IRoleType, string>();

            foreach (var @class in this.Database.MetaPopulation.DatabaseClasses)
            {
                this.tableNameForObjectByClass.Add(@class, this.Database.SchemaName + "." + TableNameFor(@class));

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
                        this.paramInvocationNameByRoleType[roleType] = string.Format(ParameterInvocationFormat, ModelId.Encode(relationType.Id));
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
                    this.tableNameForRelationByRelationType.Add(relationType, this.Database.SchemaName + "." + TableNameFor(relationType));
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

            this.CommandTextForLoadSetUnitRoleByRelationTypeByClass = new Dictionary<IClass, Dictionary<IRelationType, string>>();
            this.CommandTextForLoadSetRoleByRelationType = new Dictionary<IRelationType, string>();
            this.CommandTextForLoadAddRoleByRelationType = new Dictionary<IRelationType, string>();

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
                        this.GetCompositesRoleObjectTable(@class, associationType);
                        this.PrefetchCompositesRoleObjectTable(@class, associationType);

                        if (associationType.IsOne)
                        {
                            this.GetCompositeAssociationObjectTable(@class, associationType);
                            this.PrefetchCompositeAssociationObjectTable(@class, associationType);
                        }

                        this.AddCompositeRoleObjectTable(@class, associationType);
                        this.RemoveCompositeRoleObjectTable(@class, associationType);
                        this.ClearCompositeRoleObjectTable(@class, associationType);
                    }
                }

                foreach (var roleType in @class.DatabaseRoleTypes)
                {
                    if (roleType.ObjectType.IsUnit)
                    {
                        this.SetUnitRoleType(@class, roleType);
                    }
                    else if (!(roleType.AssociationType.IsMany && roleType.IsMany) && roleType.RelationType.ExistExclusiveDatabaseClasses && roleType.IsOne)
                    {
                        this.GetCompositeRoleObjectTable(@class, roleType);
                        this.PrefetchCompositeRoleObjectTable(@class, roleType);

                        if (roleType.AssociationType.IsOne)
                        {
                            this.GetCompositeAssociationOne2OneObjectTable(@class, roleType);
                            this.PrefetchCompositeAssociationObjectTable(@class, roleType);
                        }
                        else
                        {
                            this.GetCompositesAssociationMany2OneObjectTable(@class, roleType);
                            this.PrefetchCompositesAssociationMany2OneObjectTable(@class, roleType);
                        }

                        this.SetCompositeRole(@class, roleType);
                        this.ClearCompositeRole(@class, roleType);
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

        // A load sends each of its statements once, with all the relations that the statement sets in its arrays. Its
        // statements take the arrays as they are, not behind a sub-select, so that PostgreSQL plans a statement for the
        // arrays that it gets.
        internal Dictionary<IClass, Dictionary<IRelationType, string>> CommandTextForLoadSetUnitRoleByRelationTypeByClass { get; }

        internal Dictionary<IRelationType, string> CommandTextForLoadSetRoleByRelationType { get; }

        internal Dictionary<IRelationType, string> CommandTextForLoadAddRoleByRelationType { get; }

        protected internal Database Database { get; }

        internal string NormalizeName(string name)
        {
            name = name.ToLowerInvariant();
            if (ReservedWords.Names.Contains(name))
            {
                return "\"" + name + "\"";
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
                        return "text";
                    }

                    return "varchar(" + roleType.Size + ")";

                case UnitTags.Integer:
                    return "integer";

                case UnitTags.Decimal:
                    return "numeric(" + roleType.Precision + "," + roleType.Scale + ")";

                case UnitTags.Float:
                    return "double precision";

                case UnitTags.Boolean:
                    return "boolean";

                case UnitTags.DateTime:
                    return "timestamp";

                case UnitTags.Unique:
                    return "uuid";

                case UnitTags.Binary:
                    return "bytea";

                default:
                    return "!UNKNOWN VALUE TYPE!";
            }
        }

        internal NpgsqlDbType GetNpgsqlDbType(IRoleType roleType)
        {
            var unit = (IUnit)roleType.ObjectType;
            switch (unit.Tag)
            {
                case UnitTags.String:
                    return NpgsqlDbType.Varchar;

                case UnitTags.Integer:
                    return NpgsqlDbType.Integer;

                case UnitTags.Decimal:
                    return NpgsqlDbType.Numeric;

                case UnitTags.Float:
                    return NpgsqlDbType.Double;

                case UnitTags.Boolean:
                    return NpgsqlDbType.Boolean;

                case UnitTags.DateTime:
                    return NpgsqlDbType.Timestamp;

                case UnitTags.Unique:
                    return NpgsqlDbType.Uuid;

                case UnitTags.Binary:
                    return NpgsqlDbType.Bytea;

                default:
                    throw new Exception("Unknown Unit Type");
            }
        }

        // In a statement an array sits behind a sub-select: PostgreSQL then plans the statement without the values of
        // the array. A plan for the values of one execution can scan a whole table when the array is long.
        private static string SubSelect(MappingArrayParameter array) => "(SELECT " + array.InvocationName + ")";

        private void CreateObject(IClass @class)
        {
            var table = this.tableNameForObjectByClass[@class];

            // CreateObject
            // One statement creates the object: the row of the objects table gives its id to the row of the table
            // of the class.
            var sql = $@"WITH created AS (
    INSERT INTO {this.TableNameForObjects} ({ColumnNameForClass}, {ColumnNameForVersion})
    VALUES ({this.ParamInvocationNameForClass}, {(long)Allors.Version.DatabaseInitial})
    RETURNING {ColumnNameForObject})
INSERT INTO {table} ({ColumnNameForObject}, {ColumnNameForClass})
SELECT {ColumnNameForObject}, {this.ParamInvocationNameForClass}
FROM created
RETURNING {ColumnNameForObject}";
            this.commandTextForCreateObjectByClass.Add(@class, sql);
        }

        private void CreateObjects(IClass @class)
        {
            // CreateObjects
            // One statement creates the objects: the rows of the objects table give their ids to the rows of the
            // table of the class. A count of zero or below creates nothing.
            var sql = $@"WITH created AS (
    INSERT INTO {this.TableNameForObjects} ({ColumnNameForClass}, {ColumnNameForVersion})
    SELECT {this.ParamInvocationNameForClass}, {(long)Allors.Version.DatabaseInitial}
    FROM generate_series(1, {this.ParamInvocationNameForCount})
    RETURNING {ColumnNameForObject})
INSERT INTO {this.tableNameForObjectByClass[@class.ExclusiveDatabaseClass]} ({ColumnNameForObject}, {ColumnNameForClass})
SELECT {ColumnNameForObject}, {this.ParamInvocationNameForClass}
FROM created
RETURNING {ColumnNameForObject}";
            this.commandTextForCreateObjectsByClass.Add(@class, sql);
        }

        private void DeleteObject(IClass @class)
        {
            var table = this.tableNameForObjectByClass[@class];

            // One statement deletes the object from the objects table and from the table of its class.
            var sql = $@"WITH deleted AS (
    DELETE FROM {this.TableNameForObjects}
    WHERE {ColumnNameForObject}={this.ParamInvocationNameForObject})
DELETE FROM {table}
WHERE {ColumnNameForObject}={this.ParamInvocationNameForObject}";
            this.commandTextForDeleteObjectByClass.Add(@class, sql);
        }

        private void GetUnitRoles(IClass @class)
        {
            var sortedUnitRoleTypes = this.Database.GetSortedUnitRolesByObjectType(@class);

            // Get Unit Roles
            var sql = $@"SELECT {string.Join(", ", sortedUnitRoleTypes.Select(v => this.columnNameByRelationType[v.RelationType]))}
    FROM {this.tableNameForObjectByClass[@class.ExclusiveDatabaseClass]}
    WHERE {ColumnNameForObject}={this.ParamInvocationNameForObject}";
            this.commandTextForGetUnitRolesByClass.Add(@class, sql);
        }

        private void PrefetchUnitRoles(IClass @class)
        {
            var sortedUnitRoleTypes = this.Database.GetSortedUnitRolesByObjectType(@class);
            var table = this.tableNameForObjectByClass[@class.ExclusiveDatabaseClass];
            var objects = this.ObjectArrayParam;

            var sql = $@"WITH objects AS (SELECT UNNEST({SubSelect(objects)}) AS {ColumnNameForObject})

    SELECT {ColumnNameForObject}, {string.Join(", ", sortedUnitRoleTypes.Select(v => this.columnNameByRelationType[v.RelationType]))}
    FROM {table}
    WHERE {ColumnNameForObject} IN (SELECT {ColumnNameForObject} FROM objects)";
            this.commandTextForPrefetchUnitRolesByClass.Add(@class, sql);
        }

        private void GetCompositesRoleObjectTable(IClass @class, IAssociationType associationType)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Get Composites Role (1-*) [object table]
            var sql = $@"SELECT {ColumnNameForObject}
    FROM {table}
    WHERE {this.columnNameByRelationType[relationType]}={this.ParamInvocationNameForAssociation}";
            this.commandTextForGetRoleByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositesRoleObjectTable(IClass @class, IAssociationType associationType)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;

            // Prefetch Composites Role (1-*) [object table]
            var sql = $@"WITH objects AS (SELECT UNNEST({SubSelect(objects)}) AS {ColumnNameForObject})

    SELECT {this.columnNameByRelationType[relationType]}, {ColumnNameForObject}
    FROM {table}
    WHERE {this.columnNameByRelationType[relationType]} IN (SELECT {ColumnNameForObject} FROM objects)";
            this.commandTextForPrefetchRoleByRelationType.Add(relationType, sql);
        }

        private void GetCompositeAssociationObjectTable(IClass @class, IAssociationType associationType)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Get Composite Association (1-*) [object table]
            var sql = $@"SELECT {this.columnNameByRelationType[relationType]}
    FROM {table}
    WHERE {ColumnNameForObject}={this.ParamInvocationNameForCompositeRole}";
            this.commandTextForGetAssociationByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositeAssociationObjectTable(IClass @class, IAssociationType associationType)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;

            // Prefetch Composite Association (1-*) [object table]
            var sql = $@"WITH objects AS (SELECT UNNEST({SubSelect(objects)}) AS {ColumnNameForObject})

    SELECT {this.columnNameByRelationType[relationType]}, {ColumnNameForObject}
    FROM {table}
    WHERE {ColumnNameForObject} IN (SELECT {ColumnNameForObject} FROM objects)";
            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, sql);
        }

        private void AddCompositeRoleObjectTable(IClass @class, IAssociationType associationType)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;
            var roles = this.RoleArrayParam;

            // Add Composite Role (1-*) [object table]
            string Sql(string objectArray, string roleArray) => $@"WITH relations AS (SELECT UNNEST({objectArray}) AS {AliasForAssociation}, UNNEST({roleArray}) AS {AliasForRole})

    UPDATE {table}
    SET {this.columnNameByRelationType[relationType]} = relations.{AliasForAssociation}
    FROM relations
    WHERE {table}.{ColumnNameForObject} = relations.{AliasForRole}";

            this.commandTextForAddRoleByRelationType.Add(relationType, Sql(SubSelect(objects), SubSelect(roles)));
            this.CommandTextForLoadAddRoleByRelationType.Add(relationType, Sql(objects.InvocationName, roles.InvocationName));
        }

        private void RemoveCompositeRoleObjectTable(IClass @class, IAssociationType associationType)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;
            var roles = this.RoleArrayParam;

            // Remove Composite Role (1-*) [object table]
            var sql = $@"WITH relations AS (SELECT UNNEST({SubSelect(objects)}) AS {AliasForAssociation}, UNNEST({SubSelect(roles)}) AS {AliasForRole})

    UPDATE {table}
    SET {this.columnNameByRelationType[relationType]} = null
    FROM relations
    WHERE {table}.{this.columnNameByRelationType[relationType]} = relations.{AliasForAssociation} AND
          {table}.{ColumnNameForObject} = relations.{AliasForRole}";
            this.commandTextForRemoveRoleByRelationType.Add(relationType, sql);
        }

        private void ClearCompositeRoleObjectTable(IClass @class, IAssociationType associationType)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;

            // Clear Composites Role (1-*) [object table]
            var sql = $@"WITH objects AS (SELECT UNNEST({SubSelect(objects)}) AS {ColumnNameForObject})

    UPDATE {table}
    SET {this.columnNameByRelationType[relationType]} = null
    FROM objects
    WHERE {table}.{this.columnNameByRelationType[relationType]} = objects.{ColumnNameForObject}";
            this.commandTextForClearRoleByRelationType.Add(relationType, sql);
        }

        private void SetUnitRoleType(IClass @class, IRoleType roleType)
        {
            if (!this.commandTextForSetUnitRoleByRelationTypeByClass.TryGetValue(@class, out var commandTextForSetUnitRoleByRelationType))
            {
                commandTextForSetUnitRoleByRelationType = new Dictionary<IRelationType, string>();
                this.commandTextForSetUnitRoleByRelationTypeByClass.Add(@class, commandTextForSetUnitRoleByRelationType);
            }

            if (!this.CommandTextForLoadSetUnitRoleByRelationTypeByClass.TryGetValue(@class, out var commandTextForLoadSetUnitRoleByRelationType))
            {
                commandTextForLoadSetUnitRoleByRelationType = new Dictionary<IRelationType, string>();
                this.CommandTextForLoadSetUnitRoleByRelationTypeByClass.Add(@class, commandTextForLoadSetUnitRoleByRelationType);
            }

            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;
            var roles = this.RoleArrayParam;

            string Sql(string objectArray, string roleArray) => $@"WITH relations AS (SELECT UNNEST({objectArray}) AS {AliasForAssociation}, UNNEST({roleArray}) AS {AliasForRole})

    UPDATE {table}
    SET {this.columnNameByRelationType[relationType]} = relations.{AliasForRole}
    FROM relations
    WHERE {ColumnNameForObject} = relations.{AliasForAssociation}";

            commandTextForSetUnitRoleByRelationType.Add(relationType, Sql(SubSelect(objects), SubSelect(roles)));
            commandTextForLoadSetUnitRoleByRelationType.Add(relationType, Sql(objects.InvocationName, roles.InvocationName));
        }

        private void GetCompositeRoleObjectTable(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Get Composite Role (1-1 and *-1) [object table]
            var sql = $@"SELECT {this.columnNameByRelationType[relationType]}
    FROM {table}
    WHERE {ColumnNameForObject}={this.ParamInvocationNameForAssociation}";
            this.commandTextForGetRoleByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositeRoleObjectTable(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;

            // Prefetch Composite Role (1-1 and *-1) [object table]
            var sql = $@"WITH objects AS (SELECT UNNEST({SubSelect(objects)}) AS {ColumnNameForObject})

    SELECT {ColumnNameForObject}, {this.columnNameByRelationType[relationType]}
    FROM {table}
    WHERE {ColumnNameForObject} IN (SELECT {ColumnNameForObject} FROM objects)";
            this.commandTextForPrefetchRoleByRelationType.Add(relationType, sql);
        }

        private void GetCompositeAssociationOne2OneObjectTable(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Get Composite Association (1-1) [object table]
            var sql = $@"SELECT {ColumnNameForObject}
    FROM {table}
    WHERE {this.columnNameByRelationType[relationType]}={this.ParamInvocationNameForCompositeRole}";
            this.commandTextForGetAssociationByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositeAssociationObjectTable(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;

            // Prefetch Composite Association (1-1) [object table]
            var sql = $@"WITH objects AS (SELECT UNNEST({SubSelect(objects)}) AS {ColumnNameForObject})

    SELECT {ColumnNameForObject}, {this.columnNameByRelationType[relationType]}
    FROM {table}
    WHERE {this.columnNameByRelationType[relationType]} IN (SELECT {ColumnNameForObject} FROM objects)";
            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, sql);
        }

        private void GetCompositesAssociationMany2OneObjectTable(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];

            // Get Composite Association (*-1) [object table]
            var sql = $@"SELECT {ColumnNameForObject}
    FROM {table}
    WHERE {this.columnNameByRelationType[relationType]}={this.ParamInvocationNameForCompositeRole}";
            this.commandTextForGetAssociationByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositesAssociationMany2OneObjectTable(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;

            // Prefetch Composite Association (*-1) [object table]
            var sql = $@"WITH objects AS (SELECT UNNEST({SubSelect(objects)}) AS {ColumnNameForObject})

    SELECT {ColumnNameForObject}, {this.columnNameByRelationType[relationType]}
    FROM {table}
    WHERE {this.columnNameByRelationType[relationType]} IN (SELECT {ColumnNameForObject} FROM objects)";
            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, sql);
        }

        private void SetCompositeRole(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;
            var roles = this.RoleArrayParam;

            // Set Composite Role (1-1 and *-1) [object table]
            string Sql(string objectArray, string roleArray) => $@"WITH relations AS (SELECT UNNEST({objectArray}) AS {AliasForAssociation}, UNNEST({roleArray}) AS {AliasForRole})

    UPDATE {table}
    SET {this.columnNameByRelationType[relationType]} = relations.{AliasForRole}
    FROM relations
    WHERE {ColumnNameForObject} = relations.{AliasForAssociation}";

            this.commandTextForSetRoleByRelationType.Add(relationType, Sql(SubSelect(objects), SubSelect(roles)));
            this.CommandTextForLoadSetRoleByRelationType.Add(relationType, Sql(objects.InvocationName, roles.InvocationName));
        }

        private void ClearCompositeRole(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;

            // Clear Composite Role (1-1 and *-1) [object table]
            var sql = $@"WITH objects AS (SELECT UNNEST({SubSelect(objects)}) AS {ColumnNameForObject})

    UPDATE {table}
    SET {this.columnNameByRelationType[relationType]} = null
    FROM objects
    WHERE {table}.{ColumnNameForObject} = objects.{ColumnNameForObject}";
            this.commandTextForClearRoleByRelationType.Add(relationType, sql);
        }

        private void GetCompositesRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);

            // Get Composites Role (1-* and *-*) [relation table]
            var sql = $@"SELECT {role}
    FROM {table}
    WHERE {association}={this.ParamInvocationNameForAssociation}";
            this.commandTextForGetRoleByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositesRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var objectsArray = this.ObjectArrayParam;

            // Prefetch Composites Role (1-* and *-*) [relation table]
            var sql = $@"WITH objects AS (SELECT UNNEST({SubSelect(objectsArray)}) AS {ColumnNameForObject})

    SELECT {association}, {role}
    FROM {table}
    WHERE {association} IN (SELECT {ColumnNameForObject} FROM objects)";
            this.commandTextForPrefetchRoleByRelationType.Add(relationType, sql);
        }

        private void AddCompositeRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var objects = this.ObjectArrayParam;
            var roles = this.RoleArrayParam;

            // Add Composite Role (1-* and *-*) [relation table]
            string Sql(string objectArray, string roleArray) => $@"WITH relations AS (SELECT UNNEST({objectArray}) AS {AliasForAssociation}, UNNEST({roleArray}) AS {AliasForRole})

    INSERT INTO {table} ({association},{role})
    SELECT {AliasForAssociation}, {AliasForRole}
    FROM relations";

            this.commandTextForAddRoleByRelationType.Add(relationType, Sql(SubSelect(objects), SubSelect(roles)));
            this.CommandTextForLoadAddRoleByRelationType.Add(relationType, Sql(objects.InvocationName, roles.InvocationName));
        }

        private void RemoveCompositeRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var objects = this.ObjectArrayParam;
            var roles = this.RoleArrayParam;

            // Remove Composite Role (1-* and *-*) [relation table]
            var sql = $@"WITH relations AS (SELECT UNNEST({SubSelect(objects)}) AS {AliasForAssociation}, UNNEST({SubSelect(roles)}) AS {AliasForRole})

    DELETE FROM {table}
    USING relations
    WHERE {table}.{association}=relations.{AliasForAssociation} AND {table}.{role}=relations.{AliasForRole}";
            this.commandTextForRemoveRoleByRelationType.Add(relationType, sql);
        }

        private void GetCompositeRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);

            // Get Composite Role (1-1 and *-1) [relation table]
            var sql = $@"SELECT {role}
    FROM {table}
    WHERE {association}={this.ParamInvocationNameForAssociation}";
            this.commandTextForGetRoleByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositeRoleRelationType(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var objects = this.ObjectArrayParam;

            // Prefetch Composite Role (1-1 and *-1) [relation table]
            var sql = $@"WITH objects AS (SELECT UNNEST({SubSelect(objects)}) AS {ColumnNameForObject})

    SELECT {association}, {role}
    FROM {table}
    WHERE {association} IN (SELECT {ColumnNameForObject} FROM objects)";
            this.commandTextForPrefetchRoleByRelationType.Add(relationType, sql);
        }

        private void SetCompositeRoleRelationType(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var objects = this.ObjectArrayParam;
            var roles = this.RoleArrayParam;

            // Set Composite Role (1-1 and *-1) [relation table]
            string Sql(string objectArray, string roleArray) => $@"WITH relations AS (SELECT UNNEST({objectArray}) AS {AliasForAssociation}, UNNEST({roleArray}) AS {AliasForRole})

    INSERT INTO {table} ({association}, {role})
    SELECT {AliasForAssociation}, {AliasForRole} from relations

    ON CONFLICT ({association})
    DO UPDATE
        SET {role} = excluded.{role}";

            this.commandTextForSetRoleByRelationType.Add(relationType, Sql(SubSelect(objects), SubSelect(roles)));
            this.CommandTextForLoadSetRoleByRelationType.Add(relationType, Sql(objects.InvocationName, roles.InvocationName));
        }

        private void GetCompositeAssociationRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);

            // Get Composite Association (1-1) [relation table]
            var sql = $@"SELECT {association}
    FROM {table}
    WHERE {role}={this.ParamInvocationNameForCompositeRole}";
            this.commandTextForGetAssociationByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositeAssociationRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var objects = this.ObjectArrayParam;

            // Prefetch Composite Association (1-1) [relation table]
            var sql = $@"WITH objects AS (SELECT UNNEST({SubSelect(objects)}) AS {ColumnNameForObject})

    SELECT {association},{role}
    FROM {table}
    WHERE {role} IN (SELECT {ColumnNameForObject} FROM objects)";
            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, sql);
        }

        private void GetCompositesAssociationRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);

            // Get Composite Association (*-1) [relation table]
            var sql = $@"SELECT {association}
    FROM {table}
    WHERE {role}={this.ParamInvocationNameForCompositeRole}";
            this.commandTextForGetAssociationByRelationType.Add(relationType, sql);
        }

        private void PrefetchCompositesAssociationRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var association = RelationTableAssociationColumnNameFor(relationType);
            var role = RelationTableRoleColumnNameFor(relationType);
            var objects = this.ObjectArrayParam;

            // Prefetch Composite Association (*-1) [relation table]
            var sql = $@"WITH objects AS (SELECT UNNEST({SubSelect(objects)}) AS {ColumnNameForObject})

    SELECT {association},{role}
    FROM {table}
    WHERE {role} IN (SELECT {ColumnNameForObject} FROM objects)";
            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, sql);
        }

        private void ClearCompositeRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var association = RelationTableAssociationColumnNameFor(relationType);
            var objects = this.ObjectArrayParam;

            // Clear Composite Role (1-1 and *-1) [relation table]
            var sql = $@"WITH objects AS (SELECT UNNEST({SubSelect(objects)}) AS {ColumnNameForObject})

    DELETE FROM {table}
    WHERE {association} IN (SELECT {ColumnNameForObject} FROM objects)";
            this.commandTextForClearRoleByRelationType.Add(relationType, sql);
        }

        private void UpdateVersionIds()
        {
            // Update Version Ids
            var sql = $@"UPDATE {this.TableNameForObjects}
    SET {ColumnNameForVersion} = {ColumnNameForVersion} + 1
    WHERE {ColumnNameForObject} IN (SELECT {ColumnNameForObject} FROM unnest({SubSelect(this.ObjectArrayParam)}) as t({ColumnNameForObject}))";
            this.commandTextForUpdateVersion = sql;
        }

        private void GetVersionIds()
        {
            var objects = this.ObjectArrayParam;

            // Get Version Ids
            var sql = $@"WITH objects AS (SELECT UNNEST({SubSelect(objects)}) AS {ColumnNameForObject})

    SELECT {this.TableNameForObjects}.{ColumnNameForObject}, {this.TableNameForObjects}.{ColumnNameForVersion}
    FROM {this.TableNameForObjects}
    WHERE {this.TableNameForObjects}.{ColumnNameForObject} IN (SELECT {ColumnNameForObject} FROM objects)";
            this.commandTextForGetVersion = sql;
        }

        private void Instantiate()
        {
            var objects = this.ObjectArrayParam;

            // Instantiate
            var sql = $@"WITH objects AS (SELECT UNNEST({SubSelect(objects)}) AS {ColumnNameForObject})

    SELECT {ColumnNameForObject}, {ColumnNameForClass}, {ColumnNameForVersion}
    FROM {this.TableNameForObjects}
    WHERE {this.TableNameForObjects}.{ColumnNameForObject} IN (SELECT {ColumnNameForObject} FROM objects)";
            this.commandTextForInstantiate = sql;
        }
    }
}

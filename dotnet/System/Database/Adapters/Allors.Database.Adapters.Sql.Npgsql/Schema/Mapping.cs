// <copyright file="Mapping.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
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

        public override CommandType CommandType => CommandType.Text;

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
        private const string SqlTypeForCount = "integer";

        internal const NpgsqlDbType NpgsqlDbTypeForClass = NpgsqlDbType.Uuid;
        internal const NpgsqlDbType NpgsqlDbTypeForObject = NpgsqlDbType.Bigint;
        internal const NpgsqlDbType NpgsqlDbTypeForCount = NpgsqlDbType.Integer;

        internal MappingArrayParameter ObjectArrayParam { get; }
        private MappingArrayParameter CompositeRoleArrayParam { get; }
        internal MappingArrayParameter StringRoleArrayParam { get; }
        private MappingArrayParameter StringMaxRoleArrayParam { get; }
        private MappingArrayParameter IntegerRoleArrayParam { get; }
        private MappingArrayParameter DecimalRoleArrayParam { get; }
        private MappingArrayParameter DoubleRoleArrayParam { get; }
        private MappingArrayParameter BooleanRoleArrayParam { get; }
        private MappingArrayParameter DateTimeRoleArrayParam { get; }
        private MappingArrayParameter UniqueRoleArrayParam { get; }
        private MappingArrayParameter BinaryRoleArrayParam { get; }

        private string ParamNameForAssociation { get; }
        private string ParamNameForCompositeRole { get; }
        private string ParamNameForCount { get; }
        private string ParamNameForObject { get; }
        private string ParamNameForClass { get; }

        internal string ParamInvocationNameForAssociation { get; }
        internal string ParamInvocationNameForCompositeRole { get; }
        internal string ParamInvocationNameForCount { get; }

        private const string ProcedurePrefixForInstantiate = "i";
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

        internal const string ParameterFormat = "p_{0}";
        private const string ParameterInvocationFormat = ":p_{0}";

        public Mapping(Database database)
        {
            this.Database = database;

            this.ParamInvocationNameForObject = string.Format(ParameterInvocationFormat, ColumnNameForObject);
            this.ParamInvocationNameForClass = string.Format(ParameterInvocationFormat, ColumnNameForClass);

            this.ParamNameForAssociation = string.Format(ParameterFormat, ColumnNameForAssociation);
            this.ParamNameForCompositeRole = string.Format(ParameterFormat, ColumnNameForRole);
            this.ParamNameForCount = string.Format(ParameterFormat, "count");
            this.ParamNameForObject = string.Format(ParameterFormat, ColumnNameForObject);
            this.ParamNameForClass = string.Format(ParameterFormat, ColumnNameForClass);

            this.ParamInvocationNameForAssociation = string.Format(ParameterInvocationFormat, ColumnNameForAssociation);
            this.ParamInvocationNameForCompositeRole = string.Format(ParameterInvocationFormat, ColumnNameForRole);
            this.ParamInvocationNameForCount = string.Format(ParameterInvocationFormat, "count");

            this.ObjectArrayParam = new MappingArrayParameter(database, this, "arr_o", NpgsqlDbType.Bigint);
            this.CompositeRoleArrayParam = new MappingArrayParameter(database, this, "arr_r", NpgsqlDbType.Bigint);
            this.StringRoleArrayParam = new MappingArrayParameter(database, this, "arr_r", NpgsqlDbType.Varchar);
            this.StringMaxRoleArrayParam = new MappingArrayParameter(database, this, "arr_r", NpgsqlDbType.Text);
            this.IntegerRoleArrayParam = new MappingArrayParameter(database, this, "arr_r", NpgsqlDbType.Integer);
            this.DecimalRoleArrayParam = new MappingArrayParameter(database, this, "arr_r", NpgsqlDbType.Numeric);
            this.DoubleRoleArrayParam = new MappingArrayParameter(database, this, "arr_r", NpgsqlDbType.Double);
            this.BooleanRoleArrayParam = new MappingArrayParameter(database, this, "arr_r", NpgsqlDbType.Boolean);
            this.DateTimeRoleArrayParam = new MappingArrayParameter(database, this, "arr_r", NpgsqlDbType.Timestamp);
            this.UniqueRoleArrayParam = new MappingArrayParameter(database, this, "arr_r", NpgsqlDbType.Uuid);
            this.BinaryRoleArrayParam = new MappingArrayParameter(database, this, "arr_r", NpgsqlDbType.Bytea);

            // Tables
            // ------
            this.TableNameForObjects = database.SchemaName + "._o";
            this.tableNameForObjectByClass = new Dictionary<IClass, string>();
            this.columnNameByRelationType = new Dictionary<IRelationType, string>();
            this.paramInvocationNameByRoleType = new Dictionary<IRoleType, string>();

            foreach (var @class in this.Database.MetaPopulation.DatabaseClasses)
            {
                this.tableNameForObjectByClass.Add(@class, this.Database.SchemaName + "." + this.NormalizeName(@class.SingularName));

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
                        this.paramInvocationNameByRoleType[roleType] = string.Format(ParameterInvocationFormat, roleType.SingularFullName);
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
                    this.tableNameForRelationByRelationType.Add(relationType, this.Database.SchemaName + "." + this.NormalizeName(relationType.RoleType.SingularFullName));
                }
            }

            // Commands and procedures
            // -----------------------
            // The text of a command is a statement. The schema still holds a function for every command, which the
            // adapter does not call. Where a function holds one statement, its generator writes that statement once:
            // for the command with the parameters as a command passes them, and for the function with the parameters
            // as the function declares them.
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

            this.CommandTextForLoadSetUnitRoleByRelationTypeByClass = new Dictionary<IClass, Dictionary<IRelationType, string>>();
            this.CommandTextForLoadSetRoleByRelationType = new Dictionary<IRelationType, string>();
            this.CommandTextForLoadAddRoleByRelationType = new Dictionary<IRelationType, string>();

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

        internal Dictionary<string, string> ProcedureDefinitionByName { get; }

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

        private void LoadObjects(IClass @class)
        {
            var table = this.tableNameForObjectByClass[@class];
            var name = this.Database.SchemaName + "." + ProcedurePrefixForLoad + @class.Name.ToLowerInvariant();

            // Import Objects
            var definition = $@"
DROP FUNCTION IF EXISTS {name}({SqlTypeForClass},{this.ObjectArrayParam.TypeName});
CREATE FUNCTION {name}(
	{this.ParamNameForClass} {SqlTypeForClass},
	{this.ObjectArrayParam} {this.ObjectArrayParam.TypeName})
    RETURNS void
    LANGUAGE sql
AS $$
    INSERT INTO  {table} ({ColumnNameForClass}, {ColumnNameForObject})
    SELECT p_c, o
    FROM unnest(p_arr_o) AS t(o)
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void CreateObject(IClass @class)
        {
            var table = this.tableNameForObjectByClass[@class];
            var name = this.Database.SchemaName + "." + ProcedurePrefixForCreateObject + @class.Name.ToLowerInvariant();

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

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({SqlTypeForClass});
CREATE FUNCTION {name}({this.ParamNameForClass} {SqlTypeForClass})
    RETURNS {SqlTypeForObject}
    LANGUAGE plpgsql
AS $$
DECLARE {this.ParamNameForObject} {SqlTypeForObject};
BEGIN

    INSERT INTO {this.TableNameForObjects} ({ColumnNameForClass}, {ColumnNameForVersion})
    VALUES ({this.ParamNameForClass}, {(long)Allors.Version.DatabaseInitial})
    RETURNING {ColumnNameForObject} INTO {this.ParamNameForObject};

    INSERT INTO {table} ({ColumnNameForObject},{ColumnNameForClass})
    VALUES ({this.ParamNameForObject},{this.ParamNameForClass});

    RETURN {this.ParamNameForObject};
END
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void CreateObjects(IClass @class)
        {
            var name = this.Database.SchemaName + "." + ProcedurePrefixForCreateObjects + @class.Name.ToLowerInvariant();

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

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({SqlTypeForClass}, {SqlTypeForCount});
CREATE FUNCTION {name}({this.ParamNameForClass} {SqlTypeForClass}, {this.ParamNameForCount} {SqlTypeForCount})
    RETURNS SETOF {SqlTypeForObject}
    LANGUAGE plpgsql
AS $$
DECLARE ID {SqlTypeForObject};
DECLARE COUNTER integer := 0;
BEGIN
    WHILE COUNTER < {this.ParamNameForCount} LOOP

        INSERT INTO {this.TableNameForObjects} ({ColumnNameForClass}, {ColumnNameForVersion})
        VALUES ({this.ParamNameForClass}, {(long)Allors.Version.DatabaseInitial} )
        RETURNING {ColumnNameForObject} INTO ID;

        INSERT INTO {this.tableNameForObjectByClass[@class.ExclusiveDatabaseClass]} ({ColumnNameForObject},{ColumnNameForClass})
        VALUES (ID,{this.ParamNameForClass});

        COUNTER := COUNTER+1;

        RETURN NEXT ID;
    END LOOP;
END
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void DeleteObject(IClass @class)
        {
            var table = this.tableNameForObjectByClass[@class];
            var name = this.Database.SchemaName + "." + ProcedurePrefixForDeleteObject + @class.Name.ToLowerInvariant();

            // One statement deletes the object from the objects table and from the table of its class.
            var sql = $@"WITH deleted AS (
    DELETE FROM {this.TableNameForObjects}
    WHERE {ColumnNameForObject}={this.ParamInvocationNameForObject})
DELETE FROM {table}
WHERE {ColumnNameForObject}={this.ParamInvocationNameForObject}";
            this.commandTextForDeleteObjectByClass.Add(@class, sql);

            var definition = $@"DROP FUNCTION IF EXISTS {name}({SqlTypeForObject});
CREATE FUNCTION {name}({this.ParamNameForObject} {SqlTypeForObject})
    RETURNS void
    LANGUAGE sql
AS $$

    DELETE FROM {this.TableNameForObjects}
    WHERE {ColumnNameForObject}={this.ParamNameForObject};

    DELETE FROM {table}
    WHERE {ColumnNameForObject}={this.ParamNameForObject};
$$;
";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetUnitRoles(IClass @class)
        {
            var sortedUnitRoleTypes = this.Database.GetSortedUnitRolesByObjectType(@class);
            var name = this.Database.SchemaName + "." + ProcedurePrefixForGetUnits + @class.Name.ToLowerInvariant();

            // Get Unit Roles
            string Sql(string @object) => $@"SELECT {string.Join(", ", sortedUnitRoleTypes.Select(v => this.columnNameByRelationType[v.RelationType]))}
    FROM {this.tableNameForObjectByClass[@class.ExclusiveDatabaseClass]}
    WHERE {ColumnNameForObject}={@object}";

            this.commandTextForGetUnitRolesByClass.Add(@class, Sql(this.ParamInvocationNameForObject));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({SqlTypeForObject});
CREATE FUNCTION {name}({this.ParamNameForObject} {SqlTypeForObject})
    RETURNS TABLE
    ({string.Join(", ", sortedUnitRoleTypes.Select(v => $"{this.columnNameByRelationType[v.RelationType]} {this.GetSqlType(v)}"))})
    LANGUAGE sql
AS $$
    {Sql(this.ParamNameForObject)};
$$;";
            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchUnitRoles(IClass @class)
        {
            var sortedUnitRoleTypes = this.Database.GetSortedUnitRolesByObjectType(@class);
            var table = this.tableNameForObjectByClass[@class.ExclusiveDatabaseClass];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForPrefetchUnits + @class.Name.ToLowerInvariant();

            string Sql(string objectArray) => $@"WITH objects AS (SELECT UNNEST({objectArray}) AS {ColumnNameForObject})

    SELECT {ColumnNameForObject}, {string.Join(", ", sortedUnitRoleTypes.Select(v => this.columnNameByRelationType[v.RelationType]))}
    FROM {table}
    WHERE {ColumnNameForObject} IN (SELECT {ColumnNameForObject} FROM objects)";

            this.commandTextForPrefetchUnitRolesByClass.Add(@class, Sql(SubSelect(objects)));

            var definition =
$@"DROP FUNCTION IF EXISTS {name}({objectsType});
CREATE FUNCTION {name}({objects} {objectsType})
    RETURNS TABLE
    (
        {ColumnNameForObject} {SqlTypeForObject},
        {string.Join(", ", sortedUnitRoleTypes.Select(v => $"{this.columnNameByRelationType[v.RelationType]} {this.GetSqlType(v)}"))}
    )
LANGUAGE sql
AS $$
    {Sql(objects.Name)};
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositesRoleObjectTable(IClass @class, IAssociationType associationType)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name = this.Database.SchemaName + "." + ProcedurePrefixForGetRole + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Get Composites Role (1-*) [object table]
            string Sql(string association) => $@"SELECT {ColumnNameForObject}
    FROM {table}
    WHERE {this.columnNameByRelationType[relationType]}={association}";

            this.commandTextForGetRoleByRelationType.Add(relationType, Sql(this.ParamInvocationNameForAssociation));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({SqlTypeForObject});
CREATE FUNCTION {name}({this.ParamNameForAssociation} {SqlTypeForObject})
    RETURNS SETOF {SqlTypeForObject}
    LANGUAGE sql
AS $$
    {Sql(this.ParamNameForAssociation)};
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositesRoleObjectTable(IClass @class, IAssociationType associationType)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForPrefetchRole + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Prefetch Composites Role (1-*) [object table]
            string Sql(string objectArray) => $@"WITH objects AS (SELECT UNNEST({objectArray}) AS {ColumnNameForObject})

    SELECT {this.columnNameByRelationType[relationType]}, {ColumnNameForObject}
    FROM {table}
    WHERE {this.columnNameByRelationType[relationType]} IN (SELECT {ColumnNameForObject} FROM objects)";

            this.commandTextForPrefetchRoleByRelationType.Add(relationType, Sql(SubSelect(objects)));

            var definition =
$@"DROP FUNCTION IF EXISTS {name}({objectsType});
CREATE FUNCTION {name}({objects} {objectsType})
    RETURNS TABLE
    (
         {this.columnNameByRelationType[relationType]} {SqlTypeForObject},
         {ColumnNameForObject} {SqlTypeForObject}
    )
    LANGUAGE sql
AS $$
    {Sql(objects.Name)};
$$;";
            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositeAssociationObjectTable(IClass @class, IAssociationType associationType)
        {
            var relationType = associationType.RelationType;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForGetAssociation + @class.Name.ToLowerInvariant() + "_" + relationType.RoleType.SingularFullName.ToLowerInvariant();
            var table = this.tableNameForObjectByClass[@class];

            // Get Composite Association (1-*) [object table]
            string Sql(string role) => $@"SELECT {this.columnNameByRelationType[relationType]}
    FROM {table}
    WHERE {ColumnNameForObject}={role}";

            this.commandTextForGetAssociationByRelationType.Add(relationType, Sql(this.ParamInvocationNameForCompositeRole));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({SqlTypeForObject});
CREATE FUNCTION {name}({this.ParamNameForCompositeRole} {SqlTypeForObject})
    RETURNS {SqlTypeForObject}
    LANGUAGE plpgsql
AS $$
DECLARE {this.ParamNameForAssociation} {SqlTypeForObject};
BEGIN
    {Sql(this.ParamNameForCompositeRole)}
    INTO {this.ParamNameForAssociation};

    RETURN {this.ParamNameForAssociation};
END
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositeAssociationObjectTable(IClass @class, IAssociationType associationType)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForPrefetchAssociation + @class.Name.ToLowerInvariant() + "_" + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Prefetch Composite Association (1-*) [object table]
            string Sql(string objectArray) => $@"WITH objects AS (SELECT UNNEST({objectArray}) AS {ColumnNameForObject})

    SELECT {this.columnNameByRelationType[relationType]}, {ColumnNameForObject}
    FROM {table}
    WHERE {ColumnNameForObject} IN (SELECT {ColumnNameForObject} FROM objects)";

            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, Sql(SubSelect(objects)));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({objectsType});
CREATE FUNCTION {name}({objects} {objectsType})
    RETURNS TABLE
    (
         {this.columnNameByRelationType[relationType]} {SqlTypeForObject},
         {ColumnNameForObject} {SqlTypeForObject}
    )
    LANGUAGE sql
AS $$
    {Sql(objects.Name)};
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void AddCompositeRoleObjectTable(IClass @class, IAssociationType associationType)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var roles = this.CompositeRoleArrayParam;
            var rolesType = roles.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForAddRole + @class.Name.ToLowerInvariant() + "_" + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Add Composite Role (1-*) [object table]
            string Sql(string objectArray, string roleArray) => $@"WITH relations AS (SELECT UNNEST({objectArray}) AS {ColumnNameForAssociation}, UNNEST({roleArray}) AS {ColumnNameForRole})

    UPDATE {table}
    SET {this.columnNameByRelationType[relationType]} = relations.{ColumnNameForAssociation}
    FROM relations
    WHERE {table}.{ColumnNameForObject} = relations.{ColumnNameForRole}";

            this.commandTextForAddRoleByRelationType.Add(relationType, Sql(SubSelect(objects), SubSelect(roles)));
            this.CommandTextForLoadAddRoleByRelationType.Add(relationType, Sql(objects.InvocationName, roles.InvocationName));

            var definition =
$@"DROP FUNCTION IF EXISTS {name}({objectsType}, {rolesType});
CREATE FUNCTION {name}({objects} {objectsType}, {roles} {rolesType})
    RETURNS void
    LANGUAGE sql
AS $$
    {Sql(objects.Name, roles.Name)}
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void RemoveCompositeRoleObjectTable(IClass @class, IAssociationType associationType)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var roles = this.CompositeRoleArrayParam;
            var rolesType = roles.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForRemoveRole + @class.Name.ToLowerInvariant() + "_" + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Remove Composite Role (1-*) [object table]
            string Sql(string objectArray, string roleArray) => $@"WITH relations AS (SELECT UNNEST({objectArray}) AS {ColumnNameForAssociation}, UNNEST({roleArray}) AS {ColumnNameForRole})

    UPDATE {table}
    SET {this.columnNameByRelationType[relationType]} = null
    FROM relations
    WHERE {table}.{this.columnNameByRelationType[relationType]} = relations.{ColumnNameForAssociation} AND
          {table}.{ColumnNameForObject} = relations.{ColumnNameForRole}";

            this.commandTextForRemoveRoleByRelationType.Add(relationType, Sql(SubSelect(objects), SubSelect(roles)));

            var definition =
$@"DROP FUNCTION IF EXISTS {name}({objectsType}, {rolesType});
CREATE FUNCTION {name}({objects} {objectsType}, {roles} {rolesType})
    RETURNS void
    LANGUAGE sql
AS $$
    {Sql(objects.Name, roles.Name)}
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void ClearCompositeRoleObjectTable(IClass @class, IAssociationType associationType)
        {
            var relationType = associationType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForClearRole + @class.Name.ToLowerInvariant() + "_" + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Clear Composites Role (1-*) [object table]
            string Sql(string objectArray) => $@"WITH objects AS (SELECT UNNEST({objectArray}) AS {ColumnNameForObject})

    UPDATE {table}
    SET {this.columnNameByRelationType[relationType]} = null
    FROM objects
    WHERE {table}.{this.columnNameByRelationType[relationType]} = objects.{ColumnNameForObject}";

            this.commandTextForClearRoleByRelationType.Add(relationType, Sql(SubSelect(objects)));

            var definition =
                $@"DROP FUNCTION IF EXISTS {name}({objectsType});
CREATE FUNCTION {name}({objects} {objectsType})
    RETURNS void
    LANGUAGE sql
AS $$
    {Sql(objects.Name)}
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
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
            var unitTypeTag = ((IUnit)relationType.RoleType.ObjectType).Tag;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForSetRole + @class.Name.ToLowerInvariant() + "_" + roleType.SingularFullName.ToLowerInvariant();

            MappingArrayParameter roles;
            switch (unitTypeTag)
            {
                case UnitTags.String:
                    roles = this.StringMaxRoleArrayParam;
                    break;

                case UnitTags.Integer:
                    roles = this.IntegerRoleArrayParam;
                    break;

                case UnitTags.Float:
                    roles = this.DoubleRoleArrayParam;
                    break;

                case UnitTags.Decimal:
                    roles = this.DecimalRoleArrayParam;
                    break;

                case UnitTags.Boolean:
                    roles = this.BooleanRoleArrayParam;
                    break;

                case UnitTags.DateTime:
                    roles = this.DateTimeRoleArrayParam;
                    break;

                case UnitTags.Unique:
                    roles = this.UniqueRoleArrayParam;
                    break;

                case UnitTags.Binary:
                    roles = this.BinaryRoleArrayParam;
                    break;

                default:
                    throw new ArgumentException("Unknown Unit ObjectType: " + roleType.ObjectType.SingularName);
            }

            var rolesType = roles.TypeName;

            string Sql(string objectArray, string roleArray) => $@"WITH relations AS (SELECT UNNEST({objectArray}) AS {ColumnNameForAssociation}, UNNEST({roleArray}) AS {ColumnNameForRole})

    UPDATE {table}
    SET {this.columnNameByRelationType[relationType]} = relations.{ColumnNameForRole}
    FROM relations
    WHERE {ColumnNameForObject} = relations.{ColumnNameForAssociation}";

            commandTextForSetUnitRoleByRelationType.Add(relationType, Sql(SubSelect(objects), SubSelect(roles)));
            commandTextForLoadSetUnitRoleByRelationType.Add(relationType, Sql(objects.InvocationName, roles.InvocationName));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({objectsType}, {rolesType});
CREATE FUNCTION {name}({objects} {objectsType}, {roles} {rolesType})
    RETURNS void
    LANGUAGE sql
AS $$
    {Sql(objects.Name, roles.Name)}
$$;";
            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositeRoleObjectTable(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name = this.Database.SchemaName + "." + ProcedurePrefixForGetRole + roleType.SingularFullName.ToLowerInvariant();

            // Get Composite Role (1-1 and *-1) [object table]
            string Sql(string association) => $@"SELECT {this.columnNameByRelationType[relationType]}
    FROM {table}
    WHERE {ColumnNameForObject}={association}";

            this.commandTextForGetRoleByRelationType.Add(relationType, Sql(this.ParamInvocationNameForAssociation));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({SqlTypeForObject});
CREATE FUNCTION {name}({this.ParamNameForAssociation} {SqlTypeForObject})
    RETURNS {SqlTypeForObject}
    LANGUAGE plpgsql
AS $$
DECLARE {this.ParamNameForCompositeRole} {SqlTypeForObject};
BEGIN
    {Sql(this.ParamNameForAssociation)}
    INTO {this.ParamNameForCompositeRole};

    RETURN {this.ParamNameForCompositeRole};
END
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositeRoleObjectTable(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForPrefetchRole + roleType.SingularFullName.ToLowerInvariant();

            // Prefetch Composite Role (1-1 and *-1) [object table]
            string Sql(string objectArray) => $@"WITH objects AS (SELECT UNNEST({objectArray}) AS {ColumnNameForObject})

    SELECT {ColumnNameForObject}, {this.columnNameByRelationType[relationType]}
    FROM {table}
    WHERE {ColumnNameForObject} IN (SELECT {ColumnNameForObject} FROM objects)";

            this.commandTextForPrefetchRoleByRelationType.Add(relationType, Sql(SubSelect(objects)));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({objectsType});
CREATE FUNCTION {name}({objects} {objectsType})
    RETURNS TABLE
    (
         {ColumnNameForObject} {SqlTypeForObject},
         {this.columnNameByRelationType[relationType]} {SqlTypeForObject}
    )
    LANGUAGE sql
AS $$
    {Sql(objects.Name)};
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositeAssociationOne2OneObjectTable(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name = this.Database.SchemaName + "." + ProcedurePrefixForGetAssociation + @class.Name.ToLowerInvariant() + "_" + roleType.SingularFullName.ToLowerInvariant();

            // Get Composite Association (1-1) [object table]
            string Sql(string role) => $@"SELECT {ColumnNameForObject}
    FROM {table}
    WHERE {this.columnNameByRelationType[relationType]}={role}";

            this.commandTextForGetAssociationByRelationType.Add(relationType, Sql(this.ParamInvocationNameForCompositeRole));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({SqlTypeForObject});
CREATE FUNCTION {name}({this.ParamNameForCompositeRole} {SqlTypeForObject})
    RETURNS {SqlTypeForObject}
    LANGUAGE plpgsql
AS $$
DECLARE {this.ParamNameForAssociation} {SqlTypeForObject};
BEGIN
    {Sql(this.ParamNameForCompositeRole)}
    INTO {this.ParamNameForAssociation};

    RETURN {this.ParamNameForAssociation};
END
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositeAssociationObjectTable(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForPrefetchAssociation + @class.Name.ToLowerInvariant() + "_" + roleType.SingularFullName.ToLowerInvariant();

            // Prefetch Composite Association (1-1) [object table]
            string Sql(string objectArray) => $@"WITH objects AS (SELECT UNNEST({objectArray}) AS {ColumnNameForObject})

    SELECT {ColumnNameForObject}, {this.columnNameByRelationType[relationType]}
    FROM {table}
    WHERE {this.columnNameByRelationType[relationType]} IN (SELECT {ColumnNameForObject} FROM objects)";

            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, Sql(SubSelect(objects)));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({objectsType});
CREATE FUNCTION {name}({objects} {objectsType})
    RETURNS TABLE
    (
         {ColumnNameForObject} {SqlTypeForObject},
         {ColumnNameForAssociation} {SqlTypeForObject}
    )
    LANGUAGE sql
AS $$
    {Sql(objects.Name)};
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositesAssociationMany2OneObjectTable(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var name = this.Database.SchemaName + "." + ProcedurePrefixForGetAssociation + @class.Name.ToLowerInvariant() + "_" + roleType.SingularFullName.ToLowerInvariant();

            // Get Composite Association (*-1) [object table]
            string Sql(string role) => $@"SELECT {ColumnNameForObject}
    FROM {table}
    WHERE {this.columnNameByRelationType[relationType]}={role}";

            this.commandTextForGetAssociationByRelationType.Add(relationType, Sql(this.ParamInvocationNameForCompositeRole));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({SqlTypeForObject});
CREATE FUNCTION {name}({this.ParamNameForCompositeRole} {SqlTypeForObject})
    RETURNS SETOF {SqlTypeForObject}
    LANGUAGE sql
AS $$
    {Sql(this.ParamNameForCompositeRole)};
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositesAssociationMany2OneObjectTable(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForPrefetchAssociation + @class.Name.ToLowerInvariant() + "_" + roleType.SingularFullName.ToLowerInvariant();

            // Prefetch Composite Association (*-1) [object table]
            string Sql(string objectArray) => $@"WITH objects AS (SELECT UNNEST({objectArray}) AS {ColumnNameForObject})

    SELECT {ColumnNameForObject}, {this.columnNameByRelationType[relationType]}
    FROM {table}
    WHERE {this.columnNameByRelationType[relationType]} IN (SELECT {ColumnNameForObject} FROM objects)";

            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, Sql(SubSelect(objects)));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({objectsType});
CREATE FUNCTION {name}({objects} {objectsType})
    RETURNS TABLE
    (
         {ColumnNameForObject} {SqlTypeForObject},
         {ColumnNameForAssociation} {SqlTypeForObject}
    )
    LANGUAGE sql
AS $$
    {Sql(objects.Name)};
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void SetCompositeRole(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var roles = this.CompositeRoleArrayParam;
            var rolesType = roles.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForSetRole + @class.Name.ToLowerInvariant() + "_" + roleType.SingularFullName.ToLowerInvariant();

            // Set Composite Role (1-1 and *-1) [object table]
            string Sql(string objectArray, string roleArray) => $@"WITH relations AS (SELECT UNNEST({objectArray}) AS {ColumnNameForAssociation}, UNNEST({roleArray}) AS {ColumnNameForRole})

    UPDATE {table}
    SET {this.columnNameByRelationType[relationType]} = relations.{ColumnNameForRole}
    FROM relations
    WHERE {ColumnNameForObject} = relations.{ColumnNameForAssociation}";

            this.commandTextForSetRoleByRelationType.Add(relationType, Sql(SubSelect(objects), SubSelect(roles)));
            this.CommandTextForLoadSetRoleByRelationType.Add(relationType, Sql(objects.InvocationName, roles.InvocationName));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({objectsType}, {rolesType});
CREATE FUNCTION {name}({objects} {objectsType}, {roles} {rolesType})
    RETURNS void
    LANGUAGE sql
AS $$
    {Sql(objects.Name, roles.Name)}
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void ClearCompositeRole(IClass @class, IRoleType roleType)
        {
            var relationType = roleType.RelationType;
            var table = this.tableNameForObjectByClass[@class];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForClearRole + @class.Name.ToLowerInvariant() + "_" + roleType.SingularFullName.ToLowerInvariant();

            // Clear Composite Role (1-1 and *-1) [object table]
            string Sql(string objectArray) => $@"WITH objects AS (SELECT UNNEST({objectArray}) AS {ColumnNameForObject})

    UPDATE {table}
    SET {this.columnNameByRelationType[relationType]} = null
    FROM objects
    WHERE {table}.{ColumnNameForObject} = objects.{ColumnNameForObject}";

            this.commandTextForClearRoleByRelationType.Add(relationType, Sql(SubSelect(objects)));

            var definition =
$@"DROP FUNCTION IF EXISTS {name}({objectsType});
CREATE FUNCTION {name}({objects} {objectsType})
    RETURNS void
    LANGUAGE sql
AS $$
    {Sql(objects.Name)}
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositesRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var name = this.Database.SchemaName + "." + ProcedurePrefixForGetRole + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Get Composites Role (1-* and *-*) [relation table]
            string Sql(string association) => $@"SELECT {ColumnNameForRole}
    FROM {table}
    WHERE {ColumnNameForAssociation}={association}";

            this.commandTextForGetRoleByRelationType.Add(relationType, Sql(this.ParamInvocationNameForAssociation));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({SqlTypeForObject});
CREATE FUNCTION {name}({this.ParamNameForAssociation} {SqlTypeForObject})
    RETURNS SETOF {SqlTypeForObject}
    LANGUAGE sql
AS $$
    {Sql(this.ParamNameForAssociation)};
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositesRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var objectsArray = this.ObjectArrayParam;
            var objectsArrayType = objectsArray.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForPrefetchRole + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Prefetch Composites Role (1-* and *-*) [relation table]
            string Sql(string objectArray) => $@"WITH objects AS (SELECT UNNEST({objectArray}) AS {ColumnNameForObject})

    SELECT {ColumnNameForAssociation}, {ColumnNameForRole}
    FROM {table}
    WHERE {ColumnNameForAssociation} IN (SELECT {ColumnNameForObject} FROM objects)";

            this.commandTextForPrefetchRoleByRelationType.Add(relationType, Sql(SubSelect(objectsArray)));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({objectsArrayType});
CREATE FUNCTION {name}({objectsArray} {objectsArrayType})
    RETURNS TABLE
    (
         {ColumnNameForObject} {SqlTypeForObject},
         {ColumnNameForRole} {SqlTypeForObject}
    )
    LANGUAGE sql
AS $$
    {Sql(objectsArray.Name)};
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void AddCompositeRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var roles = this.CompositeRoleArrayParam;
            var rolesType = roles.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForAddRole + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Add Composite Role (1-* and *-*) [relation table]
            string Sql(string objectArray, string roleArray) => $@"WITH relations AS (SELECT UNNEST({objectArray}) AS {ColumnNameForAssociation}, UNNEST({roleArray}) AS {ColumnNameForRole})

    INSERT INTO {table} ({ColumnNameForAssociation},{ColumnNameForRole})
    SELECT {ColumnNameForAssociation}, {ColumnNameForRole}
    FROM relations";

            this.commandTextForAddRoleByRelationType.Add(relationType, Sql(SubSelect(objects), SubSelect(roles)));
            this.CommandTextForLoadAddRoleByRelationType.Add(relationType, Sql(objects.InvocationName, roles.InvocationName));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({objectsType}, {rolesType});
CREATE FUNCTION {name}({objects} {objectsType}, {roles} {rolesType})
    RETURNS void
    LANGUAGE sql
AS $$
    {Sql(objects.Name, roles.Name)}
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void RemoveCompositeRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var roles = this.CompositeRoleArrayParam;
            var rolesType = roles.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForRemoveRole + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Remove Composite Role (1-* and *-*) [relation table]
            string Sql(string objectArray, string roleArray) => $@"WITH relations AS (SELECT UNNEST({objectArray}) AS {ColumnNameForAssociation}, UNNEST({roleArray}) AS {ColumnNameForRole})

    DELETE FROM {table}
    USING relations
    WHERE {table}.{ColumnNameForAssociation}=relations.{ColumnNameForAssociation} AND {table}.{ColumnNameForRole}=relations.{ColumnNameForRole}";

            this.commandTextForRemoveRoleByRelationType.Add(relationType, Sql(SubSelect(objects), SubSelect(roles)));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({objectsType}, {rolesType});
CREATE FUNCTION {name}({objects} {objectsType}, {roles} {rolesType})
    RETURNS void
    LANGUAGE sql
AS $$
    {Sql(objects.Name, roles.Name)}
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositeRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var name = this.Database.SchemaName + "." + ProcedurePrefixForGetRole + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Get Composite Role (1-1 and *-1) [relation table]
            string Sql(string association) => $@"SELECT {ColumnNameForRole}
    FROM {table}
    WHERE {ColumnNameForAssociation}={association}";

            this.commandTextForGetRoleByRelationType.Add(relationType, Sql(this.ParamInvocationNameForAssociation));

            var definition =
$@"DROP FUNCTION IF EXISTS {name}({SqlTypeForObject});
CREATE FUNCTION {name}({this.ParamNameForAssociation} {SqlTypeForObject})
    RETURNS {SqlTypeForObject}
    LANGUAGE plpgsql
AS $$
DECLARE {this.ParamNameForCompositeRole} {SqlTypeForObject};
BEGIN
    {Sql(this.ParamNameForAssociation)}
    INTO {this.ParamNameForCompositeRole};

    RETURN {this.ParamNameForCompositeRole};
END
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositeRoleRelationType(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForPrefetchRole + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Prefetch Composite Role (1-1 and *-1) [relation table]
            string Sql(string objectArray) => $@"WITH objects AS (SELECT UNNEST({objectArray}) AS {ColumnNameForObject})

    SELECT {ColumnNameForAssociation}, {ColumnNameForRole}
    FROM {table}
    WHERE {ColumnNameForAssociation} IN (SELECT {ColumnNameForObject} FROM objects)";

            this.commandTextForPrefetchRoleByRelationType.Add(relationType, Sql(SubSelect(objects)));

            var definition =
$@"DROP FUNCTION IF EXISTS {name}({objectsType});
CREATE FUNCTION {name}({objects} {objectsType})
    RETURNS TABLE
    (
         {ColumnNameForObject} {SqlTypeForObject},
         {ColumnNameForRole} {SqlTypeForObject}
    )
    LANGUAGE sql
AS $$
    {Sql(objects.Name)};
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void SetCompositeRoleRelationType(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var roles = this.CompositeRoleArrayParam;
            var rolesType = roles.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForSetRole + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Set Composite Role (1-1 and *-1) [relation table]
            string Sql(string objectArray, string roleArray) => $@"WITH relations AS (SELECT UNNEST({objectArray}) AS {ColumnNameForAssociation}, UNNEST({roleArray}) AS {ColumnNameForRole})

    INSERT INTO {table}
    SELECT {ColumnNameForAssociation}, {ColumnNameForRole} from relations

    ON CONFLICT ({ColumnNameForAssociation})
    DO UPDATE
        SET {ColumnNameForRole} = excluded.{ColumnNameForRole}";

            this.commandTextForSetRoleByRelationType.Add(relationType, Sql(SubSelect(objects), SubSelect(roles)));
            this.CommandTextForLoadSetRoleByRelationType.Add(relationType, Sql(objects.InvocationName, roles.InvocationName));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({objectsType}, {rolesType});
CREATE FUNCTION {name}({objects} {objectsType}, {roles} {rolesType})
    RETURNS void
    LANGUAGE sql
AS $$
    {Sql(objects.Name, roles.Name)};
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositeAssociationRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var name = this.Database.SchemaName + "." + ProcedurePrefixForGetAssociation + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Get Composite Association (1-1) [relation table]
            string Sql(string role) => $@"SELECT {ColumnNameForAssociation}
    FROM {table}
    WHERE {ColumnNameForRole}={role}";

            this.commandTextForGetAssociationByRelationType.Add(relationType, Sql(this.ParamInvocationNameForCompositeRole));

            var definition =
$@"DROP FUNCTION IF EXISTS {name}({SqlTypeForObject});
CREATE FUNCTION {name}({this.ParamNameForCompositeRole} {SqlTypeForObject})
    RETURNS {SqlTypeForObject}
    LANGUAGE plpgsql
AS $$
DECLARE {this.ParamNameForAssociation} {SqlTypeForObject};
BEGIN
    {Sql(this.ParamNameForCompositeRole)}
    INTO {this.ParamNameForAssociation};

    RETURN {this.ParamNameForAssociation};
END
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositeAssociationRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForPrefetchAssociation + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Prefetch Composite Association (1-1) [relation table]
            string Sql(string objectArray) => $@"WITH objects AS (SELECT UNNEST({objectArray}) AS {ColumnNameForObject})

    SELECT {ColumnNameForAssociation},{ColumnNameForRole}
    FROM {table}
    WHERE {ColumnNameForRole} IN (SELECT {ColumnNameForObject} FROM objects)";

            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, Sql(SubSelect(objects)));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({objectsType});
CREATE FUNCTION {name}({objects} {objectsType})
    RETURNS TABLE
    (
         {ColumnNameForAssociation} {SqlTypeForObject},
         {ColumnNameForObject} {SqlTypeForObject}
    )
    LANGUAGE sql
AS $$
    {Sql(objects.Name)};
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetCompositesAssociationRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var name = this.Database.SchemaName + "." + ProcedurePrefixForGetAssociation + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Get Composite Association (*-1) [relation table]
            string Sql(string role) => $@"SELECT {ColumnNameForAssociation}
    FROM {table}
    WHERE {ColumnNameForRole}={role}";

            this.commandTextForGetAssociationByRelationType.Add(relationType, Sql(this.ParamInvocationNameForCompositeRole));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({SqlTypeForObject});
CREATE FUNCTION {name}({this.ParamNameForCompositeRole} {SqlTypeForObject})
    RETURNS SETOF {SqlTypeForObject}
    LANGUAGE sql
AS $$
    {Sql(this.ParamNameForCompositeRole)}
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void PrefetchCompositesAssociationRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForPrefetchAssociation + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Prefetch Composite Association (*-1) [relation table]
            string Sql(string objectArray) => $@"WITH objects AS (SELECT UNNEST({objectArray}) AS {ColumnNameForObject})

    SELECT {ColumnNameForAssociation},{ColumnNameForRole}
    FROM {table}
    WHERE {ColumnNameForRole} IN (SELECT {ColumnNameForObject} FROM objects)";

            this.commandTextForPrefetchAssociationByRelationType.Add(relationType, Sql(SubSelect(objects)));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({objectsType});
CREATE FUNCTION {name}({objects} {objectsType})
    RETURNS TABLE
    (
         {ColumnNameForObject} {SqlTypeForObject},
         {ColumnNameForAssociation} {SqlTypeForObject}
    )
    LANGUAGE SQL
AS $$
    {Sql(objects.Name)};
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void ClearCompositeRoleRelationTable(IRelationType relationType)
        {
            var table = this.tableNameForRelationByRelationType[relationType];
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForClearRole + relationType.RoleType.SingularFullName.ToLowerInvariant();

            // Clear Composite Role (1-1 and *-1) [relation table]
            string Sql(string objectArray) => $@"WITH objects AS (SELECT UNNEST({objectArray}) AS {ColumnNameForObject})

    DELETE FROM {table}
    WHERE {ColumnNameForAssociation} IN (SELECT {ColumnNameForObject} FROM objects)";

            this.commandTextForClearRoleByRelationType.Add(relationType, Sql(SubSelect(objects)));

            var definition =
                $@"
DROP FUNCTION IF EXISTS {name}({objectsType});
CREATE FUNCTION {name}({objects} {objectsType})
    RETURNS void
    LANGUAGE sql
AS $$
    {Sql(objects.Name)}
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void UpdateVersionIds()
        {
            var name = this.Database.SchemaName + "." + ProcedurePrefixForUpdateVersion;

            // Update Version Ids
            string Sql(string objectArray) => $@"UPDATE {this.TableNameForObjects}
    SET {ColumnNameForVersion} = {ColumnNameForVersion} + 1
    WHERE {ColumnNameForObject} IN (SELECT {ColumnNameForObject} FROM unnest({objectArray}) as t({ColumnNameForObject}))";

            this.commandTextForUpdateVersion = Sql(SubSelect(this.ObjectArrayParam));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({this.ObjectArrayParam.TypeName});
CREATE FUNCTION {name}({this.ObjectArrayParam} {this.ObjectArrayParam.TypeName})
    RETURNS void
    LANGUAGE sql
AS $$
    {Sql(this.ObjectArrayParam.Name)};
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void GetVersionIds()
        {
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForGetVersion;

            // Get Version Ids
            string Sql(string objectArray) => $@"WITH objects AS (SELECT UNNEST({objectArray}) AS {ColumnNameForObject})

    SELECT {this.TableNameForObjects}.{ColumnNameForObject}, {this.TableNameForObjects}.{ColumnNameForVersion}
    FROM {this.TableNameForObjects}
    WHERE {this.TableNameForObjects}.{ColumnNameForObject} IN (SELECT {ColumnNameForObject} FROM objects)";

            this.commandTextForGetVersion = Sql(SubSelect(objects));

            var definition =
$@"DROP FUNCTION IF EXISTS {name}({objectsType});
CREATE FUNCTION {name}({objects} {objectsType})
    RETURNS TABLE
    (
         {ColumnNameForObject} {SqlTypeForObject},
         {ColumnNameForVersion} {SqlTypeForVersion}
    )
    LANGUAGE sql
AS $$
    {Sql(objects.Name)};
$$;";
            this.ProcedureDefinitionByName.Add(name, definition);
        }

        private void Instantiate()
        {
            var objects = this.ObjectArrayParam;
            var objectsType = objects.TypeName;
            var name = this.Database.SchemaName + "." + ProcedurePrefixForInstantiate;

            // Instantiate
            string Sql(string objectArray) => $@"WITH objects AS (SELECT UNNEST({objectArray}) AS {ColumnNameForObject})

    SELECT {ColumnNameForObject}, {ColumnNameForClass}, {ColumnNameForVersion}
    FROM {this.TableNameForObjects}
    WHERE {this.TableNameForObjects}.{ColumnNameForObject} IN (SELECT {ColumnNameForObject} FROM objects)";

            this.commandTextForInstantiate = Sql(SubSelect(objects));

            var definition = $@"
DROP FUNCTION IF EXISTS {name}({objectsType});
CREATE FUNCTION {name}({objects} {objectsType})
    RETURNS TABLE
    (
         {ColumnNameForObject} {SqlTypeForObject},
         {ColumnNameForClass} {SqlTypeForClass},
         {ColumnNameForVersion} {SqlTypeForVersion}
    )
    LANGUAGE sql
AS $$
    {Sql(objects.Name)};
$$;";

            this.ProcedureDefinitionByName.Add(name, definition);
        }
    }
}

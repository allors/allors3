// <copyright file="Mapping.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;
    using System.Collections.Generic;
    using Meta;

    public abstract class Mapping
    {
        public const string ColumnNameForObject = "o";
        public const string ColumnNameForClass = "c";
        public const string ColumnNameForVersion = "v";
        public const string ColumnNameForAssociation = "a";
        public const string ColumnNameForRole = "r";

        // The names of the schema that belong to a class or to a relation type carry its id, as ModelId writes
        // it, and no name of the model. A name of the model can be longer than a server allows, and two of them
        // can differ only in case or only beyond what a server keeps of a name: the ids differ in every name,
        // and the longest name, that of an index in the table of a class, has 57 characters.
        public const string PrimaryKeyNameForObjects = "pk__o";

        public static string TableNameFor(IClass @class) => "c_" + ModelId.Encode(@class.Id);

        public static string TableNameFor(IRelationType relationType) => "r_" + ModelId.Encode(relationType.Id);

        // A column of the table of a class holds one end of its relation type, and its name says which: a_ for the
        // association, which a one-to-many relation keeps in the table of the class of its role, and r_ for the
        // role, which the other relations with a column keep in the table of the class of their association. A
        // relation type that keeps its id and changes which end such a column holds, as a relation of a class with
        // itself that changes between one-to-many and many-to-one, then lacks its column, and the schema is
        // invalid; under one name for both ends the adapter would read the one end as the other.
        public static string AssociationColumnNameFor(IRelationType relationType) => "a_" + ModelId.Encode(relationType.Id);

        public static string RoleColumnNameFor(IRelationType relationType) => "r_" + ModelId.Encode(relationType.Id);

        public static string PrimaryKeyNameFor(IClass @class) => "pk_" + ModelId.Encode(@class.Id);

        public static string PrimaryKeyNameFor(IRelationType relationType) => "pk_" + ModelId.Encode(relationType.Id);

        /// <summary>
        /// The name of the index on the column of a relation type in the table of a class. The class is the one
        /// whose table holds the column: a unit role that an interface declares has a column, and so an index, in
        /// the table of each of its classes.
        /// </summary>
        public static string IndexNameFor(IClass @class, IRelationType relationType) => "idx_" + ModelId.Encode(@class.Id) + "_" + ModelId.Encode(relationType.Id);

        /// <summary>
        /// The name of the index on the role column of the table of a relation type.
        /// </summary>
        public static string IndexNameFor(IRelationType relationType) => "idx_" + ModelId.Encode(relationType.Id) + "_" + ColumnNameForRole;

        public abstract IDictionary<IRelationType, string> ColumnNameByRelationType { get; }

        public abstract string TableNameForObjects { get; }
        public abstract IDictionary<IClass, string> TableNameForObjectByClass { get; }
        public abstract IDictionary<IRelationType, string> TableNameForRelationByRelationType { get; }

        public abstract string ParamInvocationNameForObject { get; }
        public abstract string ParamInvocationNameForClass { get; }
        public abstract string ParamInvocationFormat { get; }
        public abstract IDictionary<IRoleType, string> ParamInvocationNameByRoleType { get; }

        public abstract IDictionary<IClass, string> CommandTextForDeleteObjectByClass { get; }
        public abstract IDictionary<IClass, string> CommandTextForCreateObjectsByClass { get; }
        public abstract IDictionary<IClass, string> CommandTextForGetUnitRolesByClass { get; }
        public abstract IDictionary<IClass, IDictionary<IRelationType, string>> CommandTextForSetUnitRoleByRelationTypeByClass { get; }
        public abstract IDictionary<IRelationType, string> CommandTextForGetRoleByRelationType { get; }
        public abstract IDictionary<IRelationType, string> CommandTextForSetRoleByRelationType { get; }
        public abstract IDictionary<IRelationType, string> CommandTextForAddRoleByRelationType { get; }
        public abstract IDictionary<IRelationType, string> CommandTextForRemoveRoleByRelationType { get; }
        public abstract IDictionary<IRelationType, string> CommandTextForClearRoleByRelationType { get; }
        public abstract IDictionary<IRelationType, string> CommandTextForGetAssociationByRelationType { get; }
        public abstract IDictionary<IClass, string> CommandTextForCreateObjectByClass { get; }
        public abstract string CommandTextForInstantiate { get; }
        public abstract string CommandTextForGetVersion { get; }
        public abstract string CommandTextForUpdateVersion { get; }
        public abstract IDictionary<IClass, string> CommandTextForPrefetchUnitRolesByClass { get; }
        public abstract IDictionary<IRelationType, string> CommandTextForPrefetchRoleByRelationType { get; }
        public abstract IDictionary<IRelationType, string> CommandTextForPrefetchAssociationByRelationType { get; }

        public abstract string StringCollation { get; }
        public abstract string Ascending { get; }
        public abstract string Descending { get; }

        /// <summary>
        /// What a table of the schema is in the model, for a message: the objects table, the table of a class or
        /// the table of a relation type, and with a column of the table of a class the relation type of that
        /// column. The tables and the columns carry ids, so a message names the class and the relation type next
        /// to them. Null for a table that is not of the model.
        /// </summary>
        public string Describe(string tableName, string columnName = null)
        {
            if (string.Equals(tableName, this.TableNameForObjects, StringComparison.Ordinal))
            {
                return "the objects table";
            }

            foreach (var (@class, name) in this.TableNameForObjectByClass)
            {
                if (string.Equals(tableName, name, StringComparison.Ordinal))
                {
                    var table = "the table of the class " + @class.Name;
                    foreach (var (relationType, column) in this.ColumnNameByRelationType)
                    {
                        if (string.Equals(columnName, column, StringComparison.Ordinal))
                        {
                            // A one-to-many relation stores its association in the class table. The other
                            // relations with a mapped column store their role there.
                            var holdsColumn = relationType.RoleType.IsMany
                                ? @class.ExistAssociationType(relationType.AssociationType)
                                : @class.ExistRoleType(relationType.RoleType);
                            return holdsColumn ? "the relation type " + Name(relationType) + " in " + table : table;
                        }
                    }

                    return table;
                }
            }

            foreach (var (relationType, name) in this.TableNameForRelationByRelationType)
            {
                if (string.Equals(tableName, name, StringComparison.Ordinal))
                {
                    return "the table of the relation type " + Name(relationType);
                }
            }

            return null;
        }

        private static string Name(IRelationType relationType) => relationType.AssociationType.ObjectType.Name + "." + relationType.RoleType.Name;

        /// <summary>
        /// Escapes a SQL <c>LIKE</c> pattern so only <c>%</c> and <c>_</c> are wildcards (ANSI semantics). The
        /// default leaves the pattern unchanged (PostgreSQL); SqlClient overrides it to neutralise T-SQL
        /// character classes.
        /// </summary>
        public virtual string EscapeLikePattern(string like) => like;
    }
}

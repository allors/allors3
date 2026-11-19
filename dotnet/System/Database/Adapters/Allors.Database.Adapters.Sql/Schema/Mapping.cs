// <copyright file="Mapping.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System.Collections.Generic;
    using Meta;

    public abstract class Mapping
    {
        public const string ColumnNameForObject = "o";
        public const string ColumnNameForClass = "c";
        public const string ColumnNameForVersion = "v";
        public const string ColumnNameForAssociation = "a";
        public const string ColumnNameForRole = "r";
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
        /// Escapes a SQL <c>LIKE</c> pattern so only <c>%</c> and <c>_</c> are wildcards (ANSI semantics). The
        /// default leaves the pattern unchanged (PostgreSQL); SqlClient overrides it to neutralise T-SQL
        /// character classes.
        /// </summary>
        public virtual string EscapeLikePattern(string like) => like;
    }
}

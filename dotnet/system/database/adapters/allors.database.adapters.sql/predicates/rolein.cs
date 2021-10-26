// <copyright file="RoleEqualsValue.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System.Collections.Generic;
    using Adapters;
    using Meta;

    internal sealed class RoleIn : Predicate
    {
        private readonly IRoleType roleType;
        private readonly IEnumerable<int> list;

        internal RoleIn(ExtentFiltered extent, IRoleType roleType, IEnumerable<int> list)
        {
            extent.CheckRole(roleType);
            PredicateAssertions.ValidateRoleIn(roleType, list);
            this.roleType = roleType;
            this.list = list;
        }

        internal override bool BuildWhere(ExtentStatement statement, string alias)
        {
            var schema = statement.Mapping;
            statement.Append(" " + alias + "." + schema.ColumnNameByRelationType[this.roleType.RelationType] + " IN (SELECT _r FROM " + statement.AddParameter(this.list) + ")");
            return this.Include;
        }

        internal override void Setup(ExtentStatement statement) => statement.UseRole(this.roleType);
    }
}

// <copyright file="RoleUnitEquals.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Adapters;
    using Meta;

    internal sealed class RoleUnitIn : Predicate
    {
        private readonly IRoleType roleType;
        private readonly IEnumerable<object> list;

        internal RoleUnitIn(ExtentFiltered extent, IRoleType roleType, IEnumerable<object> list)
        {
            extent.CheckForRoleType(roleType);
            PredicateAssertions.ValidateRoleIn(roleType, list);

            this.roleType = roleType;
            this.list = list;
        }

        internal override ThreeValuedLogic Evaluate(Strategy strategy)
        {
            var value = strategy.GetInternalizedUnitRole(this.roleType);

            if (value == null)
            {
                return ThreeValuedLogic.Unknown;
            }

            return this.list.Contains(value)
                       ? ThreeValuedLogic.True
                       : ThreeValuedLogic.False;
        }
    }
}

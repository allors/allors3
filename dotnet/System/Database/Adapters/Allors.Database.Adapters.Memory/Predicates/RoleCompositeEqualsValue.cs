// <copyright file="RoleCompositeEquals.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using Adapters;
    using Meta;

    internal sealed class RoleCompositeEqualsValue : Predicate
    {
        private readonly IRoleType roleType;
        private readonly object equals;

        internal RoleCompositeEqualsValue(ExtentFiltered extent, IRoleType roleType, object equals)
        {
            extent.CheckForRoleType(roleType);
            PredicateAssertions.ValidateRoleEquals(roleType, equals);

            this.roleType = roleType;
            this.equals = equals;
        }

        internal override ThreeValuedLogic Evaluate(Strategy strategy)
        {
            object value = strategy.GetCompositeRole(this.roleType);

            if (value == null)
            {
                return ThreeValuedLogic.False;
            }

            var equalsValue = this.equals;

            if (this.equals is IRoleType)
            {
                var equalsRole = (IRoleType)this.equals;
                equalsValue = strategy.GetCompositeRole(equalsRole);
            }

            if (equalsValue == null)
            {
                return ThreeValuedLogic.False;
            }

            return value.Equals(equalsValue)
                       ? ThreeValuedLogic.True
                       : ThreeValuedLogic.False;
        }

        /// <summary>
        /// Gets the role type and target object ID for index lookup.
        /// Returns (null, null) if this predicate uses a role-to-role comparison.
        /// </summary>
        internal (IRoleType RoleType, long? TargetId) GetIndexKey()
        {
            // Cannot use index for role-to-role comparisons
            if (this.equals is IRoleType)
            {
                return (null, null);
            }

            if (this.equals is IObject obj)
            {
                return (this.roleType, obj.Id);
            }

            return (null, null);
        }
    }
}

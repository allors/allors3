// <copyright file="ObjectExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using Meta;

    public static partial class ObjectExtensions
    {
        public static void CoreOnPostBuild(this Object @this, ObjectOnPostBuild method)
        {
            // TODO: Optimize
            foreach (var roleType in ((Class)@this.Strategy.Class).RequiredRoleTypes)
            {
                if (roleType.ObjectType is IUnit unit && !@this.Strategy.ExistRole(roleType))
                {
                    switch (unit.Tag)
                    {
                        case UnitTags.Boolean:
                            @this.Strategy.SetUnitRole(roleType, false);
                            break;

                        case UnitTags.Decimal:
                            @this.Strategy.SetUnitRole(roleType, 0m);
                            break;

                        case UnitTags.Float:
                            @this.Strategy.SetUnitRole(roleType, 0d);
                            break;

                        case UnitTags.Integer:
                            @this.Strategy.SetUnitRole(roleType, 0);
                            break;

                        case UnitTags.Unique:
                            @this.Strategy.SetUnitRole(roleType, Guid.NewGuid());
                            break;

                        case UnitTags.DateTime:
                            @this.Strategy.SetUnitRole(roleType, @this.Strategy.Transaction.Now());
                            break;
                    }
                }
            }
        }
   }
}

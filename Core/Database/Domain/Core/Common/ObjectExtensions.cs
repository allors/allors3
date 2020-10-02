// <copyright file="ObjectExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using Allors.Meta;
    using Domain;

    public static partial class ObjectExtensions
    {
        public static void CoreOnPreDerive(this Object @this, ObjectOnPreDerive method)
        {
            var (iteration, changeSet, derivedObjects) = method;

            if (iteration.IsMarked(@this))
            {
                iteration.Schedule(@this);
            }
            else
            {
                if (!iteration.Cycle.Derivation.DerivedObjects.Contains(@this))
                {
                    if (changeSet.IsCreated(@this) || changeSet.HasChangedRoles(@this))
                    {
                        iteration.Schedule(@this);
                    }
                }
            }
        }

        public static void CoreOnPostDerive(this Object @this, ObjectOnPostDerive method)
        {
            var derivation = method.Derivation;
            var @class = (Class)@this.Strategy.Class;
            var metaService = @this.DatabaseState().MetaService;

            foreach (var roleType in metaService.GetRequiredRoleTypes(@class))
            {
                derivation.Validation.AssertExists(@this, roleType);
            }

            foreach (var roleType in @metaService.GetUniqueRoleTypes(@class))
            {
                derivation.Validation.AssertIsUnique(@this, roleType);
            }
        }
    }
}



// <copyright file="InventoryItemExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public static partial class InventoryItemExtensions
    {
        public static void AppsOnBuild(this InventoryItem @this, ObjectOnBuild method)
        {
            // TODO: Let Sync set Unit of Measure
            if (!@this.ExistUnitOfMeasure)
            {
                @this.UnitOfMeasure = @this.Part?.UnitOfMeasure;
            }
        }

        public static void AppsOnPreDerive(this InventoryItem @this, ObjectOnPreDerive method)
        {
            //var (iteration, changeSet, derivedObjects) = method;

            //if (iteration.IsMarked(@this) || changeSet.IsCreated(@this) || changeSet.HasChangedRoles(@this))
            //{
            //    iteration.AddDependency(@this.Part, @this);
            //    iteration.Mark(@this.Part);
            //}
        }

        public static void AppsOnDerive(this InventoryItem @this, ObjectOnDerive method)
        {
            //var session = @this.Strategy.Session;
            //var now = session.Now();

            //(@this).PartDisplayName = @this.Part?.DisplayName;

            //if (!@this.ExistFacility && @this.ExistPart && @this.Part.ExistDefaultFacility)
            //{
            //    @this.Facility = @this.Part.DefaultFacility;
            //}

            //// TODO: Let Sync set Unit of Measure
            //if (!@this.ExistUnitOfMeasure)
            //{
            //    @this.UnitOfMeasure = @this.Part?.UnitOfMeasure;
            //}
        }

        public static void AppsOnPostDerive(this InventoryItem @this, ObjectOnPostDerive method)
        {
            //var derivation = method.Derivation;
            //var part = @this.Part;

            //var builder = new StringBuilder();

            //builder.Append(part.SearchString);

            //@this.SearchString = builder.ToString();
        }
    }
}

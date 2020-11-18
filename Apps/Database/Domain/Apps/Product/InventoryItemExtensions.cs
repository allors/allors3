

// <copyright file="InventoryItemExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
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
    }
}



// <copyright file="InventoryItemExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static partial class InventoryItemExtensions
    {
        public static void Sync(this InventoryItem @this, Part part)
        {
            @this.UnitOfMeasure = part.UnitOfMeasure;
        }
    }
}

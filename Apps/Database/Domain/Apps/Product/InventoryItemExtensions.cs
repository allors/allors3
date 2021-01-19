

// <copyright file="InventoryItemExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static partial class InventoryItemExtensions
    {
        public static void Sync(this InventoryItem @this, Part part)
        {
            @this.SearchString = part.SearchString;
            @this.UnitOfMeasure = part.UnitOfMeasure;
            @this.PartDisplayName = part.DisplayName;
        }
    }
}

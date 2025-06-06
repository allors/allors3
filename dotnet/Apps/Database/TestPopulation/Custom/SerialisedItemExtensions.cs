// <copyright file="SerialisedItemExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

namespace Allors.Database.Domain.TestPopulation
{
    using Domain;

    public static partial class SerialisedItemExtensions
    {
        public static string DisplayName(this SerialisedItem @this) => @this.ItemNumber + " " + @this.PartWhereSerialisedItem?.Name + " SN: " + @this.SerialNumber;
    }
}

// <copyright file="NonSerialisedInventoryItemStates.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class NonSerialisedInventoryItemStates
    {
        private static readonly Guid GoodId = new Guid("6806CC54-3AA7-4510-A209-99F92D5C1D58");
        private static readonly Guid SlightlyDamagedId = new Guid("EE4C6034-7320-4209-A0AA-6067B20AC418");
        private static readonly Guid DefectiveId = new Guid("C0E10011-1BA4-412f-B426-103C1C11B879");
        private static readonly Guid ScrapId = new Guid("CF51C221-111C-4666-8E97-CC060643C5FD");

        private UniquelyIdentifiableCache<NonSerialisedInventoryItemState> cache;

        public NonSerialisedInventoryItemState Good => this.Cache[GoodId];

        public NonSerialisedInventoryItemState SlightlyDamaged => this.Cache[SlightlyDamagedId];

        public NonSerialisedInventoryItemState Defective => this.Cache[DefectiveId];

        public NonSerialisedInventoryItemState Scrap => this.Cache[ScrapId];

        private UniquelyIdentifiableCache<NonSerialisedInventoryItemState> Cache => this.cache ??= new UniquelyIdentifiableCache<NonSerialisedInventoryItemState>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(GoodId, v =>
            {
                v.Name = "Good";
                v.AvailableForSale = true;
            });

            merge(SlightlyDamagedId, v =>
            {
                v.Name = "Slightly Damaged";
                v.AvailableForSale = true;
            });

            merge(DefectiveId, v =>
            {
                v.Name = "Defective";
                v.AvailableForSale = false;
            });

            merge(ScrapId, v =>
            {
                v.Name = "Scrap";
                v.AvailableForSale = false;
            });
        }
    }
}

// <copyright file="FacilityTypes.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class FacilityTypes
    {
        public static readonly Guid WarehouseId = new Guid("d4a70252-58d0-425b-8f54-7f55ae01a7b3");
        public static readonly Guid StorageLocationId = new Guid("921f33b2-5978-409f-b09e-f28708fe770b");

        private UniquelyIdentifiableCache<FacilityType> cache;

        public FacilityType Warehouse => this.Cache[WarehouseId];

        public FacilityType StorageLocation => this.Cache[StorageLocationId];

        private UniquelyIdentifiableCache<FacilityType> Cache => this.cache ??= new UniquelyIdentifiableCache<FacilityType>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).LocaleByName["nl"];

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(WarehouseId, v =>
            {
                v.Name = "Warehouse";
                localisedName.Set(v, dutchLocale, "Magazijn");
                v.IsActive = true;
            });

            merge(StorageLocationId, v =>
            {
                v.Name = "Storage location";
                localisedName.Set(v, dutchLocale, "Opslag plaats");
                v.IsActive = true;
            });
        }
    }
}

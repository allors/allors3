// <copyright file="VatTariffs.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class VatTariffs
    {
        private static readonly Guid StandardId = new Guid("2942BF39-0B73-4015-8815-E6279ED0381F");
        private static readonly Guid ReducedRateId = new Guid("7F5A5FE2-BB18-4644-B1CA-81B391AB82D6");
        private static readonly Guid ZeroRateId = new Guid("70C228CA-AE50-4DA2-B209-D115BEE7F4FF");

        private UniquelyIdentifiableCache<VatTariff> cache;

        public VatTariff Standard => this.Cache[StandardId];

        public VatTariff ReducedRate => this.Cache[ReducedRateId];

        public VatTariff ZeroRate => this.Cache[ZeroRateId];

        private UniquelyIdentifiableCache<VatTariff> Cache => this.cache ??= new UniquelyIdentifiableCache<VatTariff>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(StandardId, v =>
            {
                v.Name = "Standard";
                localisedName.Set(v, dutchLocale, "Hoog");
                v.IsActive = true;
            });

            merge(ReducedRateId, v =>
            {
                v.Name = "Reduced rate";
                localisedName.Set(v, dutchLocale, "Laag");
                v.IsActive = true;
            });

            merge(ZeroRateId, v =>
            {
                v.Name = "Zero rate";
                localisedName.Set(v, dutchLocale, "Nul tarief");
                v.IsActive = true;
            });
        }
    }
}

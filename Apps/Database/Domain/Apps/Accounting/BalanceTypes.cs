// <copyright file="BalanceTypes.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class BalanceTypes
    {
        private static readonly Guid BalanceId = new Guid("b2a37f6e-eb61-4560-bff3-0f1d64168af4");
        private static readonly Guid ProfitLossId = new Guid("d77573fd-3626-439b-bd27-972d152e1502");

        private UniquelyIdentifiableCache<BalanceType> cache;

        public BalanceType Balance => this.Cache[BalanceId];

        public BalanceType ProfitLoss => this.Cache[ProfitLossId];

        private UniquelyIdentifiableCache<BalanceType> Cache => this.cache ??= new UniquelyIdentifiableCache<BalanceType>(this.Transaction);

        protected override void BaseSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(BalanceId, v =>
            {
                v.Name = "Balance";
                localisedName.Set(v, dutchLocale, "Balans");
                v.IsActive = true;
            });

            merge(ProfitLossId, v =>
            {
                v.Name = "Profit and Loss";
                localisedName.Set(v, dutchLocale, "Verlies en Winst");
                v.IsActive = true;
            });
        }
    }
}

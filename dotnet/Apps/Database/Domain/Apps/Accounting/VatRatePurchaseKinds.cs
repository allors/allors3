// <copyright file="VatRatePurchaseKinds.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class VatRatePurchaseKinds
    {
        private static readonly Guid GoodsId = new Guid("6C38B839-DD14-4C79-BD18-6293EB5E87A3");
        private static readonly Guid ServicesId = new Guid("6B7250E4-F1DA-4BA2-ACB6-CAB351839E9F");
        private static readonly Guid InvestmentsId = new Guid("F317A718-6197-47C6-B231-138EF9C26814");

        private UniquelyIdentifiableCache<VatRatePurchaseKind> cache;

        public VatRatePurchaseKind Goods => this.Cache[GoodsId];

        public VatRatePurchaseKind Services => this.Cache[ServicesId];

        public VatRatePurchaseKind Investments => this.Cache[InvestmentsId];

        private UniquelyIdentifiableCache<VatRatePurchaseKind> Cache => this.cache ??= new UniquelyIdentifiableCache<VatRatePurchaseKind>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).LocaleByName["nl"];

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(GoodsId, v =>
            {
                v.Name = "Goods";
                localisedName.Set(v, dutchLocale, "Goederen");
                v.IsActive = true;
            });

            merge(ServicesId, v =>
            {
                v.Name = "Services";
                localisedName.Set(v, dutchLocale, "Diensten");
                v.IsActive = true;
            });

            merge(InvestmentsId, v =>
            {
                v.Name = "Investments";
                localisedName.Set(v, dutchLocale, "Investeringen");
                v.IsActive = true;
            });
        }
    }
}

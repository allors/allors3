// <copyright file="VatRateUsages.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class VatRateUsages
    {
        private static readonly Guid PurchaseId = new Guid("50C1F5FE-C569-4DA7-8451-F06F1C2ADDDE");
        private static readonly Guid SalesId = new Guid("719E799C-A890-459E-AB5C-407530EEC964");
        private static readonly Guid PurchaseAndSalesId = new Guid("1A28BE06-56E0-4ABA-81E9-04232DF4C909");

        private UniquelyIdentifiableCache<VatRateUsage> cache;

        public VatRateUsage Purchase => this.Cache[PurchaseId];

        public VatRateUsage Sales => this.Cache[SalesId];

        public VatRateUsage PurchaseAndSales => this.Cache[PurchaseAndSalesId];

        private UniquelyIdentifiableCache<VatRateUsage> Cache => this.cache ??= new UniquelyIdentifiableCache<VatRateUsage>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).LocaleByName["nl"];

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(PurchaseId, v =>
            {
                v.Name = "Purchase";
                localisedName.Set(v, dutchLocale, "Inkoop");
                v.IsActive = true;
            });

            merge(SalesId, v =>
            {
                v.Name = "Sales";
                localisedName.Set(v, dutchLocale, "Verkoop");
                v.IsActive = true;
            });

            merge(PurchaseAndSalesId, v =>
            {
                v.Name = "Purchase & sales";
                localisedName.Set(v, dutchLocale, "Inkoop & verkoop");
                v.IsActive = true;
            });
        }
    }
}

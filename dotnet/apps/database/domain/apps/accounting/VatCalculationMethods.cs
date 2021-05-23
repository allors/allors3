// <copyright file="VatSystems.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class VatSystems
    {
        private static readonly Guid CashId = new Guid("0EBDC40A-B744-4518-8DEB-F060DEDB0FE6");
        private static readonly Guid InvoiceId = new Guid("180E1FEA-929E-46F4-9F5E-16E1DF60065F");

        private UniquelyIdentifiableCache<VatSystem> cache;

        public VatSystem Cash => this.Cache[CashId];

        public VatSystem Invoice => this.Cache[InvoiceId];

        private UniquelyIdentifiableCache<VatSystem> Cache => this.cache ??= new UniquelyIdentifiableCache<VatSystem>(this.Transaction);

        protected override void BaseSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            // Account for receipts and expenses in period where money is actually transferred.
            // Primarily used when goods/services (> 90%) are delivered to private customers   
            merge(CashId, v =>
            {
                v.Name = "Cash management scheme";
                localisedName.Set(v, dutchLocale, "Kasstelsel");
                v.IsActive = false;
            });

            // Account for receipts and expenses in period based on invoice date.
            merge(InvoiceId, v =>
            {
                v.Name = "Invoice management scheme";
                localisedName.Set(v, dutchLocale, "Factuurstelsel");
                v.IsActive = true;
            });
        }
    }
}

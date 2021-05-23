// <copyright file="SalesInvoiceTypes.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class SalesInvoiceTypes
    {
        public static readonly Guid SalesInvoiceId = new Guid("92411BF1-835E-41f8-80AF-6611EFCE5B32");
        public static readonly Guid CreditNoteId = new Guid("EF5B7C52-E782-416D-B46F-89C8C7A5C24D");

        private UniquelyIdentifiableCache<SalesInvoiceType> cache;

        public SalesInvoiceType SalesInvoice => this.Cache[SalesInvoiceId];

        public SalesInvoiceType CreditNote => this.Cache[CreditNoteId];

        private UniquelyIdentifiableCache<SalesInvoiceType> Cache => this.cache ??= new UniquelyIdentifiableCache<SalesInvoiceType>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(SalesInvoiceId, v =>
            {
                v.Name = "Sales invoice";
                localisedName.Set(v, dutchLocale, "Verkoop factuur");
                v.IsActive = true;
            });

            merge(CreditNoteId, v =>
            {
                v.Name = "Credit Note";
                localisedName.Set(v, dutchLocale, "Credit Nota");
                v.IsActive = true;
            });
        }
    }
}

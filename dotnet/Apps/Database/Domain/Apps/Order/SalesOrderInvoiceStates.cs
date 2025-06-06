// <copyright file="SalesOrderInvoiceStates.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class SalesOrderInvoiceStates
    {
        internal static readonly Guid NotInvoicedId = new Guid("4F5B8C2D-13A1-4E94-A122-89582BBF0B80");
        internal static readonly Guid InvoicedId = new Guid("F3245344-F201-433E-B2B4-83218E4AB84F");
        internal static readonly Guid PartiallyInvoicedId = new Guid("1C927890-5BA1-413C-9682-05DCA4C992DD");

        private UniquelyIdentifiableCache<SalesOrderInvoiceState> cache;

        public SalesOrderInvoiceState NotInvoiced => this.Cache[NotInvoicedId];

        public SalesOrderInvoiceState PartiallyInvoiced => this.Cache[PartiallyInvoicedId];

        public SalesOrderInvoiceState Invoiced => this.Cache[InvoicedId];

        private UniquelyIdentifiableCache<SalesOrderInvoiceState> Cache => this.cache ??= new UniquelyIdentifiableCache<SalesOrderInvoiceState>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(NotInvoicedId, v => v.Name = "Not Invoiced");
            merge(PartiallyInvoicedId, v => v.Name = "Partially Invoiced");
            merge(InvoicedId, v => v.Name = "Invoiced");
        }
    }
}

// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class SerialisedItemPurchaseInvoiceDerivation : DomainDerivation
    {
        public SerialisedItemPurchaseInvoiceDerivation(M m) : base(m, new Guid("510975a7-e210-4d30-8fde-b401cbbb3694")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PurchaseInvoice.ValidInvoiceItems) { Steps = new IPropertyType[] { m.PurchaseInvoice.PurchaseInvoiceItems, m.PurchaseInvoiceItem.SerialisedItem } },
                new ChangedPattern(m.PurchaseInvoice.PurchaseInvoiceState) { Steps = new IPropertyType[] { m.PurchaseInvoice.PurchaseInvoiceItems, m.PurchaseInvoiceItem.SerialisedItem } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.PurchaseInvoice = @this.PurchaseInvoiceItemsWhereSerialisedItem
                    .LastOrDefault(v => v.ExistInvoiceWhereValidInvoiceItem
                                        && v.ExistInvoiceItemType
                                        && (v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Transaction()).PartItem)
                                            || v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Transaction()).ProductItem))
                                        && (((PurchaseInvoice)v.InvoiceWhereValidInvoiceItem).PurchaseInvoiceState.Equals(new PurchaseInvoiceStates(@this.Transaction()).NotPaid)
                                            || ((PurchaseInvoice)v.InvoiceWhereValidInvoiceItem).PurchaseInvoiceState.Equals(new PurchaseInvoiceStates(@this.Transaction()).PartiallyPaid)
                                            || ((PurchaseInvoice)v.InvoiceWhereValidInvoiceItem).PurchaseInvoiceState.Equals(new PurchaseInvoiceStates(@this.Transaction()).Paid)))?
                    .PurchaseInvoiceWherePurchaseInvoiceItem;
            }
        }
    }
}
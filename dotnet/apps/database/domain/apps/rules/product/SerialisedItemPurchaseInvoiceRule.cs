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

    public class SerialisedItemPurchaseInvoiceRule : Rule
    {
        public SerialisedItemPurchaseInvoiceRule(MetaPopulation m) : base(m, new Guid("510975a7-e210-4d30-8fde-b401cbbb3694")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoice.RolePattern(v => v.ValidInvoiceItems, v => v.PurchaseInvoiceItems.PurchaseInvoiceItem.SerialisedItem),
                m.PurchaseInvoice.RolePattern(v => v.PurchaseInvoiceState, v => v.PurchaseInvoiceItems.PurchaseInvoiceItem.SerialisedItem),
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

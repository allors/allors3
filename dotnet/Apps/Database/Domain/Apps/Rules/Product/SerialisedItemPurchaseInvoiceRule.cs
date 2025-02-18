// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class SerialisedItemPurchaseInvoiceRule : Rule
    {
        public SerialisedItemPurchaseInvoiceRule(MetaPopulation m) : base(m, new Guid("510975a7-e210-4d30-8fde-b401cbbb3694")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoice.RolePattern(v => v.ValidInvoiceItems, v => v.PurchaseInvoiceItems.ObjectType.SerialisedItem),
                m.PurchaseInvoice.RolePattern(v => v.PurchaseInvoiceState, v => v.PurchaseInvoiceItems.ObjectType.SerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.PurchaseInvoice = @this.PurchaseInvoiceItemsWhereSerialisedItem
                    .Where(v => v.ExistInvoiceWhereValidInvoiceItem
                                        && v.ExistInvoiceItemType
                                        && (v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Transaction()).PartItem)
                                            || v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Transaction()).ProductItem))
                                        && (((PurchaseInvoice)v.InvoiceWhereValidInvoiceItem).PurchaseInvoiceState.Equals(new PurchaseInvoiceStates(@this.Transaction()).NotPaid)
                                            || ((PurchaseInvoice)v.InvoiceWhereValidInvoiceItem).PurchaseInvoiceState.Equals(new PurchaseInvoiceStates(@this.Transaction()).PartiallyPaid)
                                            || ((PurchaseInvoice)v.InvoiceWhereValidInvoiceItem).PurchaseInvoiceState.Equals(new PurchaseInvoiceStates(@this.Transaction()).Paid)))?
                    .OrderBy(v => v.PurchaseInvoiceWherePurchaseInvoiceItem.InvoiceDate)
                    .LastOrDefault()?
                    .PurchaseInvoiceWherePurchaseInvoiceItem;
            }
        }
    }
}

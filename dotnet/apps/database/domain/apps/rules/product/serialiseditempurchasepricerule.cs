// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

    public class SerialisedItemPurchasePriceRule : Rule
    {
        public SerialisedItemPurchasePriceRule(MetaPopulation m) : base(m, new Guid("d9748a88-862d-4793-8fa2-0e052c6c13c9")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.PurchaseInvoice),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.PurchasePrice = @this.AssignedPurchasePrice ?? @this.PurchaseInvoiceItemsWhereSerialisedItem
                    .Where(v => v.ExistInvoiceWhereValidInvoiceItem
                                        && (v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Transaction()).PartItem)
                                            || v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Transaction()).ProductItem)))?
                    .OrderBy(v => v.PurchaseInvoiceWherePurchaseInvoiceItem.InvoiceDate)
                    .LastOrDefault()?
                    .UnitPrice ?? 0M;
            }
        }
    }
}

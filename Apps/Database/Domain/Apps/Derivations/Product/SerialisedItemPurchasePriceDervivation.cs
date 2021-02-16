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

    public class SerialisedItemPurchasePriceDervivation : DomainDerivation
    {
        public SerialisedItemPurchasePriceDervivation(M m) : base(m, new Guid("d9748a88-862d-4793-8fa2-0e052c6c13c9")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.SerialisedItem.PurchaseInvoice),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                if (@this.ExistPurchaseInvoice)
                {
                    @this.PurchasePrice = @this.PurchaseInvoiceItemsWhereSerialisedItem
                        .LastOrDefault(v => v.ExistInvoiceWhereValidInvoiceItem
                                            && (v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Transaction()).PartItem)
                                                || v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Transaction()).ProductItem)))?
                        .UnitPrice ?? 0M;

                    @this.RemoveAssignedPurchasePrice();
                }
            }
        }
    }
}

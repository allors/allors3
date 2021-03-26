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

    public class SerialisedItemPurchaseOrderDerivation : DomainDerivation
    {
        public SerialisedItemPurchaseOrderDerivation(M m) : base(m, new Guid("401c31c7-09cf-4091-aee1-eccc460b8578")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseOrder.ValidOrderItems) { Steps = new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems, m.PurchaseOrderItem.SerialisedItem} },
                new RolePattern(m.PurchaseOrder.PurchaseOrderState) { Steps = new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems, m.PurchaseOrderItem.SerialisedItem} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.PurchaseOrder = @this.PurchaseOrderItemsWhereSerialisedItem
                    .LastOrDefault(v => v.ExistOrderWhereValidOrderItem
                                        && v.ExistInvoiceItemType
                                        && (v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Transaction()).PartItem)
                                            || v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Transaction()).ProductItem))
                                        && (((PurchaseOrder)v.OrderWhereValidOrderItem).PurchaseOrderState.Equals(new PurchaseOrderStates(@this.Transaction()).Sent)
                                            || ((PurchaseOrder)v.OrderWhereValidOrderItem).PurchaseOrderState.Equals(new PurchaseOrderStates(@this.Transaction()).Completed)
                                            || ((PurchaseOrder)v.OrderWhereValidOrderItem).PurchaseOrderState.Equals(new PurchaseOrderStates(@this.Transaction()).Finished)))?
                    .PurchaseOrderWherePurchaseOrderItem;
            }
        }
    }
}

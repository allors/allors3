// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;
    using Derivations;
    using Resources;

    public class SerialisedItemPurchasedOrderDervivation : DomainDerivation
    {
        public SerialisedItemPurchasedOrderDervivation(M m) : base(m, new Guid("401c31c7-09cf-4091-aee1-eccc460b8578")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PurchaseOrderItem.SerialisedItem) { Steps = new IPropertyType[] {m.PurchaseOrderItem.SerialisedItem} },
                new ChangedPattern(m.PurchaseOrder.ValidOrderItems) { Steps = new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems, m.PurchaseOrderItem.SerialisedItem} },
                new ChangedPattern(m.PurchaseOrder.PurchaseOrderState) { Steps = new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems, m.PurchaseOrderItem.SerialisedItem} },

            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            //changeSet.AssociationsByRoleType.TryGetValue(M.BasePrice, out var changedEmployer);
            //var employmentWhereEmployer = changedEmployer?.Select(session.Instantiate).OfType<BasePrice>();

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.PurchaseOrder = @this.PurchaseOrderItemsWhereSerialisedItem
                    .LastOrDefault(v => v.ExistOrderWhereValidOrderItem
                                        && (v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Session()).PartItem)
                                            || v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Session()).ProductItem))
                                        && (((PurchaseOrder)v.OrderWhereValidOrderItem).PurchaseOrderState.Equals(new PurchaseOrderStates(@this.Session()).Sent)
                                            || ((PurchaseOrder)v.OrderWhereValidOrderItem).PurchaseOrderState.Equals(new PurchaseOrderStates(@this.Session()).Completed)
                                            || ((PurchaseOrder)v.OrderWhereValidOrderItem).PurchaseOrderState.Equals(new PurchaseOrderStates(@this.Session()).Finished)))?
                    .PurchaseOrderWherePurchaseOrderItem;
            }
        }
    }
}

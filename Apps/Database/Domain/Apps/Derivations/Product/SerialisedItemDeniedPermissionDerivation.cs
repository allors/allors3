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

    public class SerialisedItemDeniedPermissionDerivation : DomainDerivation
    {
        public SerialisedItemDeniedPermissionDerivation(M m) : base(m, new Guid("14466d15-63cb-4ef2-9725-f93a361ec24c")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(m.InventoryItemTransaction.SerialisedItem) { Steps =  new IPropertyType[] { m.InventoryItemTransaction.SerialisedItem } },
            new ChangedPattern(m.PurchaseInvoiceItem.SerialisedItem) { Steps =  new IPropertyType[] { m.PurchaseInvoiceItem.SerialisedItem } },
            new ChangedPattern(m.PurchaseOrderItem.SerialisedItem) { Steps =  new IPropertyType[] { m.PurchaseOrderItem.SerialisedItem } },
            new ChangedPattern(m.QuoteItem.SerialisedItem) { Steps =  new IPropertyType[] { m.QuoteItem.SerialisedItem } },
            new ChangedPattern(m.SalesInvoiceItem.SerialisedItem) { Steps =  new IPropertyType[] { m.SalesInvoiceItem.SerialisedItem } },
            new ChangedPattern(m.SalesOrderItem.SerialisedItem) { Steps =  new IPropertyType[] { m.SalesOrderItem.SerialisedItem } },
            new ChangedPattern(m.SerialisedInventoryItem.SerialisedItem) { Steps =  new IPropertyType[] { m.SerialisedInventoryItem.SerialisedItem } },
            new ChangedPattern(m.ShipmentItem.SerialisedItem) { Steps =  new IPropertyType[] { m.ShipmentItem.SerialisedItem } },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                var deletePermission = new Permissions(@this.Strategy.Session).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (!@this.ExistInventoryItemTransactionsWhereSerialisedItem
                    && !@this.ExistPurchaseInvoiceItemsWhereSerialisedItem
                    && !@this.ExistPurchaseOrderItemsWhereSerialisedItem
                    && !@this.ExistQuoteItemsWhereSerialisedItem
                    && !@this.ExistSalesInvoiceItemsWhereSerialisedItem
                    && !@this.ExistSalesOrderItemsWhereSerialisedItem
                    && !@this.ExistSerialisedInventoryItemsWhereSerialisedItem
                    && !@this.ExistShipmentItemsWhereSerialisedItem)
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }
            }
        }
    }
}

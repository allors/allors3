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

    public class SerialisedItemDeniedPermissionRule : Rule
    {
        public SerialisedItemDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("14466d15-63cb-4ef2-9725-f93a361ec24c")) =>
            this.Patterns = new Pattern[]
        {
            m.SerialisedItem.AssociationPattern(v => v.InventoryItemTransactionsWhereSerialisedItem),
            m.SerialisedItem.AssociationPattern(v => v.PurchaseInvoiceItemsWhereSerialisedItem),
            m.SerialisedItem.AssociationPattern(v => v.PurchaseOrderItemsWhereSerialisedItem),
            m.SerialisedItem.AssociationPattern(v => v.QuoteItemsWhereSerialisedItem),
            m.SerialisedItem.AssociationPattern(v => v.SalesInvoiceItemsWhereSerialisedItem),
            m.SerialisedItem.AssociationPattern(v => v.SalesOrderItemsWhereSerialisedItem),
            m.SerialisedItem.AssociationPattern(v => v.SerialisedInventoryItemsWhereSerialisedItem),
            m.SerialisedItem.AssociationPattern(v => v.ShipmentItemsWhereSerialisedItem),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Delete);

                if (@this.IsDeletable)
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

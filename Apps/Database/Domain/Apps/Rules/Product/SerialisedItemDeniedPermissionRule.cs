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
        public SerialisedItemDeniedPermissionRule(M m) : base(m, new Guid("14466d15-63cb-4ef2-9725-f93a361ec24c")) =>
            this.Patterns = new Pattern[]
        {
            new AssociationPattern(m.InventoryItemTransaction.SerialisedItem),
            new AssociationPattern(m.PurchaseInvoiceItem.SerialisedItem),
            new AssociationPattern(m.PurchaseOrderItem.SerialisedItem),
            new AssociationPattern(m.QuoteItem.SerialisedItem),
            new AssociationPattern(m.SalesInvoiceItem.SerialisedItem),
            new AssociationPattern(m.SalesOrderItem.SerialisedItem),
            new AssociationPattern(m.SerialisedInventoryItem.SerialisedItem),
            new AssociationPattern(m.ShipmentItem.SerialisedItem),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta.ObjectType, @this.Meta.Delete);

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

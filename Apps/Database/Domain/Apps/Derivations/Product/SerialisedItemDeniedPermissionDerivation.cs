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
            new RolePattern(m.InventoryItemTransaction.SerialisedItem),
            new RolePattern(m.PurchaseInvoiceItem.SerialisedItem),
            new RolePattern(m.PurchaseOrderItem.SerialisedItem),
            new RolePattern(m.QuoteItem.SerialisedItem),
            new RolePattern(m.SalesInvoiceItem.SerialisedItem),
            new RolePattern(m.SalesOrderItem.SerialisedItem),
            new RolePattern(m.SerialisedInventoryItem.SerialisedItem),
            new RolePattern(m.ShipmentItem.SerialisedItem),
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

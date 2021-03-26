// <copyright file="PurchaseOrderItemByProductDerivation.cs" company="Allors bvba">
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

    public class PurchaseOrderItemByProductDerivation : DomainDerivation
    {
        public PurchaseOrderItemByProductDerivation(M m) : base(m, new Guid("dbd7f09a-aa32-44a6-a671-e7269d47ae81")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseOrderItemVersion, m.PurchaseOrderItemVersion.Part) { Steps = new IPropertyType[] {m.PurchaseOrderItemVersion.PurchaseOrderItemWhereCurrentVersion, m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder.PurchaseOrderItemsByProduct } },
                new RolePattern(m.PurchaseOrderItem, m.PurchaseOrderItem.Part) { Steps = new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder.PurchaseOrderItemsByProduct } },
                new RolePattern(m.PurchaseOrderItem, m.PurchaseOrderItem.QuantityOrdered) { Steps = new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder.PurchaseOrderItemsByProduct } },
                new RolePattern(m.PurchaseOrderItem, m.PurchaseOrderItem.TotalBasePrice) { Steps = new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder.PurchaseOrderItemsByProduct } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<PurchaseOrderItemByProduct>())
            {
                var sameProductItems = @this.PurchaseOrderWherePurchaseOrderItemsByProduct?.PurchaseOrderItems
                    .Where(v => v.IsValid && v.ExistPart && v.Part.Equals(@this.UnifiedProduct))
                    .ToArray();

                @this.QuantityOrdered = sameProductItems.Sum(w => w.QuantityOrdered);
                @this.ValueOrdered = sameProductItems.Sum(w => w.TotalBasePrice);
            }
        }
    }
}

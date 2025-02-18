// <copyright file="PurchaseOrderItemByProductDerivation.cs" company="Allors bv">
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

    public class PurchaseOrderItemByProductRule : Rule
    {
        public PurchaseOrderItemByProductRule(MetaPopulation m) : base(m, new Guid("dbd7f09a-aa32-44a6-a671-e7269d47ae81")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrderItemVersion.RolePattern(v => v.Part, v => v.PurchaseOrderItemWhereCurrentVersion.ObjectType.PurchaseOrderWherePurchaseOrderItem.ObjectType.PurchaseOrderItemsByProduct),
                m.PurchaseOrderItem.RolePattern(v => v.Part, v => v.PurchaseOrderWherePurchaseOrderItem.ObjectType.PurchaseOrderItemsByProduct),
                m.PurchaseOrderItem.RolePattern(v => v.QuantityOrdered, v => v.PurchaseOrderWherePurchaseOrderItem.ObjectType.PurchaseOrderItemsByProduct),
                m.PurchaseOrderItem.RolePattern(v => v.TotalBasePrice, v => v.PurchaseOrderWherePurchaseOrderItem.ObjectType.PurchaseOrderItemsByProduct),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
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

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

    public class SalesOrderItemByProductDerivation : DomainDerivation
    {
        public SalesOrderItemByProductDerivation(M m) : base(m, new Guid("89dc8d53-d4a2-4e03-aa87-cbde9659cf87")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.SalesOrderItemVersion.Product) { Steps = new IPropertyType[] {m.SalesOrderItemVersion.SalesOrderItemWhereCurrentVersion, m.SalesOrderItem.SalesOrderWhereSalesOrderItem, m.SalesOrder.SalesOrderItemsByProduct} },
                new ChangedPattern(this.M.SalesOrderItem.Product) { Steps = new IPropertyType[] {m.SalesOrderItem.SalesOrderWhereSalesOrderItem, m.SalesOrder.SalesOrderItemsByProduct} },
                new ChangedPattern(this.M.SalesOrderItem.QuantityOrdered) { Steps = new IPropertyType[] {m.SalesOrderItem.SalesOrderWhereSalesOrderItem, m.SalesOrder.SalesOrderItemsByProduct} },
                new ChangedPattern(this.M.SalesOrderItem.TotalBasePrice) { Steps = new IPropertyType[] {m.SalesOrderItem.SalesOrderWhereSalesOrderItem, m.SalesOrder.SalesOrderItemsByProduct} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrderItemByProduct>())
            {
                var sameProductItems = @this.SalesOrderWhereSalesOrderItemsByProduct?.SalesOrderItems
                    .Where(v => v.IsValid && v.ExistProduct && v.Product.Equals(@this.Product))
                    .ToArray();

                @this.QuantityOrdered = sameProductItems.Sum(w => w.QuantityOrdered);
                @this.ValueOrdered = sameProductItems.Sum(w => w.TotalBasePrice);
            }
        }
    }
}

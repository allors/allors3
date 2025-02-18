// <copyright file="Domain.cs" company="Allors bv">
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

    public class SalesOrderItemByProductRule : Rule
    {
        public SalesOrderItemByProductRule(MetaPopulation m) : base(m, new Guid("89dc8d53-d4a2-4e03-aa87-cbde9659cf87")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrderItemVersion.RolePattern(v => v.Product, v => v.SalesOrderItemWhereCurrentVersion.ObjectType.SalesOrderWhereSalesOrderItem.ObjectType.SalesOrderItemsByProduct ),
                m.SalesOrderItem.RolePattern(v => v.Product, v => v.SalesOrderWhereSalesOrderItem.ObjectType.SalesOrderItemsByProduct ),
                m.SalesOrderItem.RolePattern(v => v.QuantityOrdered, v => v.SalesOrderWhereSalesOrderItem.ObjectType.SalesOrderItemsByProduct ),
                m.SalesOrderItem.RolePattern(v => v.TotalBasePrice, v => v.SalesOrderWhereSalesOrderItem.ObjectType.SalesOrderItemsByProduct ),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
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

// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class SalesOrderItemSalesOrderItemsByProductRule : Rule
    {
        public SalesOrderItemSalesOrderItemsByProductRule(MetaPopulation m) : base(m, new Guid("0bd194e2-b72b-4502-8609-a53c38cc2cb8")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrderItem.RolePattern(v => v.Product),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrderItem>())
            {
                var salesOrder = @this.SalesOrderWhereSalesOrderItem;

                if (salesOrder != null
                    && @this.ExistProduct
                    && !salesOrder.SalesOrderItemsByProduct.Any(v => v.Product.Equals(@this.Product)))
                {
                    salesOrder.AddSalesOrderItemsByProduct(new SalesOrderItemByProductBuilder(transaction).WithProduct(@this.Product).Build());
                }

            }
        }
    }
}

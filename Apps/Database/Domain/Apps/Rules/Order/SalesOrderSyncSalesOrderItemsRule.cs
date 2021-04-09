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

    public class SalesOrderSyncSalesOrderItemsRule : Rule
    {
        public SalesOrderSyncSalesOrderItemsRule(MetaPopulation m) : base(m, new Guid("abf19c62-816f-4c15-8c73-297b1f835173")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.SalesOrderItems),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SalesOrder>())
            {
                foreach (SalesOrderItem salesOrderItem in @this.SalesOrderItems)
                {
                    salesOrderItem.Sync(@this);
                }
            }
        }
    }
}

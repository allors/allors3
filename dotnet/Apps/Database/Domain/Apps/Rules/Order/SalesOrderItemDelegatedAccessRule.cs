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

    public class SalesOrderItemDelegatedAccessRule : Rule
    {
        public SalesOrderItemDelegatedAccessRule(MetaPopulation m) : base(m, new Guid("abf19c62-816f-4c15-8c73-297b1f835173")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.SalesOrderItems),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SalesOrder>())
            {
                foreach (var salesOrderItem in @this.SalesOrderItems)
                {
                    salesOrderItem.DelegatedAccess = @this;
                }
            }
        }
    }
}

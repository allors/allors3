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
    using Derivations.Rules;

    public class SalesOrderCustomerRule : Rule
    {
        public SalesOrderCustomerRule(MetaPopulation m) : base(m, new Guid("3a868039-b573-43b3-9ea5-1f91ad55a1ff")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.BillToCustomer),
                m.SalesOrder.RolePattern(v => v.ShipToCustomer),
                m.SalesOrder.RolePattern(v => v.PlacingCustomer),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SalesOrder>())
            {
                @this.BillToCustomer ??= @this.ShipToCustomer;
                @this.ShipToCustomer ??= @this.BillToCustomer;
                @this.Customers = new[] { @this.BillToCustomer, @this.ShipToCustomer, @this.PlacingCustomer };
            }
        }
    }
}

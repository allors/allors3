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

    public class SalesOrderProvisionalShipToAddressRule : Rule
    {
        public SalesOrderProvisionalShipToAddressRule(MetaPopulation m) : base(m, new Guid("13d04aec-53c6-4863-bb28-e2992990a2b6")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.SalesOrderState),
                m.SalesOrder.RolePattern(v => v.AssignedShipToAddress),
                m.SalesOrder.RolePattern(v => v.ShipToCustomer),
                m.Party.RolePattern(v => v.ShippingAddress, v => v.SalesOrdersWhereBillToCustomer),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                @this.DerivedShipToAddress = @this.AssignedShipToAddress ?? @this.ShipToCustomer?.ShippingAddress;
            }
        }
    }
}

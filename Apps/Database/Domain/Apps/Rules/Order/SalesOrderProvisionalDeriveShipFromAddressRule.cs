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

    public class SalesOrderProvisionalDeriveShipFromAddressRule : Rule
    {
        public SalesOrderProvisionalDeriveShipFromAddressRule(MetaPopulation m) : base(m, new Guid("69682dd0-7700-4a16-976a-66d9e0c34641")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesOrder, m.SalesOrder.SalesOrderState),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedShipFromAddress),
                new RolePattern(m.SalesOrder, m.SalesOrder.TakenBy),
                                new RolePattern(m.Organisation, m.Organisation.ShippingAddress) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                @this.DerivedShipFromAddress = @this.AssignedShipFromAddress ?? @this.TakenBy?.ShippingAddress;
            }
        }
    }
}

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

    public class SalesOrderProvisionalShipFromAddressRule : Rule
    {
        public SalesOrderProvisionalShipFromAddressRule(MetaPopulation m) : base(m, new Guid("69682dd0-7700-4a16-976a-66d9e0c34641")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.SalesOrderState),
                m.SalesOrder.RolePattern(v => v.AssignedShipFromAddress),
                m.SalesOrder.RolePattern(v => v.TakenBy),
                m.Organisation.RolePattern(v=>v.ShippingAddress, v=>v.SalesOrdersWhereTakenBy)
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

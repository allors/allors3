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

    public class SalesOrderItemProvisionalDeriveShipToPartyRule : Rule
    {
        public SalesOrderItemProvisionalDeriveShipToPartyRule(MetaPopulation m) : base(m, new Guid("6275f7c5-be38-44df-88d1-8bed31b641cb")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.SalesOrder.SalesOrderItems),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.SalesOrderItemState),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.AssignedShipToParty),
                new RolePattern(m.SalesOrder, m.SalesOrder.ShipToCustomer) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems} },
                new RolePattern(m.Organisation, m.Organisation.ShippingAddress) { Steps = new IPropertyType[] { m.Organisation.SalesOrderItemsWhereAssignedShipToParty  }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrderItem>().Where(v => v.SalesOrderItemState.IsProvisional))
            {
                var salesOrder = @this.SalesOrderWhereSalesOrderItem;

                @this.DerivedShipToParty = @this.AssignedShipToParty ?? salesOrder?.ShipToCustomer;
            }
        }
    }
}

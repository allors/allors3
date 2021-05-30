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

    public class SalesOrderItemProvisionalShipToPartyRule : Rule
    {
        public SalesOrderItemProvisionalShipToPartyRule(MetaPopulation m) : base(m, new Guid("6275f7c5-be38-44df-88d1-8bed31b641cb")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrderItem.AssociationPattern(v => v.SalesOrderWhereSalesOrderItem),
                m.SalesOrderItem.RolePattern(v => v.SalesOrderItemState),
                m.SalesOrderItem.RolePattern(v => v.AssignedShipToParty),
                m.SalesOrder.RolePattern(v => v.ShipToCustomer, v => v.SalesOrderItems),
                m.Organisation.RolePattern(v => v.ShippingAddress, v => v.SalesOrderItemsWhereAssignedShipToParty),
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

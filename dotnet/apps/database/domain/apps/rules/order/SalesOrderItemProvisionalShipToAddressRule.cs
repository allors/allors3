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

    public class SalesOrderItemProvisionalShipToAddressRule : Rule
    {
        public SalesOrderItemProvisionalShipToAddressRule(MetaPopulation m) : base(m, new Guid("c533c0cd-0085-4678-9b1a-7940255a24c7")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrderItem.AssociationPattern(v => v.SalesOrderWhereSalesOrderItem),
                m.SalesOrderItem.RolePattern(v => v.SalesOrderItemState),
                m.SalesOrderItem.RolePattern(v => v.AssignedShipToAddress),
                m.SalesOrderItem.RolePattern(v => v.AssignedShipToParty),
                m.SalesOrder.RolePattern(v => v.DerivedShipToAddress, v => v.SalesOrderItems),
                m.Organisation.RolePattern(v => v.ShippingAddress, v => v.SalesOrderItemsWhereAssignedShipToParty),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrderItem>().Where(v => v.SalesOrderItemState.IsProvisional))
            {
                var salesOrder = @this.SalesOrderWhereSalesOrderItem;

                @this.DerivedShipToAddress = @this.AssignedShipToAddress ?? @this.AssignedShipToParty?.ShippingAddress ?? salesOrder?.DerivedShipToAddress;
            }
        }
    }
}

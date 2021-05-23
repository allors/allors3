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

    public class SalesOrderItemProvisionalShipFromAddressRule : Rule
    {
        public SalesOrderItemProvisionalShipFromAddressRule(MetaPopulation m) : base(m, new Guid("6a824045-939a-48f1-954e-2905fcc0942e")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrderItem.AssociationPattern(v => v.SalesOrderWhereSalesOrderItem),
                m.SalesOrderItem.RolePattern(v => v.SalesOrderItemState),
                m.SalesOrderItem.RolePattern(v => v.AssignedShipFromAddress),
                m.SalesOrder.RolePattern(v => v.DerivedShipFromAddress, v => v.SalesOrderItems),
                m.Organisation.RolePattern(v => v.ShippingAddress, v => v.SalesOrderItemsWhereAssignedShipToParty),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrderItem>().Where(v => v.SalesOrderItemState.IsProvisional))
            {
                var salesOrder = @this.SalesOrderWhereSalesOrderItem;

                @this.DerivedShipFromAddress = @this.AssignedShipFromAddress ?? salesOrder?.DerivedShipFromAddress;
            }
        }
    }
}

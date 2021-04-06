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

    public class SalesOrderItemProvisionalDeriveShipFromAddressRule : Rule
    {
        public SalesOrderItemProvisionalDeriveShipFromAddressRule(MetaPopulation m) : base(m, new Guid("6a824045-939a-48f1-954e-2905fcc0942e")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.SalesOrder.SalesOrderItems),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.SalesOrderItemState),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.AssignedShipFromAddress),
                new RolePattern(m.SalesOrder, m.SalesOrder.DerivedShipFromAddress) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems} },new RolePattern(m.Organisation, m.Organisation.ShippingAddress) { Steps = new IPropertyType[] { m.Organisation.SalesOrderItemsWhereAssignedShipToParty  }},
                new RolePattern(m.Organisation, m.Organisation.ShippingAddress) { Steps = new IPropertyType[] { m.Organisation.SalesOrderItemsWhereAssignedShipToParty  }},
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

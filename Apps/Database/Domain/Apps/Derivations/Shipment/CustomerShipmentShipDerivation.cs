// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;

    public class CustomerShipmentShipDerivation : DomainDerivation
    {
        public CustomerShipmentShipDerivation(M m) : base(m, new Guid("09c6d242-b089-4fe3-860c-3df2e39c00f3")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.CustomerShipment.ShipmentState),
                new AssociationPattern(m.PickList.ShipToParty) { Steps = new IPropertyType[] { m.Party.ShipmentsWhereShipToParty }, OfType = m.CustomerShipment.Class },
                new RolePattern(m.PickList.PickListState) { Steps = new IPropertyType[] { m.PickList.ShipToParty, m.Party.ShipmentsWhereShipToParty }, OfType = m.CustomerShipment.Class },
                new RolePattern(m.PickListVersion.ShipToParty) { Steps = new IPropertyType[] { m.PickListVersion.PickListWhereCurrentVersion, m.PickList.ShipToParty, m.Party.ShipmentsWhereShipToParty }, OfType = m.CustomerShipment.Class },
                new RolePattern(m.SalesOrder.SalesOrderState) { Steps = new IPropertyType[] { m.SalesOrder.SalesOrderItems, m.SalesOrderItem.OrderShipmentsWhereOrderItem, m.OrderShipment.ShipmentItem, m.ShipmentItem.ShipmentWhereShipmentItem }, OfType = m.CustomerShipment.Class },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerShipment>())
            {
                if (@this.CanShip && @this.Store.IsAutomaticallyShipped)
                {
                    @this.Ship();
                }
            }
        }
    }
}

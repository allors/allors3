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

    public class CustomerShipmentShipRule : Rule
    {
        public CustomerShipmentShipRule(MetaPopulation m) : base(m, new Guid("09c6d242-b089-4fe3-860c-3df2e39c00f3")) =>
            this.Patterns = new Pattern[]
            {
                m.CustomerShipment.RolePattern(v => v.ShipmentState),
                m.PickList.RolePattern(v => v.PickListState, v => v.ShipToParty.Party.ShipmentsWhereShipToParty.Shipment.AsCustomerShipment),
                m.PickListVersion.RolePattern(v => v.ShipToParty, v => v.PickListWhereCurrentVersion.PickList.ShipToParty.Party.ShipmentsWhereShipToParty.Shipment.AsCustomerShipment),
                m.SalesOrder.RolePattern(v => v.SalesOrderState, v => v.SalesOrderItems.SalesOrderItem.OrderShipmentsWhereOrderItem.OrderShipment.ShipmentItem.ShipmentItem.ShipmentWhereShipmentItem.Shipment.AsCustomerShipment),
                m.Party.AssociationPattern(v => v.PickListsWhereShipToParty, v => v.ShipmentsWhereShipToParty.Shipment.AsCustomerShipment),
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

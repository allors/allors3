// <copyright file="CustomerShipmentShipmentValueDerivation.cs" company="Allors bvba">
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

    public class CustomerShipmentShipmentValueRule : Rule
    {
        public CustomerShipmentShipmentValueRule(MetaPopulation m) : base(m, new Guid("fefff2c8-12dd-4ef5-b2c7-923bb80c2ec3")) =>
            this.Patterns = new Pattern[]
            {
                m.OrderShipment.RolePattern(v => v.Quantity, v => v.ShipmentItem.ShipmentItem.ShipmentWhereShipmentItem.Shipment.AsCustomerShipment),
                m.SalesOrderItem.RolePattern(v => v.UnitPrice, v => v.OrderShipmentsWhereOrderItem.OrderShipment.ShipmentItem.ShipmentItem.ShipmentWhereShipmentItem.Shipment.AsCustomerShipment),
                m.ShipmentItem.AssociationPattern(v => v.OrderShipmentsWhereShipmentItem, v => v.ShipmentWhereShipmentItem.Shipment.AsCustomerShipment),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerShipment>())
            {
                var shipmentValue = 0M;
                foreach (ShipmentItem shipmentItem in @this.ShipmentItems)
                {
                    foreach (OrderShipment orderShipment in shipmentItem.OrderShipmentsWhereShipmentItem)
                    {
                        shipmentValue += orderShipment.Quantity * orderShipment.OrderItem.UnitPrice;
                    }
                }

                @this.ShipmentValue = shipmentValue;
            }
        }
    }
}

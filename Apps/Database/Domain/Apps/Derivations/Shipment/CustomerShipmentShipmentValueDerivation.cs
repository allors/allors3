// <copyright file="CustomerShipmentShipmentValueDerivation.cs" company="Allors bvba">
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

    public class CustomerShipmentShipmentValueDerivation : DomainDerivation
    {
        public CustomerShipmentShipmentValueDerivation(M m) : base(m, new Guid("fefff2c8-12dd-4ef5-b2c7-923bb80c2ec3")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.OrderShipment.ShipmentItem) { Steps = new IPropertyType[] { m.OrderShipment.ShipmentItem, m.ShipmentItem.ShipmentWhereShipmentItem }, OfType = m.CustomerShipment.Class },
                new ChangedPattern(m.OrderShipment.Quantity) { Steps = new IPropertyType[] { m.OrderShipment.ShipmentItem, m.ShipmentItem.ShipmentWhereShipmentItem }, OfType = m.CustomerShipment.Class },
                new ChangedPattern(m.SalesOrderItem.UnitPrice) { Steps = new IPropertyType[] { m.SalesOrderItem.OrderShipmentsWhereOrderItem, m.OrderShipment.ShipmentItem, m.ShipmentItem.ShipmentWhereShipmentItem }, OfType = m.CustomerShipment.Class },
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
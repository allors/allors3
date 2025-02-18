// <copyright file="CustomerShipmentShipmentValueDerivation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class CustomerShipmentShipmentValueRule : Rule
    {
        public CustomerShipmentShipmentValueRule(MetaPopulation m) : base(m, new Guid("fefff2c8-12dd-4ef5-b2c7-923bb80c2ec3")) =>
            this.Patterns = new Pattern[]
            {
                m.OrderShipment.RolePattern(v => v.Quantity, v => v.ShipmentItem.ObjectType.ShipmentWhereShipmentItem.ObjectType.AsCustomerShipment),
                m.SalesOrderItem.RolePattern(v => v.UnitPrice, v => v.OrderShipmentsWhereOrderItem.ObjectType.ShipmentItem.ObjectType.ShipmentWhereShipmentItem.ObjectType.AsCustomerShipment),
                m.ShipmentItem.AssociationPattern(v => v.OrderShipmentsWhereShipmentItem, v => v.ShipmentWhereShipmentItem.ObjectType.AsCustomerShipment),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerShipment>())
            {
                var shipmentValue = 0M;
                foreach (var shipmentItem in @this.ShipmentItems)
                {
                    foreach (var orderShipment in shipmentItem.OrderShipmentsWhereShipmentItem)
                    {
                        shipmentValue += orderShipment.Quantity * orderShipment.OrderItem.UnitPrice;
                    }
                }

                @this.ShipmentValue = shipmentValue;
            }
        }
    }
}

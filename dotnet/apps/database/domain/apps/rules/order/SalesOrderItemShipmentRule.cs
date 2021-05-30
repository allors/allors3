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
    using Resources;

    public class SalesOrderItemShipmentRule : Rule
    {
        public SalesOrderItemShipmentRule(MetaPopulation m) : base(m, new Guid("0fc6919a-45d3-4e71-bf4d-1baca964f204")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrderItem.RolePattern(v => v.QuantityOrdered),
                m.OrderShipment.RolePattern(v => v.Quantity, v => v.OrderItem, m.SalesOrderItem),
                m.ShipmentItem.RolePattern(v => v.ShipmentItemState, v => v.OrderShipmentsWhereShipmentItem.OrderShipment.OrderItem, m.SalesOrderItem),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrderItem>())
            {
                @this.QuantityPendingShipment = @this.OrderShipmentsWhereOrderItem
                    .Where(v => v.ExistShipmentItem && !((CustomerShipment)v.ShipmentItem.ShipmentWhereShipmentItem).ShipmentState.IsShipped)
                    .Sum(v => v.Quantity);

                @this.QuantityShipped = @this.OrderShipmentsWhereOrderItem
                    .Where(v => v.ExistShipmentItem && ((CustomerShipment)v.ShipmentItem.ShipmentWhereShipmentItem).ShipmentState.IsShipped)
                    .Sum(v => v.Quantity);

                if (@this.QuantityOrdered < @this.QuantityPendingShipment || @this.QuantityOrdered < @this.QuantityShipped)
                {
                    validation.AddError($"{@this} {this.M.SalesOrderItem.QuantityOrdered} {ErrorMessages.SalesOrderItemQuantityToShipNowIsLargerThanQuantityRemaining}");
                }
            }
        }
    }
}

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
    using Resources;

    public class SalesOrderItemShipmentDerivation : DomainDerivation
    {
        public SalesOrderItemShipmentDerivation(M m) : base(m, new Guid("0fc6919a-45d3-4e71-bf4d-1baca964f204")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.SalesOrderItem.QuantityOrdered),
                new ChangedPattern(m.OrderShipment.Quantity) {Steps = new IPropertyType[]{ m.OrderShipment.OrderItem}, OfType = m.SalesOrderItem.Class },
                new ChangedPattern(m.ShipmentItem.ShipmentItemState) {Steps = new IPropertyType[]{m.ShipmentItem.OrderShipmentsWhereShipmentItem, m.OrderShipment.OrderItem}, OfType = m.SalesOrderItem.Class },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

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

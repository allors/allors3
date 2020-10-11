// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;
    using Resources;

    public class OrderShipmentDerivation : DomainDerivation
    {
        public OrderShipmentDerivation(M m) : base(m, new Guid("A4B63A0B-C6AF-44CB-B778-3CD75EDBE2B7")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.OrderShipment.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var orderShipment in matches.Cast<OrderShipment>())
            {
                if (orderShipment.ShipmentItem.ShipmentWhereShipmentItem is CustomerShipment customerShipment && orderShipment.OrderItem is SalesOrderItem salesOrderItem)
                {
                    var quantityPendingShipment = orderShipment.OrderItem?.OrderShipmentsWhereOrderItem?
                        .Where(v => v.ExistShipmentItem
                                    && !((CustomerShipment)v.ShipmentItem.ShipmentWhereShipmentItem).ShipmentState.Equals(new ShipmentStates(orderShipment.Session()).Shipped))
                        .Sum(v => v.Quantity);

                    if (salesOrderItem.QuantityPendingShipment > quantityPendingShipment)
                    {
                        var diff = orderShipment.Quantity * -1;

                        // HACK: DerivedRoles
                        (salesOrderItem).QuantityPendingShipment -= diff;
                        customerShipment.BaseOnDeriveQuantityDecreased(orderShipment.ShipmentItem, salesOrderItem, diff);
                    }

                    if (orderShipment.Strategy.IsNewInSession)
                    {
                        var quantityPicked = orderShipment.OrderItem.OrderShipmentsWhereOrderItem.Select(v => v.ShipmentItem?.ItemIssuancesWhereShipmentItem.Sum(z => z.PickListItem.Quantity)).Sum();
                        var pendingFromOthers = salesOrderItem.QuantityPendingShipment - orderShipment.Quantity;

                        if (salesOrderItem.QuantityRequestsShipping > 0)
                        {
                            // HACK: DerivedRoles
                            (salesOrderItem).QuantityRequestsShipping -= orderShipment.Quantity;
                        }

                        if (salesOrderItem.ExistReservedFromNonSerialisedInventoryItem && orderShipment.Quantity > salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityOnHand + quantityPicked)
                        {
                            validation.AddError($"{orderShipment} {this.M.OrderShipment.Quantity} {ErrorMessages.SalesOrderItemQuantityToShipNowNotAvailable}");
                        }
                        else if (orderShipment.Quantity > salesOrderItem.QuantityOrdered)
                        {
                            validation.AddError($"{orderShipment} {this.M.OrderShipment.Quantity} {ErrorMessages.SalesOrderItemQuantityToShipNowIsLargerThanQuantityOrdered}");
                        }
                        else
                        {
                            if (orderShipment.Quantity > salesOrderItem.QuantityOrdered - salesOrderItem.QuantityShipped - pendingFromOthers + salesOrderItem.QuantityReturned + quantityPicked)
                            {
                                validation.AddError($"{orderShipment} {this.M.OrderShipment.Quantity} {ErrorMessages.SalesOrderItemQuantityToShipNowIsLargerThanQuantityRemaining}");
                            }
                        }
                    }
                }
            }
        }
    }
}

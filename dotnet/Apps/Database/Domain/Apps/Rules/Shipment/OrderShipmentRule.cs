// <copyright file="Domain.cs" company="Allors bv">
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
    using Resources;

    public class OrderShipmentRule : Rule
    {
        public OrderShipmentRule(MetaPopulation m) : base(m, new Guid("A4B63A0B-C6AF-44CB-B778-3CD75EDBE2B7")) =>
            this.Patterns = new Pattern[]
            {
                m.OrderShipment.RolePattern(v => v.ShipmentItem),
                m.OrderShipment.RolePattern(v => v.OrderItem),
                m.OrderShipment.RolePattern(v => v.Quantity),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<OrderShipment>())
            {
                if (@this.ExistShipmentItem
                    && @this.ExistOrderItem
                    && @this.ShipmentItem.ShipmentWhereShipmentItem is CustomerShipment customerShipment && @this.OrderItem is SalesOrderItem salesOrderItem)
                {
                    if (@this.ExistCurrentVersion && @this.CurrentVersion.Quantity > @this.Quantity)
                    {
                        var diff = @this.CurrentVersion.Quantity - @this.Quantity;

                        customerShipment.AppsOnDeriveQuantityDecreased(@this.ShipmentItem, salesOrderItem, diff);
                    }

                    if (@this.Strategy.IsNewInTransaction)
                    {
                        var quantityPicked = @this.OrderItem.OrderShipmentsWhereOrderItem.Select(v => v.ShipmentItem?.ItemIssuancesWhereShipmentItem.Sum(z => z.PickListItem.Quantity)).Sum();
                        var pendingFromOthers = salesOrderItem.QuantityPendingShipment - @this.Quantity;

                        if (salesOrderItem.QuantityRequestsShipping > 0)
                        {
                            salesOrderItem.QuantityRequestsShipping -= @this.Quantity;
                        }

                        if (salesOrderItem.ExistReservedFromNonSerialisedInventoryItem && @this.Quantity > salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityOnHand + quantityPicked)
                        {
                            validation.AddError(@this, this.M.OrderShipment.Quantity, ErrorMessages.SalesOrderItemQuantityToShipNowNotAvailable);
                        }
                        else if (@this.Quantity > salesOrderItem.QuantityOrdered)
                        {
                            validation.AddError(@this, this.M.OrderShipment.Quantity, ErrorMessages.SalesOrderItemQuantityToShipNowIsLargerThanQuantityOrdered);
                        }
                        else if (@this.Quantity > salesOrderItem.QuantityOrdered - salesOrderItem.QuantityShipped - pendingFromOthers + salesOrderItem.QuantityReturned + quantityPicked)
                        {
                            validation.AddError(@this, this.M.OrderShipment.Quantity, ErrorMessages.SalesOrderItemQuantityToShipNowIsLargerThanQuantityRemaining);
                        }
                    }
                }
            }
        }
    }
}

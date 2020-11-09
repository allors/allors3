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

    public class PickListItemDerivation : DomainDerivation
    {
        public PickListItemDerivation(M m) : base(m, new Guid("7E5843FB-7D25-4D41-833B-077C1B83AAD1")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.PickListItem.QuantityPicked),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PickListItem>())
            {
                if (@this.Quantity > 0 && @this.QuantityPicked > @this.Quantity)
                {
                    cycle.Validation.AddError($"{@this} {this.M.PickListItem.QuantityPicked} {ErrorMessages.PickListItemQuantityMoreThanAllowed}");
                }

                if (@this.QuantityPicked > 0 && @this.ExistPickListWherePickListItem && @this.PickListWherePickListItem.PickListState.Equals(new PickListStates(@this.Session()).Picked))
                {
                    var quantityProcessed = @this.ItemIssuancesWherePickListItem.SelectMany(v => v.ShipmentItem.OrderShipmentsWhereShipmentItem).Sum(v => v.Quantity);
                    var diff = quantityProcessed - @this.QuantityPicked;

                    foreach (ItemIssuance itemIssuance in @this.ItemIssuancesWherePickListItem)
                    {
                        itemIssuance.IssuanceDateTime = @this.Session().Now();
                        foreach (OrderShipment orderShipment in itemIssuance.ShipmentItem.OrderShipmentsWhereShipmentItem)
                        {
                            if (orderShipment.OrderItem is SalesOrderItem salesOrderItem)
                            {
                                if (diff > 0 && @this.QuantityPicked != orderShipment.Quantity)
                                {
                                    if (orderShipment.Quantity >= diff)
                                    {
                                        new OrderShipmentBuilder(@this.Strategy.Session)
                                            .WithOrderItem(salesOrderItem)
                                            .WithShipmentItem(orderShipment.ShipmentItem)
                                            .WithQuantity(diff * -1)
                                            .Build();

                                        diff = 0;
                                    }
                                    else
                                    {
                                        new OrderShipmentBuilder(@this.Strategy.Session)
                                            .WithOrderItem(salesOrderItem)
                                            .WithShipmentItem(orderShipment.ShipmentItem)
                                            .WithQuantity(orderShipment.Quantity * -1)
                                            .Build();

                                        diff -= orderShipment.Quantity;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

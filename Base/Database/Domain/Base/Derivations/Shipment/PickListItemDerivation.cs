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
                new CreatedPattern(this.M.PickListItem.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var pickListItems in matches.Cast<PickListItem>())
            {
                if (pickListItems.Quantity > 0 && pickListItems.QuantityPicked > pickListItems.Quantity)
                {
                    cycle.Validation.AddError($"{pickListItems} {this.M.PickListItem.QuantityPicked} {ErrorMessages.PickListItemQuantityMoreThanAllowed}");
                }

                if (pickListItems.QuantityPicked > 0 && pickListItems.ExistPickListWherePickListItem && pickListItems.PickListWherePickListItem.PickListState.Equals(new PickListStates(pickListItems.Session()).Picked))
                {
                    var quantityProcessed = pickListItems.ItemIssuancesWherePickListItem.SelectMany(v => v.ShipmentItem.OrderShipmentsWhereShipmentItem).Sum(v => v.Quantity);
                    var diff = quantityProcessed - pickListItems.QuantityPicked;

                    foreach (ItemIssuance itemIssuance in pickListItems.ItemIssuancesWherePickListItem)
                    {
                        itemIssuance.IssuanceDateTime = pickListItems.Session().Now();
                        foreach (OrderShipment orderShipment in itemIssuance.ShipmentItem.OrderShipmentsWhereShipmentItem)
                        {
                            if (orderShipment.OrderItem is SalesOrderItem salesOrderItem)
                            {
                                if (diff > 0 && pickListItems.QuantityPicked != orderShipment.Quantity)
                                {
                                    if (orderShipment.Quantity >= diff)
                                    {
                                        new OrderShipmentBuilder(pickListItems.Strategy.Session)
                                            .WithOrderItem(salesOrderItem)
                                            .WithShipmentItem(orderShipment.ShipmentItem)
                                            .WithQuantity(diff * -1)
                                            .Build();

                                        diff = 0;
                                    }
                                    else
                                    {
                                        new OrderShipmentBuilder(pickListItems.Strategy.Session)
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

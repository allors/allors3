// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class PickListItemCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdPickListItems = changeSet.Created.Select(v=>v.GetObject()).OfType<PickListItem>();

                foreach(var pickListItems in createdPickListItems)
                {
                    if (pickListItems.Quantity > 0 && pickListItems.QuantityPicked > pickListItems.Quantity)
                    {
                        validation.AddError($"{pickListItems} {M.PickListItem.QuantityPicked} {ErrorMessages.PickListItemQuantityMoreThanAllowed}");
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

        public static void PickListItemRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("0268ad59-05cd-4ffb-9c0d-bf57df0fdc7d")] = new PickListItemCreationDerivation();
        }
    }
}

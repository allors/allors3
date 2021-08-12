// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

    public class PickListItemRule : Rule
    {
        public PickListItemRule(MetaPopulation m) : base(m, new Guid("7E5843FB-7D25-4D41-833B-077C1B83AAD1")) =>
            this.Patterns = new Pattern[]
            {
                m.PickListItem.RolePattern(v => v.Quantity),
                m.PickListItem.RolePattern(v => v.QuantityPicked),
                m.PickList.RolePattern(v => v.PickListState, v => v.PickListItems),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PickListItem>())
            {
                if (@this.Quantity > 0 && @this.QuantityPicked > @this.Quantity)
                {
                    cycle.Validation.AddError(@this, this.M.PickListItem.QuantityPicked, ErrorMessages.PickListItemQuantityMoreThanAllowed);
                }

                if (@this.QuantityPicked > 0 && @this.ExistPickListWherePickListItem && @this.PickListWherePickListItem.PickListState.IsPicked)
                {
                    var quantityProcessed = @this.ItemIssuancesWherePickListItem.SelectMany(v => v.ShipmentItem.OrderShipmentsWhereShipmentItem).Sum(v => v.Quantity);
                    var diff = quantityProcessed - @this.QuantityPicked;

                    foreach (var itemIssuance in @this.ItemIssuancesWherePickListItem)
                    {
                        itemIssuance.IssuanceDateTime = @this.Transaction().Now();
                        foreach (var orderShipment in itemIssuance.ShipmentItem.OrderShipmentsWhereShipmentItem)
                        {
                            if (orderShipment.OrderItem is SalesOrderItem salesOrderItem)
                            {
                                if (diff > 0 && @this.QuantityPicked != orderShipment.Quantity)
                                {
                                    if (orderShipment.Quantity >= diff)
                                    {
                                        orderShipment.Quantity -= diff;
                                        diff = 0;
                                    }
                                    else
                                    {
                                        diff -= orderShipment.Quantity;
                                        orderShipment.Quantity = 0;
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

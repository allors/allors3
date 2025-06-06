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

    public class CustomerShipmentStateRule : Rule
    {
        public CustomerShipmentStateRule(MetaPopulation m) : base(m, new Guid("7ff771c2-af69-427a-aeb0-4ceb73e699c7")) =>
            this.Patterns = new Pattern[]
            {
                m.CustomerShipment.RolePattern(v => v.DerivationTrigger),
                m.CustomerShipment.RolePattern(v => v.ShipToParty),
                m.CustomerShipment.RolePattern(v => v.ShipmentItems),
                m.CustomerShipment.RolePattern(v => v.ShipmentState),
                m.CustomerShipment.RolePattern(v => v.ShipmentValue),
                m.CustomerShipment.RolePattern(v => v.ReleasedManually),
                m.ShipmentItem.RolePattern(v => v.Quantity, v => v.ShipmentWhereShipmentItem.ObjectType.AsCustomerShipment),
                m.PackagingContent.RolePattern(v => v.Quantity, v => v.ShipmentItem.ObjectType.ShipmentWhereShipmentItem.ObjectType.AsCustomerShipment),
                m.PickList.RolePattern(v => v.PickListState, v => v.ShipToParty.ObjectType.ShipmentsWhereShipToParty.ObjectType.AsCustomerShipment),
                m.PickList.RolePattern(v => v.CurrentVersion, v => v.AllVersions.ObjectType.ShipToParty.ObjectType.ShipmentsWhereShipToParty.ObjectType.AsCustomerShipment),
                m.Store.RolePattern(v => v.ShipmentThreshold, v => v.ShipmentsWhereStore.ObjectType.AsCustomerShipment),
                m.Party.AssociationPattern(v => v.PickListsWhereShipToParty, v => v.ShipmentsWhereShipToParty.ObjectType.AsCustomerShipment),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerShipment>())
            {
                if (@this.ExistShipToParty && @this.ExistShipmentItems)
                {
                    //cancel shipment if nothing left to ship
                    if (@this.ExistShipmentItems && @this.PendingPickList == null
                        && !@this.ShipmentState.Equals(new ShipmentStates(@this.Transaction()).Cancelled)
                        && !@this.ShipmentState.Equals(new ShipmentStates(@this.Transaction()).Shipped))
                    {
                        var canCancel = true;
                        foreach (var shipmentItem in @this.ShipmentItems)
                        {
                            if (shipmentItem.Quantity > 0)
                            {
                                canCancel = false;
                                break;
                            }
                        }

                        if (canCancel)
                        {
                            @this.Cancel();
                        }
                    }

                    if (@this.ShipmentState.IsPicking && @this.ShipToParty.ExistPickListsWhereShipToParty)
                    {
                        var isPicked = true;
                        var pickList = @this.ShipmentItems.FirstOrDefault()?.ItemIssuancesWhereShipmentItem.FirstOrDefault()?.PickListItem.PickListWherePickListItem;

                        if (pickList != null
                            && @this.Store.Equals(pickList.Store)
                            && !pickList.PickListState.Equals(new PickListStates(@this.Transaction()).Picked))
                        {
                            isPicked = false;
                        }

                        if (pickList != null && isPicked)
                        {
                            @this.SetPicked();
                        }
                    }

                    if (@this.ShipmentState.IsPicked)
                    {
                        var totalShippingQuantity = 0M;
                        var totalPackagedQuantity = 0M;
                        foreach (var shipmentItem in @this.ShipmentItems)
                        {
                            totalShippingQuantity += shipmentItem.Quantity;
                            foreach (var packagingContent in shipmentItem.PackagingContentsWhereShipmentItem)
                            {
                                totalPackagedQuantity += packagingContent.Quantity;
                            }
                        }

                        if (@this.Store.IsImmediatelyPacked && totalPackagedQuantity == totalShippingQuantity)
                        {
                            @this.SetPacked();
                        }
                    }

                    if (@this.ShipmentState.IsCreated
                        || @this.ShipmentState.IsPicked
                        || @this.ShipmentState.IsPacked)
                    {
                        if (@this.ShipmentValue < @this.Store.ShipmentThreshold && !@this.ReleasedManually)
                        {
                            @this.PutOnHold();
                        }
                    }

                    if (@this.ShipmentState.Equals(new ShipmentStates(@this.Transaction()).OnHold) &&
                        !@this.HeldManually &&
                        (@this.ShipmentValue >= @this.Store.ShipmentThreshold || @this.ReleasedManually))
                    {
                        @this.Continue();
                    }
                }
            }
        }
    }
}

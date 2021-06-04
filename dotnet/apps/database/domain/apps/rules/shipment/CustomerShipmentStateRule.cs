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
                m.ShipmentItem.RolePattern(v => v.Quantity, v => v.ShipmentWhereShipmentItem.Shipment.AsCustomerShipment),
                m.PackagingContent.RolePattern(v => v.Quantity, v => v.ShipmentItem.ShipmentItem.ShipmentWhereShipmentItem.Shipment.AsCustomerShipment),
                m.PickList.RolePattern(v => v.PickListState, v => v.ShipToParty.Party.ShipmentsWhereShipToParty.Shipment.AsCustomerShipment),
                m.PickList.RolePattern(v => v.CurrentVersion, v => v.AllVersions.PickListVersion.ShipToParty.Party.ShipmentsWhereShipToParty.Shipment.AsCustomerShipment),
                m.Store.RolePattern(v => v.ShipmentThreshold, v => v.ShipmentsWhereStore.Shipment.AsCustomerShipment),
                m.Party.AssociationPattern(v => v.PickListsWhereShipToParty, v => v.ShipmentsWhereShipToParty.Shipment.AsCustomerShipment),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerShipment>())
            {
                if (@this.ExistShipToParty && @this.ExistShipmentItems)
                {
                    //cancel shipment if nothing left to ship
                    if (@this.ExistShipmentItems && @this.PendingPickList == null
                        && !@this.ShipmentState.Equals(new ShipmentStates(@this.Transaction()).Cancelled))
                    {
                        var canCancel = true;
                        foreach (ShipmentItem shipmentItem in @this.ShipmentItems)
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
                        foreach (PickList pickList in @this.ShipToParty.PickListsWhereShipToParty)
                        {
                            if (@this.Store.Equals(pickList.Store) &&
                                !pickList.PickListState.Equals(new PickListStates(@this.Transaction()).Picked))
                            {
                                isPicked = false;
                            }
                        }

                        if (isPicked)
                        {
                            @this.SetPicked();
                        }
                    }

                    if (@this.ShipmentState.IsPicked)
                    {
                        var totalShippingQuantity = 0M;
                        var totalPackagedQuantity = 0M;
                        foreach (ShipmentItem shipmentItem in @this.ShipmentItems)
                        {
                            totalShippingQuantity += shipmentItem.Quantity;
                            foreach (PackagingContent packagingContent in shipmentItem.PackagingContentsWhereShipmentItem)
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

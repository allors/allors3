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

    public class CustomerShipmentDerivation : DomainDerivation
    {
        public CustomerShipmentDerivation(M m) : base(m, new Guid("7FE90E97-A4B4-4991-9063-91BF5670B4A9")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.CustomerShipment.ShipmentState),
                new ChangedPattern(this.M.CustomerShipment.ShipmentPackages),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerShipment>())
            {
                if (!@this.ExistShipmentNumber && @this.ExistStore)
                {
                    var year = @this.Session().Now().Year;
                    @this.ShipmentNumber = @this.Store.NextOutgoingShipmentNumber(year);

                    var fiscalYearStoreSequenceNumbers = @this.Store.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = fiscalYearStoreSequenceNumbers == null ? @this.Store.OutgoingShipmentNumberPrefix : fiscalYearStoreSequenceNumbers.OutgoingShipmentNumberPrefix;
                    @this.SortableShipmentNumber = @this.Session().GetSingleton().SortableNumber(prefix, @this.ShipmentNumber, year.ToString());
                }

                cycle.Validation.AssertExists(@this, @this.Meta.ShipToParty);

                if (!@this.ExistShipToAddress && @this.ExistShipToParty)
                {
                    @this.ShipToAddress = @this.ShipToParty.ShippingAddress;
                }

                if (!@this.ExistShipFromAddress)
                {
                    @this.ShipFromAddress = @this.ShipFromParty?.ShippingAddress;
                }

                if (!@this.ExistShipFromFacility)
                {
                    @this.ShipFromFacility = ((Organisation)@this.ShipFromParty)?.FacilitiesWhereOwner.FirstOrDefault();
                }

                //AppsOnDeriveShipmentValue
                var shipmentValue = 0M;
                foreach (ShipmentItem shipmentItem in @this.ShipmentItems)
                {
                    foreach (OrderShipment orderShipment in shipmentItem.OrderShipmentsWhereShipmentItem)
                    {
                        shipmentValue += orderShipment.Quantity * orderShipment.OrderItem.UnitPrice;
                    }
                }

                @this.ShipmentValue = shipmentValue;

                if (@this.ExistShipToParty && @this.ExistShipmentItems)
                {
                    ////cancel shipment if nothing left to ship
                    if (@this.ExistShipmentItems && @this.PendingPickList == null
                        && !@this.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Cancelled))
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

                    var packed = @this.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Packed);
                    var picked = @this.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Picked);

                    if ((@this.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Picking) ||
                         picked ||
                         packed) &&
                         @this.ShipToParty.ExistPickListsWhereShipToParty)
                    {
                        var isPicked = true;
                        foreach (PickList pickList in @this.ShipToParty.PickListsWhereShipToParty)
                        {
                            if (@this.Store.Equals(pickList.Store) &&
                                !pickList.PickListState.Equals(new PickListStates(@this.Strategy.Session).Picked))
                            {
                                isPicked = false;
                            }
                        }

                        // TODO: Review martien

                        if (isPicked && !packed)
                        {
                            @this.SetPicked();
                        }
                    }

                    if (@this.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Picked)
                        || @this.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Packed))
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

                    if (@this.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Created)
                        || @this.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Picked)
                        || @this.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Picked)
                        || @this.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Packed))
                    {
                        if (@this.ShipmentValue < @this.Store.ShipmentThreshold && !@this.ReleasedManually)
                        {
                            @this.PutOnHold();
                        }
                    }

                    if (@this.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).OnHold) &&
                        !@this.HeldManually &&
                        ((@this.ShipmentValue >= @this.Store.ShipmentThreshold) || @this.ReleasedManually))
                    {
                        @this.Continue();
                    }
                }

                //

                if (@this.CanShip && @this.Store.IsAutomaticallyShipped)
                {
                    @this.Ship();
                }

                //AppsOnDeriveCurrentObjectState

                if (@this.ExistShipmentState && !@this.ShipmentState.Equals(@this.LastShipmentState) &&
                    @this.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Shipped))
                {
                    if (Equals(@this.Store.BillingProcess, new BillingProcesses(@this.Strategy.Session).BillingForShipmentItems))
                    {
                        @this.Invoice();
                    }
                }

                //

                if (@this.ShipmentState.IsShipped
                    && (!@this.ExistLastShipmentState || !@this.LastShipmentState.IsShipped))
                {
                    foreach (var item in @this.ShipmentItems.Where(v => v.ExistSerialisedItem))
                    {
                        if (item.ExistNextSerialisedItemAvailability)
                        {
                            item.SerialisedItem.SerialisedItemAvailability = item.NextSerialisedItemAvailability;

                            if ((@this.ShipFromParty as InternalOrganisation)?.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(@this.Session()).CustomerShipmentShip) == true
                                && item.NextSerialisedItemAvailability.Equals(new SerialisedItemAvailabilities(@this.Session()).Sold))
                            {
                                item.SerialisedItem.OwnedBy = @this.ShipToParty;
                                item.SerialisedItem.Ownership = new Ownerships(@this.Session()).ThirdParty;
                            }
                        }

                        item.SerialisedItem.AvailableForSale = false;
                    }
                }

                Sync(@this);
            }

            void Sync(CustomerShipment customerShipment)
            {
                // session.Prefetch(this.SyncPrefetch, this);
                foreach (ShipmentItem shipmentItem in customerShipment.ShipmentItems)
                {
                    shipmentItem.Sync(customerShipment);
                }
            }
        }
    }
}

// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public class CustomerShipmentDerivation : DomainDerivation
    {
        public CustomerShipmentDerivation(M m) : base(m, new Guid("7FE90E97-A4B4-4991-9063-91BF5670B4A9")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.CustomerShipment.Class),
                new ChangedConcreteRolePattern(this.M.CustomerShipment.ShipmentState),
                new ChangedConcreteRolePattern(this.M.CustomerShipment.ShipmentPackages),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var customerShipment in matches.Cast<CustomerShipment>())
            {
                if (!customerShipment.ExistShipmentNumber && customerShipment.ExistStore)
                {
                    customerShipment.ShipmentNumber = customerShipment.Store.NextShipmentNumber();
                    customerShipment.SortableShipmentNumber = customerShipment.Session().GetSingleton().SortableNumber(customerShipment.Store.OutgoingShipmentNumberPrefix, customerShipment.ShipmentNumber, customerShipment.CreationDate.Value.Year.ToString());
                }

                var internalOrganisations = new Organisations(customerShipment.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                if (!customerShipment.ExistShipFromParty && internalOrganisations.Count() == 1)
                {
                    customerShipment.ShipFromParty = internalOrganisations.First();
                }

                cycle.Validation.AssertExists(customerShipment, customerShipment.Meta.ShipToParty);

                if (!customerShipment.ExistShipToAddress && customerShipment.ExistShipToParty)
                {
                    customerShipment.ShipToAddress = customerShipment.ShipToParty.ShippingAddress;
                }

                if (!customerShipment.ExistShipFromAddress)
                {
                    customerShipment.ShipFromAddress = customerShipment.ShipFromParty?.ShippingAddress;
                }

                if (!customerShipment.ExistShipFromFacility)
                {
                    customerShipment.ShipFromFacility = ((Organisation)customerShipment.ShipFromParty)?.FacilitiesWhereOwner.FirstOrDefault();
                }

                //BaseOnDeriveShipmentValue
                var shipmentValue = 0M;
                foreach (ShipmentItem shipmentItem in customerShipment.ShipmentItems)
                {
                    foreach (OrderShipment orderShipment in shipmentItem.OrderShipmentsWhereShipmentItem)
                    {
                        shipmentValue += orderShipment.Quantity * orderShipment.OrderItem.UnitPrice;
                    }
                }

                customerShipment.ShipmentValue = shipmentValue;

                //BaseOnDeriveCurrentShipmentState

                if (customerShipment.ExistShipToParty && customerShipment.ExistShipmentItems)
                {
                    ////cancel shipment if nothing left to ship
                    if (customerShipment.ExistShipmentItems && customerShipment.PendingPickList == null
                        && !customerShipment.ShipmentState.Equals(new ShipmentStates(customerShipment.Strategy.Session).Cancelled))
                    {
                        var canCancel = true;
                        foreach (ShipmentItem shipmentItem in customerShipment.ShipmentItems)
                        {
                            if (shipmentItem.Quantity > 0)
                            {
                                canCancel = false;
                                break;
                            }
                        }

                        if (canCancel)
                        {
                            customerShipment.Cancel();
                        }
                    }

                    var packed = customerShipment.ShipmentState.Equals(new ShipmentStates(customerShipment.Strategy.Session).Packed);
                    var picked = customerShipment.ShipmentState.Equals(new ShipmentStates(customerShipment.Strategy.Session).Picked);

                    if ((customerShipment.ShipmentState.Equals(new ShipmentStates(customerShipment.Strategy.Session).Picking) ||
                         picked ||
                         packed) &&
                         customerShipment.ShipToParty.ExistPickListsWhereShipToParty)
                    {
                        var isPicked = true;
                        foreach (PickList pickList in customerShipment.ShipToParty.PickListsWhereShipToParty)
                        {
                            if (customerShipment.Store.Equals(pickList.Store) &&
                                !pickList.PickListState.Equals(new PickListStates(customerShipment.Strategy.Session).Picked))
                            {
                                isPicked = false;
                            }
                        }

                        // TODO: Review martien

                        if (isPicked && !packed)
                        {
                            customerShipment.SetPicked();
                        }
                    }

                    if (customerShipment.ShipmentState.Equals(new ShipmentStates(customerShipment.Strategy.Session).Picked)
                        || customerShipment.ShipmentState.Equals(new ShipmentStates(customerShipment.Strategy.Session).Packed))
                    {
                        var totalShippingQuantity = 0M;
                        var totalPackagedQuantity = 0M;
                        foreach (ShipmentItem shipmentItem in customerShipment.ShipmentItems)
                        {
                            totalShippingQuantity += shipmentItem.Quantity;
                            foreach (PackagingContent packagingContent in shipmentItem.PackagingContentsWhereShipmentItem)
                            {
                                totalPackagedQuantity += packagingContent.Quantity;
                            }
                        }

                        if (customerShipment.Store.IsImmediatelyPacked && totalPackagedQuantity == totalShippingQuantity)
                        {
                            customerShipment.SetPacked();
                        }
                    }

                    if (customerShipment.ShipmentState.Equals(new ShipmentStates(customerShipment.Strategy.Session).Created)
                        || customerShipment.ShipmentState.Equals(new ShipmentStates(customerShipment.Strategy.Session).Picked)
                        || customerShipment.ShipmentState.Equals(new ShipmentStates(customerShipment.Strategy.Session).Picked)
                        || customerShipment.ShipmentState.Equals(new ShipmentStates(customerShipment.Strategy.Session).Packed))
                    {
                        if (customerShipment.ShipmentValue < customerShipment.Store.ShipmentThreshold && !customerShipment.ReleasedManually)
                        {
                            customerShipment.PutOnHold();
                        }
                    }

                    if (customerShipment.ShipmentState.Equals(new ShipmentStates(customerShipment.Strategy.Session).OnHold) &&
                        !customerShipment.HeldManually &&
                        ((customerShipment.ShipmentValue >= customerShipment.Store.ShipmentThreshold) || customerShipment.ReleasedManually))
                    {
                        customerShipment.Continue();
                    }
                }

                //

                if (customerShipment.CanShip && customerShipment.Store.IsAutomaticallyShipped)
                {
                    customerShipment.Ship();
                }

                //BaseOnDeriveCurrentObjectState

                if (customerShipment.ExistShipmentState && !customerShipment.ShipmentState.Equals(customerShipment.LastShipmentState) &&
                    customerShipment.ShipmentState.Equals(new ShipmentStates(customerShipment.Strategy.Session).Shipped))
                {
                    if (Equals(customerShipment.Store.BillingProcess, new BillingProcesses(customerShipment.Strategy.Session).BillingForShipmentItems))
                    {
                        customerShipment.Invoice();
                    }
                }

                //

                if (customerShipment.ShipmentState.IsShipped
                    && (!customerShipment.ExistLastShipmentState || !customerShipment.LastShipmentState.IsShipped))
                {
                    foreach (var item in customerShipment.ShipmentItems.Where(v => v.ExistSerialisedItem))
                    {
                        if (item.ExistNextSerialisedItemAvailability)
                        {
                            item.SerialisedItem.SerialisedItemAvailability = item.NextSerialisedItemAvailability;

                            if ((customerShipment.ShipFromParty as InternalOrganisation)?.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(customerShipment.Session()).CustomerShipmentShip) == true
                                && item.NextSerialisedItemAvailability.Equals(new SerialisedItemAvailabilities(customerShipment.Session()).Sold))
                            {
                                item.SerialisedItem.OwnedBy = customerShipment.ShipToParty;
                                item.SerialisedItem.Ownership = new Ownerships(customerShipment.Session()).ThirdParty;
                            }
                        }

                        item.SerialisedItem.AvailableForSale = false;
                    }
                }

                Sync(customerShipment);
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

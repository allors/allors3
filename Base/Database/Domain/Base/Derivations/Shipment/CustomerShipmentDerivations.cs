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

    public static partial class DabaseExtensions
    {
        public class CustomerShipmentCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdCustomerShipments = changeSet.Created.Select(session.Instantiate).OfType<CustomerShipment>();

                foreach(var customerShipment in createdCustomerShipments)
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

                    validation.AssertExists(customerShipment, customerShipment.Meta.ShipToParty);

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

                    if (customerShipment.CanShip && customerShipment.Store.IsAutomaticallyShipped)
                    {
                        customerShipment.Ship();
                    }

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

        public static void CustomerShipmentRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("cf865955-b326-4b8f-acba-88423c7bcbfa")] = new CustomerShipmentCreationDerivation();
        }
    }
}

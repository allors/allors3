// <copyright file="PartyExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

    public static class PartyExtensions
    {
        public static void AppsOnBuild(this Party @this, ObjectOnBuild method)
        {
            var transaction = @this.Strategy.Transaction;

            if (!@this.ExistPreferredCurrency)
            {
                var singleton = transaction.GetSingleton();
                @this.PreferredCurrency = singleton.Settings.PreferredCurrency;
            }
        }

        public static bool AppsIsActiveCustomer(this Party @this, InternalOrganisation internalOrganisation, DateTime? date)
        {
            if (date == DateTime.MinValue || internalOrganisation == null)
            {
                return false;
            }

            var customerRelationships = @this.CustomerRelationshipsWhereCustomer.Where(v => Equals(internalOrganisation, v.InternalOrganisation));
            return customerRelationships.Any(relationship => relationship.FromDate.Date <= date
                                                             && (!relationship.ExistThroughDate || relationship.ThroughDate >= date));
        }

        public static CustomerShipment AppsGetPendingCustomerShipmentForStore(this Party @this, PostalAddress address, Store store, ShipmentMethod shipmentMethod)
        {
            var m = @this.Strategy.Transaction.Database.Services().M;

            var shipments = @this.ShipmentsWhereShipToParty.OfType<CustomerShipment>();
            if (address != null)
            {
                shipments = shipments.Where(v => Equals(address, v.ShipToAddress));
            }

            if (store != null)
            {
                shipments = shipments.Where(v => Equals(store, v.Store));
            }

            if (shipmentMethod != null)
            {
                shipments = shipments.Where(v => Equals(shipmentMethod, v.ShipmentMethod));
            }

            foreach (var shipment in shipments)
            {
                if (shipment.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Transaction).Created) ||
                    shipment.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Transaction).Picking) ||
                    shipment.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Transaction).Picked) ||
                    shipment.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Transaction).OnHold) ||
                    shipment.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Transaction).Packed))
                {
                    return shipment;
                }
            }

            return null;
        }

        public static void DeriveRelationships(this Party @this)
        {
            var now = @this.Transaction().Now();

            @this.CurrentPartyContactMechanisms = @this.PartyContactMechanisms
                .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                .ToArray();

            @this.InactivePartyContactMechanisms = @this.PartyContactMechanisms
                .Except(@this.CurrentPartyContactMechanisms)
                .ToArray();

            var allPartyRelationshipsWhereParty = @this.PartyRelationshipsWhereParty;

            @this.CurrentPartyRelationships = allPartyRelationshipsWhereParty
                .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                .ToArray();

            @this.InactivePartyRelationships = allPartyRelationshipsWhereParty
                .Except(@this.CurrentPartyRelationships)
                .ToArray();
        }
    }
}

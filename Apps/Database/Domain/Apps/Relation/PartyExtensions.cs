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
            var session = @this.Strategy.Session;

            if (!@this.ExistPreferredCurrency)
            {
                var singleton = session.GetSingleton();
                @this.PreferredCurrency = singleton.Settings.PreferredCurrency;
            }
        }

        public static bool AppsIsActiveCustomer(this Party @this, InternalOrganisation internalOrganisation, DateTime? date)
        {
            if (date == DateTime.MinValue || internalOrganisation == null)
            {
                return false;
            }

            var m = @this.Strategy.Session.Database.State().M;

            var customerRelationships = @this.CustomerRelationshipsWhereCustomer;
            customerRelationships.Filter.AddEquals(m.CustomerRelationship.InternalOrganisation, internalOrganisation);

            return customerRelationships.Any(relationship => relationship.FromDate.Date <= date
                                                             && (!relationship.ExistThroughDate || relationship.ThroughDate >= date));
        }

        public static CustomerShipment AppsGetPendingCustomerShipmentForStore(this Party @this, PostalAddress address, Store store, ShipmentMethod shipmentMethod)
        {
            var m = @this.Strategy.Session.Database.State().M;

            var shipments = @this.ShipmentsWhereShipToParty;
            if (address != null)
            {
                shipments.Filter.AddEquals(m.Shipment.ShipToAddress, address);
            }

            if (store != null)
            {
                shipments.Filter.AddEquals(m.Shipment.Store, store);
            }

            if (shipmentMethod != null)
            {
                shipments.Filter.AddEquals(m.Shipment.ShipmentMethod, shipmentMethod);
            }

            foreach (CustomerShipment shipment in shipments)
            {
                if (shipment.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Created) ||
                    shipment.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Picking) ||
                    shipment.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Picked) ||
                    shipment.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).OnHold) ||
                    shipment.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Session).Packed))
                {
                    return shipment;
                }
            }

            return null;
        }
    }
}

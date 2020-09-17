// <copyright file="PartyExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Meta;

    public static class PartyExtensions
    {
        public static void BaseOnBuild(this Party @this, ObjectOnBuild method)
        {
            var session = @this.Strategy.Session;

            if (!@this.ExistPreferredCurrency)
            {
                var singleton = session.GetSingleton();
                @this.PreferredCurrency = singleton.Settings.PreferredCurrency;
            }
        }

        public static void BaseOnPreDerive(this Party @this, ObjectOnPreDerive method)
        {
            //var (iteration, changeSet, derivedObjects) = method;

            //if (iteration.IsMarked(@this) || changeSet.IsCreated(@this) || changeSet.HasChangedRoles(@this))
            //{
            //    foreach (PartyFinancialRelationship partyFinancialRelationship in @this.PartyFinancialRelationshipsWhereFinancialParty)
            //    {
            //        iteration.AddDependency(partyFinancialRelationship, @this);
            //        iteration.Mark(partyFinancialRelationship);
            //    }
            //}
        }

        public static void BaseOnDerive(this Party @this, ObjectOnDerive method)
        {
            //@this.BillingAddress = null;
            //@this.BillingInquiriesFax = null;
            //@this.BillingInquiriesPhone = null;
            //@this.CellPhoneNumber = null;
            //@this.GeneralCorrespondence = null;
            //@this.GeneralFaxNumber = null;
            //@this.GeneralPhoneNumber = null;
            //@this.HeadQuarter = null;
            //@this.HomeAddress = null;
            //@this.InternetAddress = null;
            //@this.OrderAddress = null;
            //@this.OrderInquiriesFax = null;
            //@this.OrderInquiriesPhone = null;
            //@this.PersonalEmailAddress = null;
            //@this.SalesOffice = null;
            //@this.ShippingAddress = null;
            //@this.ShippingInquiriesFax = null;
            //@this.ShippingAddress = null;

            //foreach (PartyContactMechanism partyContactMechanism in @this.PartyContactMechanisms)
            //{
            //    if (partyContactMechanism.UseAsDefault)
            //    {
            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).BillingAddress))
            //        {
            //            @this.BillingAddress = partyContactMechanism.ContactMechanism;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).BillingInquiriesFax))
            //        {
            //            @this.BillingInquiriesFax = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).BillingInquiriesPhone))
            //        {
            //            @this.BillingInquiriesPhone = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).MobilePhoneNumber))
            //        {
            //            @this.CellPhoneNumber = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).GeneralCorrespondence))
            //        {
            //            @this.GeneralCorrespondence = partyContactMechanism.ContactMechanism;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).GeneralEmail))
            //        {
            //            @this.GeneralEmail = partyContactMechanism.ContactMechanism as EmailAddress;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).GeneralFaxNumber))
            //        {
            //            @this.GeneralFaxNumber = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).GeneralPhoneNumber))
            //        {
            //            @this.GeneralPhoneNumber = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).HeadQuarters))
            //        {
            //            @this.HeadQuarter = partyContactMechanism.ContactMechanism;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).HomeAddress))
            //        {
            //            @this.HomeAddress = partyContactMechanism.ContactMechanism;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).InternetAddress))
            //        {
            //            @this.InternetAddress = partyContactMechanism.ContactMechanism as ElectronicAddress;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).OrderAddress))
            //        {
            //            @this.OrderAddress = partyContactMechanism.ContactMechanism;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).OrderInquiriesFax))
            //        {
            //            @this.OrderInquiriesFax = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).OrderInquiriesPhone))
            //        {
            //            @this.OrderInquiriesPhone = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).PersonalEmailAddress))
            //        {
            //            @this.PersonalEmailAddress = partyContactMechanism.ContactMechanism as EmailAddress;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).SalesOffice))
            //        {
            //            @this.SalesOffice = partyContactMechanism.ContactMechanism;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).ShippingAddress))
            //        {
            //            @this.ShippingAddress = partyContactMechanism.ContactMechanism as PostalAddress;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).ShippingInquiriesFax))
            //        {
            //            @this.ShippingInquiriesFax = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
            //        }

            //        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).ShippingInquiriesPhone))
            //        {
            //            @this.ShippingInquiriesPhone = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
            //        }
            //    }

            //    Fallback
            //    if (!@this.ExistBillingAddress && @this.ExistGeneralCorrespondence)
            //    {
            //        @this.BillingAddress = @this.GeneralCorrespondence;
            //    }

            //    Fallback
            //    if (!@this.ExistShippingAddress && @this.GeneralCorrespondence is PostalAddress postalAddress)
            //    {
            //        @this.ShippingAddress = postalAddress;
            //    }
            //}

            //@this.CurrentPartyContactMechanisms = @this.PartyContactMechanisms
            //    .Where(v => v.FromDate <= @this.Strategy.Session.Now() && (!v.ExistThroughDate || v.ThroughDate >= @this.Strategy.Session.Now()))
            //    .ToArray();

            //@this.InactivePartyContactMechanisms = @this.PartyContactMechanisms
            //    .Except(@this.CurrentPartyContactMechanisms)
            //    .ToArray();

            //var allPartyRelationshipsWhereParty = @this.PartyRelationshipsWhereParty;

            //@this.CurrentPartyRelationships = allPartyRelationshipsWhereParty
            //    .Where(v => v.FromDate <= @this.Strategy.Session.Now() && (!v.ExistThroughDate || v.ThroughDate >= @this.Strategy.Session.Now()))
            //    .ToArray();

            //@this.InactivePartyRelationships = allPartyRelationshipsWhereParty
            //    .Except(@this.CurrentPartyRelationships)
            //    .ToArray();
        }

        public static void BaseOnPostDerive(this Party @this, ObjectOnPostDerive method)
        {
            //var derivation = method.Derivation;

            //@this.BaseOnDerivePartyFinancialRelationships(derivation);
        }

        public static bool BaseIsActiveCustomer(this Party @this, InternalOrganisation internalOrganisation, DateTime? date)
        {
            if (date == DateTime.MinValue || internalOrganisation == null)
            {
                return false;
            }

            var customerRelationships = @this.CustomerRelationshipsWhereCustomer;
            customerRelationships.Filter.AddEquals(M.CustomerRelationship.InternalOrganisation, internalOrganisation);

            return customerRelationships.Any(relationship => relationship.FromDate.Date <= date
                                                             && (!relationship.ExistThroughDate || relationship.ThroughDate >= date));
        }

        public static CustomerShipment BaseGetPendingCustomerShipmentForStore(this Party @this, PostalAddress address, Store store, ShipmentMethod shipmentMethod)
        {
            var shipments = @this.ShipmentsWhereShipToParty;
            if (address != null)
            {
                shipments.Filter.AddEquals(M.Shipment.ShipToAddress, address);
            }

            if (store != null)
            {
                shipments.Filter.AddEquals(M.Shipment.Store, store);
            }

            if (shipmentMethod != null)
            {
                shipments.Filter.AddEquals(M.Shipment.ShipmentMethod, shipmentMethod);
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

        public static void BaseOnDerivePartyFinancialRelationships(this Party @this, IDerivation derivation)
        {
            //var internalOrganisations = new Organisations(@this.Strategy.Session).InternalOrganisations();

            //if (!internalOrganisations.Contains(@this))
            //{
            //    foreach (var internalOrganisation in internalOrganisations)
            //    {
            //        var partyFinancial = @this.PartyFinancialRelationshipsWhereFinancialParty.FirstOrDefault(v => Equals(v.InternalOrganisation, internalOrganisation));

            //        if (partyFinancial == null)
            //        {
            //            partyFinancial = new PartyFinancialRelationshipBuilder(@this.Strategy.Session)
            //                .WithFinancialParty(@this)
            //                .WithInternalOrganisation(internalOrganisation)
            //                .Build();
            //        }

            //        if (partyFinancial.SubAccountNumber == 0)
            //        {
            //            partyFinancial.SubAccountNumber = internalOrganisation.NextSubAccountNumber();
            //        }
            //    }
            //}
        }
    }
}

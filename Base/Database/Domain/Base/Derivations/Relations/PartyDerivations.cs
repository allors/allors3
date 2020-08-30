// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class PartyDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdParties = changeSet.Created.Select(v=>v.GetObject()).OfType<Party>();

                foreach (var party in createdParties)
                {
                    party.BillingAddress = null;
                    party.BillingInquiriesFax = null;
                    party.BillingInquiriesPhone = null;
                    party.CellPhoneNumber = null;
                    party.GeneralCorrespondence = null;
                    party.GeneralFaxNumber = null;
                    party.GeneralPhoneNumber = null;
                    party.HeadQuarter = null;
                    party.HomeAddress = null;
                    party.InternetAddress = null;
                    party.OrderAddress = null;
                    party.OrderInquiriesFax = null;
                    party.OrderInquiriesPhone = null;
                    party.PersonalEmailAddress = null;
                    party.SalesOffice = null;
                    party.ShippingAddress = null;
                    party.ShippingInquiriesFax = null;
                    party.ShippingAddress = null;

                    foreach (PartyContactMechanism partyContactMechanism in party.PartyContactMechanisms)
                    {
                        if (partyContactMechanism.UseAsDefault)
                        {
                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).BillingAddress))
                            {
                                party.BillingAddress = partyContactMechanism.ContactMechanism;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).BillingInquiriesFax))
                            {
                                party.BillingInquiriesFax = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).BillingInquiriesPhone))
                            {
                                party.BillingInquiriesPhone = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).MobilePhoneNumber))
                            {
                                party.CellPhoneNumber = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).GeneralCorrespondence))
                            {
                                party.GeneralCorrespondence = partyContactMechanism.ContactMechanism;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).GeneralEmail))
                            {
                                party.GeneralEmail = partyContactMechanism.ContactMechanism as EmailAddress;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).GeneralFaxNumber))
                            {
                                party.GeneralFaxNumber = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).GeneralPhoneNumber))
                            {
                                party.GeneralPhoneNumber = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).HeadQuarters))
                            {
                                party.HeadQuarter = partyContactMechanism.ContactMechanism;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).HomeAddress))
                            {
                                party.HomeAddress = partyContactMechanism.ContactMechanism;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).InternetAddress))
                            {
                                party.InternetAddress = partyContactMechanism.ContactMechanism as ElectronicAddress;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).OrderAddress))
                            {
                                party.OrderAddress = partyContactMechanism.ContactMechanism;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).OrderInquiriesFax))
                            {
                                party.OrderInquiriesFax = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).OrderInquiriesPhone))
                            {
                                party.OrderInquiriesPhone = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).PersonalEmailAddress))
                            {
                                party.PersonalEmailAddress = partyContactMechanism.ContactMechanism as EmailAddress;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).SalesOffice))
                            {
                                party.SalesOffice = partyContactMechanism.ContactMechanism;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).ShippingAddress))
                            {
                                party.ShippingAddress = partyContactMechanism.ContactMechanism as PostalAddress;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).ShippingInquiriesFax))
                            {
                                party.ShippingInquiriesFax = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).ShippingInquiriesPhone))
                            {
                                party.ShippingInquiriesPhone = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }
                        }

                        // Fallback
                        if (!party.ExistBillingAddress && party.ExistGeneralCorrespondence)
                        {
                            party.BillingAddress = party.GeneralCorrespondence;
                        }

                        // Fallback
                        if (!party.ExistShippingAddress && party.GeneralCorrespondence is PostalAddress postalAddress)
                        {
                            party.ShippingAddress = postalAddress;
                        }
                    }

                    SyncPartyRelationships(party);
                }
            }

            public static void SyncPartyRelationships(Party party)
            {
                party.CurrentPartyContactMechanisms = party.PartyContactMechanisms
                                        .Where(v => v.FromDate <= party.Strategy.Session.Now() && (!v.ExistThroughDate || v.ThroughDate >= party.Strategy.Session.Now()))
                                        .ToArray();

                party.InactivePartyContactMechanisms = party.PartyContactMechanisms
                    .Except(party.CurrentPartyContactMechanisms)
                    .ToArray();

                var allPartyRelationshipsWhereParty = party.PartyRelationshipsWhereParty;

                party.CurrentPartyRelationships = allPartyRelationshipsWhereParty
                    .Where(v => v.FromDate <= party.Strategy.Session.Now() && (!v.ExistThroughDate || v.ThroughDate >= party.Strategy.Session.Now()))
                    .ToArray();

                party.InactivePartyRelationships = allPartyRelationshipsWhereParty
                    .Except(party.CurrentPartyRelationships)
                    .ToArray();
            }
        }

        public class CustomerRelationshipSyncDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdCustomerRelationships = changeSet.Created.Select(v=>v.GetObject()).OfType<CustomerRelationship>();

                foreach (var relationship in createdCustomerRelationships)
                {
                    // TODO: samenvoegen
                    PartyDerivation.SyncPartyRelationships(relationship.Customer);
                }
            }
        }

        public class DateCheckDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                changeSet.AssociationsByRoleType.TryGetValue(M.PartyContactMechanism.ThroughDate.RoleType, out var changedPartyContactMechanisms);
                var partyContactMechansmWhereFromDateChanged = changedPartyContactMechanisms?.Select(session.Instantiate).OfType<PartyContactMechanism>();

                if (partyContactMechansmWhereFromDateChanged?.Any() == true)
                {
                    foreach (var partyContactMechanism in partyContactMechansmWhereFromDateChanged)
                    {
                        if (partyContactMechanism.PartyWherePartyContactMechanism is Organisation organisation)
                        {
                            PersonSyncDerivation.SyncContactPartyContactMechanism(organisation);
                        }
                    }
                }
            }
        }

        public static void PartyRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("55ea0c36-c3b2-4fcd-84ba-60bad8d92bfa")] = new DateCheckDerivation();
            @this.DomainDerivationById[new Guid("1511398e-b3ff-4133-8ab0-e20a362116c7")] = new PartyDerivation();
            @this.DomainDerivationById[new Guid("c02399c5-fea4-472a-95e4-322fa0dd4a75")] = new CustomerRelationshipSyncDerivation();
        }
    }
}

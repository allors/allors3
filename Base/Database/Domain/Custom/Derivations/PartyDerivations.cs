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
                var createdParties = changeSet.Created.Select(session.Instantiate).OfType<Party>();

                foreach (var party in createdParties)
                {
                    party.DerivedRoles.BillingAddress = null;
                    party.DerivedRoles.BillingInquiriesFax = null;
                    party.DerivedRoles.BillingInquiriesPhone = null;
                    party.DerivedRoles.CellPhoneNumber = null;
                    party.DerivedRoles.GeneralCorrespondence = null;
                    party.DerivedRoles.GeneralFaxNumber = null;
                    party.DerivedRoles.GeneralPhoneNumber = null;
                    party.DerivedRoles.HeadQuarter = null;
                    party.DerivedRoles.HomeAddress = null;
                    party.DerivedRoles.InternetAddress = null;
                    party.DerivedRoles.OrderAddress = null;
                    party.DerivedRoles.OrderInquiriesFax = null;
                    party.DerivedRoles.OrderInquiriesPhone = null;
                    party.DerivedRoles.PersonalEmailAddress = null;
                    party.DerivedRoles.SalesOffice = null;
                    party.DerivedRoles.ShippingAddress = null;
                    party.DerivedRoles.ShippingInquiriesFax = null;
                    party.DerivedRoles.ShippingAddress = null;

                    foreach (PartyContactMechanism partyContactMechanism in party.PartyContactMechanisms)
                    {
                        if (partyContactMechanism.UseAsDefault)
                        {
                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).BillingAddress))
                            {
                                party.DerivedRoles.BillingAddress = partyContactMechanism.ContactMechanism;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).BillingInquiriesFax))
                            {
                                party.DerivedRoles.BillingInquiriesFax = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).BillingInquiriesPhone))
                            {
                                party.DerivedRoles.BillingInquiriesPhone = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).MobilePhoneNumber))
                            {
                                party.DerivedRoles.CellPhoneNumber = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).GeneralCorrespondence))
                            {
                                party.DerivedRoles.GeneralCorrespondence = partyContactMechanism.ContactMechanism;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).GeneralEmail))
                            {
                                party.DerivedRoles.GeneralEmail = partyContactMechanism.ContactMechanism as EmailAddress;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).GeneralFaxNumber))
                            {
                                party.DerivedRoles.GeneralFaxNumber = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).GeneralPhoneNumber))
                            {
                                party.DerivedRoles.GeneralPhoneNumber = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).HeadQuarters))
                            {
                                party.DerivedRoles.HeadQuarter = partyContactMechanism.ContactMechanism;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).HomeAddress))
                            {
                                party.DerivedRoles.HomeAddress = partyContactMechanism.ContactMechanism;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).InternetAddress))
                            {
                                party.DerivedRoles.InternetAddress = partyContactMechanism.ContactMechanism as ElectronicAddress;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).OrderAddress))
                            {
                                party.DerivedRoles.OrderAddress = partyContactMechanism.ContactMechanism;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).OrderInquiriesFax))
                            {
                                party.DerivedRoles.OrderInquiriesFax = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).OrderInquiriesPhone))
                            {
                                party.DerivedRoles.OrderInquiriesPhone = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).PersonalEmailAddress))
                            {
                                party.DerivedRoles.PersonalEmailAddress = partyContactMechanism.ContactMechanism as EmailAddress;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).SalesOffice))
                            {
                                party.DerivedRoles.SalesOffice = partyContactMechanism.ContactMechanism;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).ShippingAddress))
                            {
                                party.DerivedRoles.ShippingAddress = partyContactMechanism.ContactMechanism as PostalAddress;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).ShippingInquiriesFax))
                            {
                                party.DerivedRoles.ShippingInquiriesFax = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }

                            if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(party.Strategy.Session).ShippingInquiriesPhone))
                            {
                                party.DerivedRoles.ShippingInquiriesPhone = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                            }
                        }

                        // Fallback
                        if (!party.ExistBillingAddress && party.ExistGeneralCorrespondence)
                        {
                            party.DerivedRoles.BillingAddress = party.GeneralCorrespondence;
                        }

                        // Fallback
                        if (!party.ExistShippingAddress && party.GeneralCorrespondence is PostalAddress postalAddress)
                        {
                            party.DerivedRoles.ShippingAddress = postalAddress;
                        }
                    }

                    SyncPartyRelationships(party);
                }
            }

            public static void SyncPartyRelationships(Party party)
            {
                party.DerivedRoles.CurrentPartyContactMechanisms = party.PartyContactMechanisms
                                        .Where(v => v.FromDate <= party.Strategy.Session.Now() && (!v.ExistThroughDate || v.ThroughDate >= party.Strategy.Session.Now()))
                                        .ToArray();

                party.DerivedRoles.InactivePartyContactMechanisms = party.PartyContactMechanisms
                    .Except(party.CurrentPartyContactMechanisms)
                    .ToArray();

                var allPartyRelationshipsWhereParty = party.PartyRelationshipsWhereParty;

                party.DerivedRoles.CurrentPartyRelationships = allPartyRelationshipsWhereParty
                    .Where(v => v.FromDate <= party.Strategy.Session.Now() && (!v.ExistThroughDate || v.ThroughDate >= party.Strategy.Session.Now()))
                    .ToArray();

                party.DerivedRoles.InactivePartyRelationships = allPartyRelationshipsWhereParty
                    .Except(party.CurrentPartyRelationships)
                    .ToArray();
            }
        }

        public class CustomerRelationshipSyncDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdCustomerRelationships = changeSet.Created.Select(session.Instantiate).OfType<CustomerRelationship>();

                foreach (var relationship in createdCustomerRelationships)
                {
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

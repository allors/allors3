// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class PartyDerivation : DomainDerivation
    {
        public PartyDerivation(M m) : base(m, new Guid("C57CD79C-F75E-4282-BFAD-B2F5F54FD4A4")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.Party.Interface),
                new CreatedPattern(this.M.PartyContactMechanism.Class) {Steps = new IPropertyType[] { this.M.PartyContactMechanism.PartyWherePartyContactMechanism } },
                new CreatedPattern(this.M.CustomerRelationship.Class) {Steps = new IPropertyType[] { this.M.CustomerRelationship.Customer } },
                new ChangedPattern(this.M.PartyContactMechanism.ThroughDate) {Steps = new IPropertyType[] { this.M.PartyContactMechanism.PartyWherePartyContactMechanism } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var party in matches.Cast<Party>())
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

                // SyncPartyRelationships
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

                var internalOrganisations = new Organisations(party.Strategy.Session).InternalOrganisations();

                if (!internalOrganisations.Contains(party))
                {
                    foreach (var internalOrganisation in internalOrganisations)
                    {
                        var partyFinancial = party.PartyFinancialRelationshipsWhereFinancialParty.FirstOrDefault(v => Equals(v.InternalOrganisation, internalOrganisation));

                        if (partyFinancial == null)
                        {
                            partyFinancial = new PartyFinancialRelationshipBuilder(party.Strategy.Session)
                                .WithFinancialParty(party)
                                .WithInternalOrganisation(internalOrganisation)
                                .Build();
                        }

                        if (partyFinancial.SubAccountNumber == 0)
                        {
                            partyFinancial.SubAccountNumber = internalOrganisation.NextSubAccountNumber();
                        }
                    }
                }

                if (party is Organisation organisation)
                {
                    // TODO: Martien
                    //PersonDerivation.SyncContactPartyContactMechanism(organisation);
                }
            }
        }
    }
}

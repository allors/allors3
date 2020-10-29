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
            foreach (var @this in matches.Cast<Party>())
            {
                @this.BillingAddress = null;
                @this.BillingInquiriesFax = null;
                @this.BillingInquiriesPhone = null;
                @this.CellPhoneNumber = null;
                @this.GeneralCorrespondence = null;
                @this.GeneralFaxNumber = null;
                @this.GeneralPhoneNumber = null;
                @this.HeadQuarter = null;
                @this.HomeAddress = null;
                @this.InternetAddress = null;
                @this.OrderAddress = null;
                @this.OrderInquiriesFax = null;
                @this.OrderInquiriesPhone = null;
                @this.PersonalEmailAddress = null;
                @this.SalesOffice = null;
                @this.ShippingAddress = null;
                @this.ShippingInquiriesFax = null;
                @this.ShippingAddress = null;

                foreach (PartyContactMechanism partyContactMechanism in @this.PartyContactMechanisms)
                {
                    if (partyContactMechanism.UseAsDefault)
                    {
                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).BillingAddress))
                        {
                            @this.BillingAddress = partyContactMechanism.ContactMechanism;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).BillingInquiriesFax))
                        {
                            @this.BillingInquiriesFax = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).BillingInquiriesPhone))
                        {
                            @this.BillingInquiriesPhone = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).MobilePhoneNumber))
                        {
                            @this.CellPhoneNumber = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).GeneralCorrespondence))
                        {
                            @this.GeneralCorrespondence = partyContactMechanism.ContactMechanism;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).GeneralEmail))
                        {
                            @this.GeneralEmail = partyContactMechanism.ContactMechanism as EmailAddress;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).GeneralFaxNumber))
                        {
                            @this.GeneralFaxNumber = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).GeneralPhoneNumber))
                        {
                            @this.GeneralPhoneNumber = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).HeadQuarters))
                        {
                            @this.HeadQuarter = partyContactMechanism.ContactMechanism;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).HomeAddress))
                        {
                            @this.HomeAddress = partyContactMechanism.ContactMechanism;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).InternetAddress))
                        {
                            @this.InternetAddress = partyContactMechanism.ContactMechanism as ElectronicAddress;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).OrderAddress))
                        {
                            @this.OrderAddress = partyContactMechanism.ContactMechanism;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).OrderInquiriesFax))
                        {
                            @this.OrderInquiriesFax = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).OrderInquiriesPhone))
                        {
                            @this.OrderInquiriesPhone = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).PersonalEmailAddress))
                        {
                            @this.PersonalEmailAddress = partyContactMechanism.ContactMechanism as EmailAddress;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).SalesOffice))
                        {
                            @this.SalesOffice = partyContactMechanism.ContactMechanism;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).ShippingAddress))
                        {
                            @this.ShippingAddress = partyContactMechanism.ContactMechanism as PostalAddress;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).ShippingInquiriesFax))
                        {
                            @this.ShippingInquiriesFax = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Session).ShippingInquiriesPhone))
                        {
                            @this.ShippingInquiriesPhone = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }
                    }

                    // Fallback
                    if (!@this.ExistBillingAddress && @this.ExistGeneralCorrespondence)
                    {
                        @this.BillingAddress = @this.GeneralCorrespondence;
                    }

                    // Fallback
                    if (!@this.ExistShippingAddress && @this.GeneralCorrespondence is PostalAddress postalAddress)
                    {
                        @this.ShippingAddress = postalAddress;
                    }
                }

                // SyncPartyRelationships
                @this.CurrentPartyContactMechanisms = @this.PartyContactMechanisms
                    .Where(v => v.FromDate <= @this.Strategy.Session.Now() && (!v.ExistThroughDate || v.ThroughDate >= @this.Strategy.Session.Now()))
                    .ToArray();

                @this.InactivePartyContactMechanisms = @this.PartyContactMechanisms
                    .Except(@this.CurrentPartyContactMechanisms)
                    .ToArray();

                var allPartyRelationshipsWhereParty = @this.PartyRelationshipsWhereParty;

                @this.CurrentPartyRelationships = allPartyRelationshipsWhereParty
                    .Where(v => v.FromDate <= @this.Strategy.Session.Now() && (!v.ExistThroughDate || v.ThroughDate >= @this.Strategy.Session.Now()))
                    .ToArray();

                @this.InactivePartyRelationships = allPartyRelationshipsWhereParty
                    .Except(@this.CurrentPartyRelationships)
                    .ToArray();

                var internalOrganisations = new Organisations(@this.Strategy.Session).InternalOrganisations();

                if (!internalOrganisations.Contains(@this))
                {
                    foreach (var internalOrganisation in internalOrganisations)
                    {
                        var partyFinancial = @this.PartyFinancialRelationshipsWhereFinancialParty.FirstOrDefault(v => Equals(v.InternalOrganisation, internalOrganisation));

                        if (partyFinancial == null)
                        {
                            partyFinancial = new PartyFinancialRelationshipBuilder(@this.Strategy.Session)
                                .WithFinancialParty(@this)
                                .WithInternalOrganisation(internalOrganisation)
                                .Build();
                        }

                        if (partyFinancial.SubAccountNumber == 0)
                        {
                            partyFinancial.SubAccountNumber = internalOrganisation.NextSubAccountNumber();
                        }
                    }
                }

                if (@this is Organisation organisation)
                {
                    // TODO: Martien
                    //PersonDerivation.SyncContactPartyContactMechanism(organisation);
                }
            }
        }
    }
}

// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class PartyRule : Rule
    {
        public PartyRule(M m) : base(m, new Guid("C57CD79C-F75E-4282-BFAD-B2F5F54FD4A4")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Party, m.Party.DerivationTrigger),
                new RolePattern(m.Party, m.Party.PartyContactMechanisms),
                new RolePattern(m.PartyContactMechanism, m.PartyContactMechanism.ContactPurposes) {Steps = new IPropertyType[] { this.M.PartyContactMechanism.PartyWherePartyContactMechanism } },
                new RolePattern(m.PartyContactMechanism, m.PartyContactMechanism.FromDate) {Steps = new IPropertyType[]{ m.PartyContactMechanism.PartyWherePartyContactMechanism } },
                new RolePattern(m.PartyContactMechanism, m.PartyContactMechanism.ThroughDate) {Steps = new IPropertyType[]{ m.PartyContactMechanism.PartyWherePartyContactMechanism } },
                new AssociationPattern(m.PartyRelationship.Parties),
                new RolePattern(m.PartyRelationship, m.PartyRelationship.FromDate) {Steps = new IPropertyType[]{ m.PartyRelationship.Parties } },
                new RolePattern(m.PartyRelationship, m.PartyRelationship.ThroughDate) {Steps = new IPropertyType[]{ m.PartyRelationship.Parties } },
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
                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).BillingAddress))
                        {
                            @this.BillingAddress = partyContactMechanism.ContactMechanism;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).BillingInquiriesFax))
                        {
                            @this.BillingInquiriesFax = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).BillingInquiriesPhone))
                        {
                            @this.BillingInquiriesPhone = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).MobilePhoneNumber))
                        {
                            @this.CellPhoneNumber = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).GeneralCorrespondence))
                        {
                            @this.GeneralCorrespondence = partyContactMechanism.ContactMechanism;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).GeneralEmail))
                        {
                            @this.GeneralEmail = partyContactMechanism.ContactMechanism as EmailAddress;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).GeneralFaxNumber))
                        {
                            @this.GeneralFaxNumber = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).GeneralPhoneNumber))
                        {
                            @this.GeneralPhoneNumber = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).HeadQuarters))
                        {
                            @this.HeadQuarter = partyContactMechanism.ContactMechanism;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).HomeAddress))
                        {
                            @this.HomeAddress = partyContactMechanism.ContactMechanism;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).InternetAddress))
                        {
                            @this.InternetAddress = partyContactMechanism.ContactMechanism as ElectronicAddress;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).OrderAddress))
                        {
                            @this.OrderAddress = partyContactMechanism.ContactMechanism;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).OrderInquiriesFax))
                        {
                            @this.OrderInquiriesFax = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).OrderInquiriesPhone))
                        {
                            @this.OrderInquiriesPhone = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).PersonalEmailAddress))
                        {
                            @this.PersonalEmailAddress = partyContactMechanism.ContactMechanism as EmailAddress;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).SalesOffice))
                        {
                            @this.SalesOffice = partyContactMechanism.ContactMechanism;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).ShippingAddress))
                        {
                            @this.ShippingAddress = partyContactMechanism.ContactMechanism as PostalAddress;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).ShippingInquiriesFax))
                        {
                            @this.ShippingInquiriesFax = partyContactMechanism.ContactMechanism as TelecommunicationsNumber;
                        }

                        if (partyContactMechanism.ContactPurposes.Contains(new ContactMechanismPurposes(@this.Strategy.Transaction).ShippingInquiriesPhone))
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

                @this.DeriveRelationships();

                var internalOrganisations = new Organisations(@this.Strategy.Transaction).InternalOrganisations();

                if (!internalOrganisations.Contains(@this))
                {
                    foreach (var internalOrganisation in internalOrganisations)
                    {
                        var partyFinancial = @this.PartyFinancialRelationshipsWhereFinancialParty.FirstOrDefault(v => Equals(v.InternalOrganisation, internalOrganisation));

                        if (partyFinancial == null)
                        {
                            partyFinancial = new PartyFinancialRelationshipBuilder(@this.Strategy.Transaction)
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
            }
        }
    }
}

// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class OrganisationSearchStringRule : Rule
    {
        public OrganisationSearchStringRule(MetaPopulation m) : base(m, new Guid("30b64128-3d1c-4426-8f37-ee32423ed1df")) =>
            this.Patterns = new Pattern[]
            {
                m.Party.RolePattern(v => v.Qualifications, m.Organisation),
                m.Qualification.RolePattern(v => v.Name, v => v.PartiesWhereQualification.Party, m.Organisation),
                m.Party.RolePattern(v => v.PartySkills, m.Organisation),
                m.Skill.RolePattern(v => v.Name, v => v.PartySkillsWhereSkill.PartySkill.PartiesWherePartySkill.Party, m.Organisation),
                m.Party.RolePattern(v => v.PartyClassifications, m.Organisation),
                m.PartyClassification.RolePattern(v => v.Name, v => v.PartiesWherePartyClassification.Party, m.Organisation),
                m.Party.RolePattern(v => v.BankAccounts, m.Organisation),
                m.BankAccount.RolePattern(v => v.Iban, v => v.PartyWhereBankAccount.Party, m.Organisation),
                m.Party.RolePattern(v => v.CreditCards, m.Organisation),
                m.CreditCard.RolePattern(v => v.CardNumber, v => v.PartyWhereCreditCard.Party, m.Organisation),
                m.Party.RolePattern(v => v.DefaultPaymentMethod, m.Organisation),
                m.Party.RolePattern(v => v.DefaultShipmentMethod, m.Organisation),
                m.Party.RolePattern(v => v.PartyContactMechanisms, m.Organisation),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.PartyContactMechanismsWhereContactMechanism.PartyContactMechanism.PartyWherePartyContactMechanism.Party, m.Organisation),
                m.Party.RolePattern(v => v.CurrentPartyRelationships, m.Organisation),
                m.PartyRelationship.RolePattern(v => v.Parties, v => v.PartiesWhereCurrentPartyRelationship.Party, m.Organisation),
                m.Party.RolePattern(v => v.InactivePartyRelationships, m.Organisation),
                m.PartyRelationship.RolePattern(v => v.Parties, v => v.PartiesWhereInactivePartyRelationship.Party, m.Organisation),
                m.Party.RolePattern(v => v.CurrentContacts, m.Organisation),
                m.Person.RolePattern(v => v.DisplayName, v => v.PartiesWhereCurrentContact.Party, m.Organisation),
                m.Party.RolePattern(v => v.InactiveContacts, m.Organisation),
                m.Person.RolePattern(v => v.DisplayName, v => v.PartiesWhereInactiveContact.Party, m.Organisation),

                m.Organisation.RolePattern(v => v.LegalForm),
                m.Organisation.RolePattern(v => v.DisplayName),
                m.Organisation.RolePattern(v => v.TaxNumber),
                m.Organisation.RolePattern(v => v.IndustryClassifications),
                m.IndustryClassification.RolePattern(v => v.Name, v => v.OrganisationsWhereIndustryClassification.Organisation),
                m.Organisation.RolePattern(v => v.CustomClassifications),
                m.CustomOrganisationClassification.RolePattern(v => v.Name, v => v.OrganisationsWhereCustomClassification.Organisation),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Organisation>())
            {
                var array = new string[] {
                    @this.ExistQualifications ? string.Join(" ", @this.Qualifications?.Select(v => v.Name)) : null,
                    @this.ExistPartySkills ? string.Join(" ", @this.PartySkills?.Select(v => v.Skill?.Name)) : null,
                    @this.ExistPartyClassifications ? string.Join(" ", @this.PartyClassifications?.Select(v => v.Name)) : null,
                    @this.ExistBankAccounts ? string.Join(" ", @this.BankAccounts?.Select(v => v.Iban)) : null,
                    @this.ExistCreditCards ? string.Join(" ", @this.CreditCards?.Select(v => v.CardNumber)) : null,
                    @this.DefaultPaymentMethod?.Description,
                    @this.DefaultShipmentMethod?.Name,
                    @this.ExistPartyContactMechanisms ? string.Join(" ", @this.PartyContactMechanisms?.Select(v => v.ContactMechanism?.DisplayName)) : null,
                    @this.ExistCurrentPartyRelationships ? string.Join(" ", @this.CurrentPartyRelationships?.Select(v => v.Parties.Select(v => v.DisplayName))) : null,
                    @this.ExistInactivePartyRelationships ? string.Join(" ", @this.InactivePartyRelationships?.Select(v => v.Parties.Select(v => v.DisplayName))) : null,
                    @this.ExistCurrentContacts ? string.Join(" ", @this.CurrentContacts?.Select(v => v.DisplayName)) : null,
                    @this.ExistInactiveContacts ? string.Join(" ", @this.InactiveContacts?.Select(v => v.DisplayName)) : null,
                    @this.LegalForm?.Description,
                    @this.DisplayName,
                    @this.TaxNumber,
                    @this.ExistIndustryClassifications ? string.Join(" ", @this.IndustryClassifications?.Select(v => v.Name)) : null,
                    @this.ExistCustomClassifications ? string.Join(" ", @this.CustomClassifications?.Select(v => v.Name)) : null,
                };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}

// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class PersonSearchStringRule : Rule
    {
        public PersonSearchStringRule(MetaPopulation m) : base(m, new Guid("b521a757-d5ee-468a-8d0c-e97b6911e324")) =>
            this.Patterns = new Pattern[]
            {
                m.Party.RolePattern(v => v.Qualifications, m.Person),
                m.Qualification.RolePattern(v => v.Name, v => v.PartiesWhereQualification.ObjectType, m.Person),
                m.Party.RolePattern(v => v.PartySkills, m.Person),
                m.Skill.RolePattern(v => v.Name, v => v.PartySkillsWhereSkill.ObjectType.PartiesWherePartySkill.ObjectType, m.Person),
                m.Party.RolePattern(v => v.PartyClassifications, m.Person),
                m.PartyClassification.RolePattern(v => v.Name, v => v.PartiesWherePartyClassification.ObjectType, m.Person),
                m.Party.RolePattern(v => v.BankAccounts, m.Person),
                m.BankAccount.RolePattern(v => v.Iban, v => v.PartyWhereBankAccount.ObjectType, m.Person),
                m.Party.RolePattern(v => v.CreditCards, m.Person),
                m.CreditCard.RolePattern(v => v.CardNumber, v => v.PartyWhereCreditCard.ObjectType, m.Person),
                m.Party.RolePattern(v => v.DefaultPaymentMethod, m.Person),
                m.Party.RolePattern(v => v.DefaultShipmentMethod, m.Person),
                m.Party.AssociationPattern(v => v.PartyContactMechanismsWhereParty, m.Person),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.PartyContactMechanismsWhereContactMechanism.ObjectType.Party.ObjectType, m.Person),
                m.Party.RolePattern(v => v.CurrentPartyRelationships, m.Person),
                m.PartyRelationship.RolePattern(v => v.Parties, v => v.PartiesWhereCurrentPartyRelationship.ObjectType, m.Person),
                m.Party.RolePattern(v => v.InactivePartyRelationships, m.Person),
                m.PartyRelationship.RolePattern(v => v.Parties, v => v.PartiesWhereInactivePartyRelationship.ObjectType, m.Person),
                m.Party.RolePattern(v => v.CurrentContacts, m.Person),
                m.Person.RolePattern(v => v.DisplayName, v => v.PartiesWhereCurrentContact.ObjectType, m.Person),
                m.Party.RolePattern(v => v.InactiveContacts, m.Person),
                m.Person.RolePattern(v => v.DisplayName, v => v.PartiesWhereInactiveContact.ObjectType, m.Person),

                m.Person.RolePattern(v => v.DisplayName),
                m.Person.RolePattern(v => v.FirstName),
                m.Person.RolePattern(v => v.MiddleName),
                m.Person.RolePattern(v => v.LastName),
                m.Person.RolePattern(v => v.GivenName),
                m.Person.RolePattern(v => v.MothersMaidenName),
                m.Person.RolePattern(v => v.Gender),
                m.Person.RolePattern(v => v.MaritalStatus),
                m.Person.RolePattern(v => v.PersonClassifications),
                m.Person.RolePattern(v => v.SocialSecurityNumber),
                m.Person.RolePattern(v => v.Function),
                m.PersonClassification.RolePattern(v => v.Name, v => v.PeopleWherePersonClassification.ObjectType),
                m.Person.RolePattern(v => v.PersonTrainings),
                m.Training.RolePattern(v => v.Description, v => v.PersonTrainingsWhereTraining.ObjectType.PeopleWherePersonTraining.ObjectType),
                m.Person.RolePattern(v => v.Citizenship),
                m.Citizenship.RolePattern(v => v.Passports, v => v.PersonWhereCitizenship.ObjectType),
                m.Citizenship.RolePattern(v => v.Country, v => v.PersonWhereCitizenship.ObjectType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Person>())
            {
                @this.DerivePersonSearchString(validation);
            }
        }
    }

    public static class PersonSearchStringRuleExtensions
    {
        public static void DerivePersonSearchString(this Person @this, IValidation validation)
        {
            var array = new string[] {
                    @this.ExistQualifications ? string.Join(" ", @this.Qualifications?.Select(v => v.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistPartySkills ? string.Join(" ", @this.PartySkills?.Select(v => v.Skill?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistPartyClassifications ? string.Join(" ", @this.PartyClassifications?.Select(v => v.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistBankAccounts ? string.Join(" ", @this.BankAccounts?.Select(v => v.Iban ?? string.Empty).ToArray()) : null,
                    @this.ExistCreditCards ? string.Join(" ", @this.CreditCards?.Select(v => v.CardNumber ?? string.Empty).ToArray()) : null,
                    @this.DefaultPaymentMethod?.Description,
                    @this.DefaultShipmentMethod?.Name,
                    @this.ExistPartyContactMechanismsWhereParty ? string.Join(" ", @this.PartyContactMechanismsWhereParty?.Select(v => v.ContactMechanism?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistCurrentPartyRelationships ? string.Join(" ", @this.CurrentPartyRelationships?.SelectMany(v => v.Parties?.Select(v => v.DisplayName ?? string.Empty)).ToArray()) : null,
                    @this.ExistInactivePartyRelationships ? string.Join(" ", @this.InactivePartyRelationships?.SelectMany(v => v.Parties?.Select(v => v.DisplayName ?? string.Empty)).ToArray()) : null,
                    @this.ExistCurrentContacts ? string.Join(" ", @this.CurrentContacts?.Select(v => v.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistInactiveContacts ? string.Join(" ", @this.InactiveContacts?.Select(v => v.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistPersonClassifications ? string.Join(" ", @this.PersonClassifications?.Select(v => v.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistPersonTrainings ? string.Join(" ", @this.PersonTrainings?.Select(v => v.Training?.Description ?? string.Empty).ToArray()) : null,
                    @this.ExistCitizenship ? string.Join(" ", @this.Citizenship?.Passports?.Select(v => v.Number ?? string.Empty).ToArray()) : null,
                    @this.Citizenship?.Country?.Name,
                    @this.DisplayName,
                    @this.FirstName,
                    @this.MiddleName,
                    @this.LastName,
                    @this.GivenName,
                    @this.MothersMaidenName,
                    @this.Gender?.Name,
                    @this.MaritalStatus?.Name,
                    @this.SocialSecurityNumber,
                    @this.Function,
                    @this.ExistCurrentOrganisationContactRelationships ? string.Join(" ", @this.CurrentOrganisationContactRelationships?.Select(v => v.Organisation?.DisplayName ?? string.Empty).ToArray()) : null,
                };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            else
            {
                @this.RemoveSearchString();
            }
        }
    }
}

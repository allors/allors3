// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Meta;
    using Database.Derivations;

    public class PersonDerivation : DomainDerivation
    {
        public PersonDerivation(M m) : base(m, new Guid("BC3969F4-4050-47A1-B80C-8F23879E3B10")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.Person.DerivationTrigger),
                new AssociationPattern(m.Person.Salutation),
                new AssociationPattern(m.Person.FirstName),
                new AssociationPattern(m.Person.MiddleName),
                new AssociationPattern(m.Person.LastName),
                new AssociationPattern(m.Person.UserName),
                new AssociationPattern(m.OrganisationContactRelationship.Contact) {Steps = new IPropertyType[]{ m.OrganisationContactRelationship.Contact} },
                new AssociationPattern(m.OrganisationContactRelationship.FromDate) {Steps = new IPropertyType[]{ m.OrganisationContactRelationship.Contact } },
                new AssociationPattern(m.OrganisationContactRelationship.ThroughDate) {Steps = new IPropertyType[]{ m.OrganisationContactRelationship.Contact } },
                new AssociationPattern(m.Employment.FromDate) { Steps =  new IPropertyType[] {m.Employment.Employee} },
                new AssociationPattern(m.Employment.ThroughDate) { Steps =  new IPropertyType[] {m.Employment.Employee} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Person>())
            {
                var now = @this.Transaction().Now();

                @this.Strategy.Transaction.Prefetch(@this.PrefetchPolicy);

                if (@this.ExistSalutation
                    && (@this.Salutation.Equals(new Salutations(@this.Transaction()).Mr)
                        || @this.Salutation.Equals(new Salutations(@this.Transaction()).Dr)))
                {
                    @this.Gender = new GenderTypes(@this.Transaction()).Male;
                }

                if (@this.ExistSalutation
                    && (@this.Salutation.Equals(new Salutations(@this.Transaction()).Mrs)
                        || @this.Salutation.Equals(new Salutations(@this.Transaction()).Ms)
                        || @this.Salutation.Equals(new Salutations(@this.Transaction()).Mme)))
                {
                    @this.Gender = new GenderTypes(@this.Transaction()).Female;
                }

                @this.PartyName = DerivePartyName(@this);

                @this.VatRegime = new VatRegimes(@this.Transaction()).PrivatePerson;

                @this.DeriveRelationships();

                if (!@this.ExistTimeSheetWhereWorker && (@this.AppsIsActiveEmployee(now) || @this.CurrentOrganisationContactRelationships.Count > 0))
                {
                    new TimeSheetBuilder(@this.Strategy.Transaction).WithWorker(@this).Build();
                }
            }
        }

        static string DerivePartyName(Person person)
        {
            var partyName = new StringBuilder();

            if (person.ExistFirstName)
            {
                partyName.Append(person.FirstName);
            }

            if (person.ExistMiddleName)
            {
                if (partyName.Length > 0)
                {
                    partyName.Append(" ");
                }

                partyName.Append(person.MiddleName);
            }

            if (person.ExistLastName)
            {
                if (partyName.Length > 0)
                {
                    partyName.Append(" ");
                }

                partyName.Append(person.LastName);
            }

            if (partyName.Length == 0)
            {
                partyName.Append($"[{person.UserName}]");
            }

            return partyName.ToString();
        }
    }
}

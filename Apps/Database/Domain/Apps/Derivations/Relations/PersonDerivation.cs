// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Meta;

    public class PersonDerivation : DomainDerivation
    {
        public PersonDerivation(M m) : base(m, new Guid("BC3969F4-4050-47A1-B80C-8F23879E3B10")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.Person.Salutation),
                new ChangedPattern(this.M.Employment.FromDate) { Steps =  new IPropertyType[] {m.Employment.Employee} },
                new ChangedPattern(this.M.Employment.ThroughDate) { Steps =  new IPropertyType[] {m.Employment.Employee} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Person>())
            {
                var now = @this.Session().Now();

                @this.Strategy.Session.Prefetch(@this.PrefetchPolicy);

                if (@this.ExistSalutation
                    && (@this.Salutation.Equals(new Salutations(@this.Session()).Mr)
                        || @this.Salutation.Equals(new Salutations(@this.Session()).Dr)))
                {
                    @this.Gender = new GenderTypes(@this.Session()).Male;
                }

                if (@this.ExistSalutation
                    && (@this.Salutation.Equals(new Salutations(@this.Session()).Mrs)
                        || @this.Salutation.Equals(new Salutations(@this.Session()).Ms)
                        || @this.Salutation.Equals(new Salutations(@this.Session()).Mme)))
                {
                    @this.Gender = new GenderTypes(@this.Session()).Female;
                }

                @this.PartyName = DerivePartyName(@this);

                @this.VatRegime = new VatRegimes(@this.Session()).PrivatePerson;

                @this.DeriveRelationships();

                if (!@this.ExistTimeSheetWhereWorker && (@this.AppsIsActiveEmployee(now) || @this.CurrentOrganisationContactRelationships.Count > 0))
                {
                    new TimeSheetBuilder(@this.Strategy.Session).WithWorker(@this).Build();
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

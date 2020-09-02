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

    public class PersonDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("BC3969F4-4050-47A1-B80C-8F23879E3B10");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.Person.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var person in matches.Cast<Person>())
            {
                var now = person.Session().Now();

                person.Strategy.Session.Prefetch(person.PrefetchPolicy);

                if (person.ExistSalutation
                    && (person.Salutation.Equals(new Salutations(person.Session()).Mr)
                        || person.Salutation.Equals(new Salutations(person.Session()).Dr)))
                {
                    person.Gender = new GenderTypes(person.Session()).Male;
                }

                if (person.ExistSalutation
                    && (person.Salutation.Equals(new Salutations(person.Session()).Mrs)
                        || person.Salutation.Equals(new Salutations(person.Session()).Ms)
                        || person.Salutation.Equals(new Salutations(person.Session()).Mme)))
                {
                    person.Gender = new GenderTypes(person.Session()).Female;
                }

                person.PartyName = DerivePartyName(person);

                person.VatRegime = new VatRegimes(person.Session()).PrivatePerson;

                person.DeriveRelationships();

                if (!person.ExistTimeSheetWhereWorker && (person.BaseIsActiveEmployee(now) || person.CurrentOrganisationContactRelationships.Count > 0))
                {
                    new TimeSheetBuilder(person.Strategy.Session).WithWorker(person).Build();
                }

                var deletePermission = new Permissions(person.Strategy.Session).Get(person.Meta.ObjectType, person.Meta.Delete, Operations.Execute);
                if (person.IsDeletable)
                {
                    person.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    person.AddDeniedPermission(deletePermission);
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

// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class PersonFullNameDerivation : IDomainDerivation
    {
        public PersonFullNameDerivation(M m) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.Person.FirstName),
                new ChangedPattern(m.Person.LastName),
            };

        public Guid Id => new Guid("C9895CF4-98B2-4023-A3EA-582107C7D80D");

        public Pattern[] Patterns { get; }

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var person in matches.Cast<Person>())
            {
                person.DomainFullName = $"{person.FirstName} {person.LastName}";
            }
        }
    }
}

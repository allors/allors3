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

    public class PersonGreetingDerivation : IDomainDerivation
    {
        public PersonGreetingDerivation(M m) =>
            this.Patterns = new[]
            {
                new AssociationPattern(m.Person.DomainFullName)
            };

        public Guid Id => new Guid("5FFD5696-E735-4D05-8405-3A444B6F591E");

        public Pattern[] Patterns { get; }

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var person in matches.Cast<Person>())
            {
                person.DomainGreeting = $"Hello {person.DomainFullName}!";
            }
        }
    }
}

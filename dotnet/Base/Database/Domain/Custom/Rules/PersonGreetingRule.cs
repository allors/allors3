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

    public class PersonGreetingRule : Rule
    {
        public PersonGreetingRule(MetaPopulation m) : base(m, new Guid("5FFD5696-E735-4D05-8405-3A444B6F591E")) =>
            this.Patterns = new[]
            {
                new RolePattern(m.Person, m.Person.DomainFullName)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var person in matches.Cast<Person>())
            {
                person.DomainGreeting = $"Hello {person.DomainFullName}!";
            }
        }
    }
}

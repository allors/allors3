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

    public class PersonGreetingDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("5FFD5696-E735-4D05-8405-3A444B6F591E");

        public IEnumerable<Pattern> Patterns { get; } = new[] { new ChangedRolePattern(M.Person.DomainFullName) };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var person in matches.Cast<Person>())
            {
                person.DomainGreeting = $"Hello {person.DomainFullName}!";
            }
        }
    }
}

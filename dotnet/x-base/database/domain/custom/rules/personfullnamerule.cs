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

    public class PersonFullNameRule : Rule
    {
        public PersonFullNameRule(MetaPopulation m) : base(m, new Guid("C9895CF4-98B2-4023-A3EA-582107C7D80D")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Person, m.Person.FirstName),
                new RolePattern(m.Person, m.Person.LastName),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var person in matches.Cast<Person>())
            {
                person.DomainFullName = $"{person.FirstName} {person.LastName}";
            }
        }
    }
}

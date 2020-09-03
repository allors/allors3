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

    public class PersonFullNameDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("C9895CF4-98B2-4023-A3EA-582107C7D80D");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new ChangedRolePattern( M.Person.FirstName),
            new ChangedRolePattern( M.Person.LastName),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var person in matches.Cast<Person>())
            {
                person.DomainFullName = $"{person.FirstName} {person.LastName}";
            }
        }
    }
}

// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Workspace.Derivations;

    public class PersonSessionFullNameRule : Rule
    {
        public PersonSessionFullNameRule(M m) : base(m, new Guid("CCDFC57D-5164-4BE6-9FFA-BA2862ABD006")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Person, m.Person.FirstName),
                new RolePattern(m.Person, m.Person.LastName),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var person in matches.Cast<Person>())
            {
                person.SessionFullName = $"{person.FirstName} {person.LastName}";
            }
        }
    }
}

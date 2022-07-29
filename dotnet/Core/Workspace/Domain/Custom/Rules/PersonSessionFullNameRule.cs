// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Domain
{
    using Derivations;
    using Meta;

    public class PersonSessionFullNameRule : Rule
    {
        public PersonSessionFullNameRule(M m) : base(m)
        {
            this.ObjectType = m.Person;
            this.RoleType = m.Person.SessionFullName;
            this.Dependencies = new IDependency[]
            {
                new Dependency(m.Person, m.Person.FirstName),
                new Dependency(m.Person, m.Person.LastName),
            };
        }

        public override object Derive(IObject match)
        {
            var person = (Person)match;
            return $"{person.FirstName} {person.LastName}";
        }
    }
}

// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Data;
    using Database.Derivations;
    using Derivations.Rules;
    using Meta;

    public class PersonCustomFullNameRule : Rule
    {
        public PersonCustomFullNameRule(MetaPopulation m) : base(m, new Guid("C9895CF4-98B2-4023-A3EA-582107C7D80D")) =>
            this.Patterns = new IRolePattern[]
            {
                new CustomRolePattern(m.Person.FirstName),
                new CustomRolePattern(m.Person.LastName),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var person in matches.Cast<Person>())
            {
                person.CustomFullName = $"{person.FirstName} {person.LastName}";
            }
        }

        private class CustomRolePattern : IRolePattern
        {
            public CustomRolePattern(IRoleType roleType) => this.RoleType = roleType;

            public IEnumerable<Node> Tree => null;

            public IComposite OfType => null;

            public IComposite ObjectType => null;

            public IRoleType RoleType { get; }
        }
    }
}

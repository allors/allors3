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

    public class PersonAddressRule : Rule
    {
        public PersonAddressRule(MetaPopulation m) : base(m, new Guid("E6F95E43-838D-47DF-AC8A-F1B9CB89995F")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Person, m.Person.MainAddress),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Person>())
            {
                @this.Address = @this.MainAddress;
            }
        }
    }
}

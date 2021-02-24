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

    public class PersonOwningDerivation : IDomainDerivation
    {
        public PersonOwningDerivation(M m) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Organisation.Owner),
            };

        public Guid Id => new Guid("31564037-C654-45AA-BC2B-69735A93F227");

        public Pattern[] Patterns { get; }

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var person in matches.Cast<Person>())
            {
                person.Owning = person.ExistOrganisationsWhereOwner;
            }
        }
    }
}

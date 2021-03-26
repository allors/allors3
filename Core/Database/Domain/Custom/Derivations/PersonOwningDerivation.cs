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

    public class PersonOwningDerivation : DomainDerivation
    {
        public PersonOwningDerivation(M m) : base(m, new Guid("31564037-C654-45AA-BC2B-69735A93F227")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.Organisation.Owner),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var person in matches.Cast<Person>())
            {
                person.Owning = person.ExistOrganisationsWhereOwner;
            }
        }
    }
}

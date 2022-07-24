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
    using Derivations.Rules;
    using Meta;

    public class PersonOwningRule : Rule
    {
        public PersonOwningRule(MetaPopulation m) : base(m, new Guid("31564037-C654-45AA-BC2B-69735A93F227")) =>
            this.Patterns = new Pattern[]
            {
                m.Person.AssociationPattern(v => v.OrganisationsWhereOwner)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var person in matches.Cast<Person>())
            {
                person.Owning = person.ExistOrganisationsWhereOwner;
            }
        }
    }
}

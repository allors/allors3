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

    public class CustomerRelationshipRule : Rule
    {
        public CustomerRelationshipRule(MetaPopulation m) : base(m, new Guid("3E1DE413-1939-4369-AFA0-D3A6CA340DD5")) =>
            this.Patterns = new Pattern[]
            {
                m.CustomerRelationship.RolePattern(v => v.Customer),
                m.CustomerRelationship.RolePattern(v => v.InternalOrganisation),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerRelationship>())
            {
                @this.Parties = new Party[] { @this.Customer, @this.InternalOrganisation };
            }
        }
    }
}

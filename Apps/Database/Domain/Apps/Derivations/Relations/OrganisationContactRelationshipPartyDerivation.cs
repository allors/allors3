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

    public class OrganisationContactRelationshipPartyDerivation : DomainDerivation
    {
        public OrganisationContactRelationshipPartyDerivation(M m) : base(m, new Guid("F21A5A56-0C20-4D33-B284-F366F04C3AF0")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(this.M.OrganisationContactRelationship.Organisation),
                new AssociationPattern(this.M.OrganisationContactRelationship.Contact),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<OrganisationContactRelationship>())
            {
                @this.Parties = new Party[] { @this.Contact, @this.Organisation };
            }
        }
    }
}

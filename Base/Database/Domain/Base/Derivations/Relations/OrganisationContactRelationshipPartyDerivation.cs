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

    public class OrganisationContactRelationshipPartyDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("F21A5A56-0C20-4D33-B284-F366F04C3AF0");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.OrganisationContactRelationship.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var organisationContactRelationship in matches.Cast<OrganisationContactRelationship>())
            {
                organisationContactRelationship.Parties = new Party[] { organisationContactRelationship.Contact, organisationContactRelationship.Organisation };
            }
        }
    }
}

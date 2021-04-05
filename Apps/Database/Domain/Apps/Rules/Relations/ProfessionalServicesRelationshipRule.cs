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

    public class ProfessionalServicesRelationshipRule : Rule
    {
        public ProfessionalServicesRelationshipRule(MetaPopulation m) : base(m, new Guid("DB0D802D-94D2-4850-91FC-703778ECFFC7")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.ProfessionalServicesRelationship, m.ProfessionalServicesRelationship.Professional),
                new RolePattern(m.ProfessionalServicesRelationship, m.ProfessionalServicesRelationship.ProfessionalServicesProvider),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ProfessionalServicesRelationship>())
            {
                @this.Parties = new Party[] { @this.Professional, @this.ProfessionalServicesProvider };

                if (!@this.ExistProfessional && !@this.ExistProfessionalServicesProvider)
                {
                    // TODO: Move Delete
                    @this.Delete();
                }
            }
        }
    }
}

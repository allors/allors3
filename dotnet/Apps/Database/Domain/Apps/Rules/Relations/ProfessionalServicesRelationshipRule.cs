// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class ProfessionalServicesRelationshipRule : Rule
    {
        public ProfessionalServicesRelationshipRule(MetaPopulation m) : base(m, new Guid("DB0D802D-94D2-4850-91FC-703778ECFFC7")) =>
            this.Patterns = new Pattern[]
            {
                m.ProfessionalServicesRelationship.RolePattern(v => v.Professional),
                m.ProfessionalServicesRelationship.RolePattern(v => v.ProfessionalServicesProvider),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
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

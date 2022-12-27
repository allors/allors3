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
    using Meta;
    using Derivations.Rules;

    public class ProfessionalServicesRelationshipDisplayPartyRelationshipRule : Rule
    {
        public ProfessionalServicesRelationshipDisplayPartyRelationshipRule(MetaPopulation m) : base(m, new Guid("690d1c1d-e16c-43c1-9341-10096fd67b47")) =>
        this.Patterns = new Pattern[]
        {
            m.ProfessionalServicesRelationship.RolePattern(v => v.Professional),
            m.ProfessionalServicesRelationship.RolePattern(v => v.ProfessionalServicesProvider),
            m.Person.RolePattern(v => v.DisplayName, v => v.ProfessionalServicesRelationshipsWhereProfessional),
            m.Organisation.RolePattern(v => v.DisplayName, v => v.ProfessionalServicesRelationshipsWhereProfessionalServicesProvider),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ProfessionalServicesRelationship>())
            {
                @this.DeriveProfessionalServicesRelationshipDisplayPartyRelationship(validation);
            }
        }
    }

    public static class ProfessionalServicesRelationshipDisplayPartyRelationshipRuleExtensions
    {
        public static void DeriveProfessionalServicesRelationshipDisplayPartyRelationship(this ProfessionalServicesRelationship @this, IValidation validation) => @this.DisplayPartyRelationship = $"{@this.Professional.DisplayName} is professional at {@this.ProfessionalServicesProvider.DisplayName}";
    }
}

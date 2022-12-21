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
    using Resources;

    public class ProfessionalServicesRelationshipFromDateRule : Rule
    {
        public ProfessionalServicesRelationshipFromDateRule(MetaPopulation m) : base(m, new Guid("9cb3ab82-6c50-4fd1-a438-3fc34d622626")) =>
        this.Patterns = new Pattern[]
        {
            m.ProfessionalServicesRelationship.RolePattern(v => v.FromDate),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ProfessionalServicesRelationship>())
            {
                @this.DeriveProfessionalServicesRelationshipFromDate(validation);
            }
        }
    }

    public static class ProfessionalServicesRelationshipFromDateRuleExtensions
    {
        public static void DeriveProfessionalServicesRelationshipFromDate(this ProfessionalServicesRelationship @this, IValidation validation)
        {
            if (@this.Professional.ProfessionalServicesRelationshipsWhereProfessional.Except(new[] { @this })
                .FirstOrDefault(v => v.ProfessionalServicesProvider.Equals(@this.ProfessionalServicesProvider)
                                    && v.FromDate.Date <= @this.FromDate.Date
                                    && (!v.ExistThroughDate || v.ThroughDate.Value.Date >= @this.FromDate.Date))
                != null)
            {
                validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
            }

            if (@this.Professional.ProfessionalServicesRelationshipsWhereProfessional.Except(new[] { @this })
                .FirstOrDefault(v => v.ProfessionalServicesProvider.Equals(@this.ProfessionalServicesProvider)
                                    && @this.FromDate.Date <= v.FromDate.Date
                                    && (!@this.ExistThroughDate || @this.ThroughDate.Value.Date >= v.FromDate.Date))
                != null)
            {
                validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
            }
        }
    }
}

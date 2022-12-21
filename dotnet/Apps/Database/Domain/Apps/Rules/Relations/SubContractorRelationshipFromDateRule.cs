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

    public class SubContractorRelationshipFromDateRule : Rule
    {
        public SubContractorRelationshipFromDateRule(MetaPopulation m) : base(m, new Guid("4b0e1cc3-22b8-4b29-a6c0-898642cfdc2f")) =>
        this.Patterns = new Pattern[]
        {
            m.SubContractorRelationship.RolePattern(v => v.FromDate),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SubContractorRelationship>())
            {
                @this.DeriveSubContractorRelationshipFromDate(validation);
            }
        }
    }

    public static class SubContractorRelationshipFromDateRuleExtensions
    {
        public static void DeriveSubContractorRelationshipFromDate(this SubContractorRelationship @this, IValidation validation)
        {
            if (@this.SubContractor.SubContractorRelationshipsWhereSubContractor.Except(new[] { @this })
                .FirstOrDefault(v => v.Contractor.Equals(@this.Contractor)
                                    && v.FromDate.Date <= @this.FromDate.Date
                                    && (!v.ExistThroughDate || v.ThroughDate.Value.Date >= @this.FromDate.Date))
                != null)
            {
                validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
            }

            if (@this.SubContractor.SubContractorRelationshipsWhereSubContractor.Except(new[] { @this })
                .FirstOrDefault(v => v.Contractor.Equals(@this.Contractor)
                                    && @this.FromDate.Date <= v.FromDate.Date
                                    && (!@this.ExistThroughDate || @this.ThroughDate.Value.Date >= v.FromDate.Date))
                != null)
            {
                validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
            }
        }
    }
}

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

    public class SubContractorRelationshipDisplayPartyRelationshipRule : Rule
    {
        public SubContractorRelationshipDisplayPartyRelationshipRule(MetaPopulation m) : base(m, new Guid("57f67141-cbbb-4829-bd12-02b5bed16a31")) =>
        this.Patterns = new Pattern[]
        {
            m.SubContractorRelationship.RolePattern(v => v.SubContractor),
            m.SubContractorRelationship.RolePattern(v => v.Contractor),
            m.Organisation.RolePattern(v => v.DisplayName, v => v.SubContractorRelationshipsWhereSubContractor),
            m.Organisation.RolePattern(v => v.DisplayName, v => v.SubContractorRelationshipsWhereContractor),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SubContractorRelationship>())
            {
                @this.DeriveSubContractorRelationshipDisplayPartyRelationship(validation);
            }
        }
    }

    public static class SubContractorRelationshipDisplayPartyRelationshipRuleExtensions
    {
        public static void DeriveSubContractorRelationshipDisplayPartyRelationship(this SubContractorRelationship @this, IValidation validation) => @this.DisplayPartyRelationship = $"{@this.SubContractor?.DisplayName} is subcontractor for {@this.Contractor?.DisplayName}";
    }
}

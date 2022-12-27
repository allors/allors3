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

    public class OrganisationRollUpDisplayPartyRelationshipRule : Rule
    {
        public OrganisationRollUpDisplayPartyRelationshipRule(MetaPopulation m) : base(m, new Guid("98e27b7d-ec9e-4916-8129-5729b617e42d")) =>
        this.Patterns = new Pattern[]
        {
            m.OrganisationRollUp.RolePattern(v => v.Parent),
            m.OrganisationRollUp.RolePattern(v => v.Child),
            m.Organisation.RolePattern(v => v.DisplayName, v => v.OrganisationRollUpsWhereParent),
            m.Organisation.RolePattern(v => v.DisplayName, v => v.OrganisationRollUpsWhereChild),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<OrganisationRollUp>())
            {
                @this.DeriveOrganisationRollUpDisplayPartyRelationship(validation);
            }
        }
    }

    public static class OrganisationRollUpDisplayPartyRelationshipRuleExtensions
    {
        public static void DeriveOrganisationRollUpDisplayPartyRelationship(this OrganisationRollUp @this, IValidation validation) => @this.DisplayPartyRelationship = $"{@this.Child?.DisplayName} is part of {@this.Parent?.DisplayName}";
    }
}

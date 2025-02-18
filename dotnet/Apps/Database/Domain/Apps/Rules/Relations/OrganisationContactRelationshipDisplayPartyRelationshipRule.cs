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

    public class OrganisationContactRelationshipDisplayPartyRelationshipRule : Rule
    {
        public OrganisationContactRelationshipDisplayPartyRelationshipRule(MetaPopulation m) : base(m, new Guid("5dafa0d4-51a2-439b-8c6f-bfa7c83966b4")) =>
        this.Patterns = new Pattern[]
        {
            m.OrganisationContactRelationship.RolePattern(v => v.Contact),
            m.OrganisationContactRelationship.RolePattern(v => v.Organisation),
            m.Person.RolePattern(v => v.DisplayName, v => v.OrganisationContactRelationshipsWhereContact),
            m.Organisation.RolePattern(v => v.DisplayName, v => v.OrganisationContactRelationshipsWhereOrganisation),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<OrganisationContactRelationship>())
            {
                @this.DeriveOrganisationContactRelationshipDisplayPartyRelationship(validation);
            }
        }
    }

    public static class OrganisationContactRelationshipDisplayPartyRelationshipRuleExtensions
    {
        public static void DeriveOrganisationContactRelationshipDisplayPartyRelationship(this OrganisationContactRelationship @this, IValidation validation) => @this.DisplayPartyRelationship = $"{@this.Contact?.DisplayName} is contact for {@this.Organisation?.DisplayName}";
    }
}

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

    public class PartyFinancialRelationshipDisplayPartyRelationshipRule : Rule
    {
        public PartyFinancialRelationshipDisplayPartyRelationshipRule(MetaPopulation m) : base(m, new Guid("8c71d3d9-8698-412e-ac1e-a11ede833174")) =>
        this.Patterns = new Pattern[]
        {
            m.PartyFinancialRelationship.RolePattern(v => v.FinancialParty),
            m.PartyFinancialRelationship.RolePattern(v => v.InternalOrganisation),
            m.Party.RolePattern(v => v.DisplayName, v => v.PartyFinancialRelationshipsWhereFinancialParty),
            m.Organisation.RolePattern(v => v.DisplayName, v => v.PartyFinancialRelationshipsWhereInternalOrganisation),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PartyFinancialRelationship>())
            {
                @this.DerivePartyFinancialRelationshipDisplayPartyRelationship(validation);
            }
        }
    }

    public static class PartyFinancialRelationshipDisplayPartyRelationshipRuleExtensions
    {
        public static void DerivePartyFinancialRelationshipDisplayPartyRelationship(this PartyFinancialRelationship @this, IValidation validation) => @this.DisplayPartyRelationship = $"{@this.FinancialParty.DisplayName} has account with {@this.InternalOrganisation.DisplayName}";
    }
}

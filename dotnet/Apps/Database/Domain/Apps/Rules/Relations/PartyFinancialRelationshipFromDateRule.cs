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

    public class PartyFinancialRelationshipFromDateRule : Rule
    {
        public PartyFinancialRelationshipFromDateRule(MetaPopulation m) : base(m, new Guid("963be291-950e-4dc7-868d-2c7c99b764a5")) =>
        this.Patterns = new Pattern[]
        {
            m.PartyFinancialRelationship.RolePattern(v => v.FromDate),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PartyFinancialRelationship>())
            {
                @this.DerivePartyFinancialRelationshipFromDate(validation);
            }
        }
    }

    public static class PartyFinancialRelationshipFromDateRuleExtensions
    {
        public static void DerivePartyFinancialRelationshipFromDate(this PartyFinancialRelationship @this, IValidation validation)
        {
            if (@this.FinancialParty.PartyFinancialRelationshipsWhereFinancialParty.Except(new[] { @this })
                .FirstOrDefault(v => v.InternalOrganisation.Equals(@this.InternalOrganisation)
                                    && v.FromDate.Date <= @this.FromDate.Date
                                    && (!v.ExistThroughDate || v.ThroughDate.Value.Date >= @this.FromDate.Date))
                != null)
            {
                validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
            }

            if (@this.FinancialParty.PartyFinancialRelationshipsWhereFinancialParty.Except(new[] { @this })
                .FirstOrDefault(v => v.InternalOrganisation.Equals(@this.InternalOrganisation)
                                    && @this.FromDate.Date <= v.FromDate.Date
                                    && (!@this.ExistThroughDate || @this.ThroughDate.Value.Date >= v.FromDate.Date))
                != null)
            {
                validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
            }
        }
    }
}

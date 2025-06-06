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

    public class PartyFinancialRelationshipDerivedSubAccountNumberRule : Rule
    {
        public PartyFinancialRelationshipDerivedSubAccountNumberRule(MetaPopulation m) : base(m, new Guid("98cce0cf-af0f-43aa-a144-73f73a88e147")) =>
        this.Patterns = new Pattern[]
        {
            m.PartyFinancialRelationship.RolePattern(v => v.ExternalSubAccountNumber),
            m.PartyFinancialRelationship.RolePattern(v => v.InternalSubAccountNumber),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PartyFinancialRelationship>())
            {
                @this.DerivePartyFinancialRelationshipDerivedSubAccountNumber(validation);
            }
        }
    }

    public static class PartyFinancialRelationshipDerivedSubAccountNumberRuleExtensions
    {
        public static void DerivePartyFinancialRelationshipDerivedSubAccountNumber(this PartyFinancialRelationship @this, IValidation validation) => @this.DerivedSubAccountNumber = @this.ExternalSubAccountNumber ?? @this.InternalSubAccountNumber;
    }
}

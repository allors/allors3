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

    public class GeneralLedgerAccountCostUnitAllowedRule : Rule
    {
        public GeneralLedgerAccountCostUnitAllowedRule(MetaPopulation m) : base(m, new Guid("b1f59562-0942-4a2e-b022-4c6979536ac0")) =>
            this.Patterns = new Pattern[]
            {
                m.GeneralLedgerAccount.RolePattern(v => v.DefaultCostUnit),
                m.GeneralLedgerAccount.RolePattern(v => v.AssignedCostUnitsAllowed),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<GeneralLedgerAccount>())
            {
                @this.DerivedCostUnitsAllowed = @this.AssignedCostUnitsAllowed;
                @this.AddDerivedCostUnitsAllowed(@this.DefaultCostUnit);
            }
        }
    }
}

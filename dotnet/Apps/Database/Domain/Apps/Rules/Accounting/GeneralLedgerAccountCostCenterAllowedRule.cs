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

    public class GeneralLedgerAccountCostCenterAllowedRule : Rule
    {
        public GeneralLedgerAccountCostCenterAllowedRule(MetaPopulation m) : base(m, new Guid("ea2b901c-fcb7-4ab5-a187-b50c28b4890b")) =>
            this.Patterns = new Pattern[]
            {
                m.GeneralLedgerAccount.RolePattern(v => v.AssignedCostCentersAllowed),
                m.GeneralLedgerAccount.RolePattern(v => v.DefaultCostCenter),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<GeneralLedgerAccount>())
            {
                @this.DerivedCostCentersAllowed = @this.AssignedCostCentersAllowed;
                @this.AddDerivedCostCentersAllowed(@this.DefaultCostCenter);
            }
        }
    }
}

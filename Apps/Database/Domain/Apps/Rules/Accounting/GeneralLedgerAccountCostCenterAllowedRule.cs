// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class GeneralLedgerAccountCostCenterAllowedRule : Rule
    {
        public GeneralLedgerAccountCostCenterAllowedRule(MetaPopulation m) : base(m, new Guid("ea2b901c-fcb7-4ab5-a187-b50c28b4890b")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.GeneralLedgerAccount, m.GeneralLedgerAccount.AssignedCostCentersAllowed),
                new RolePattern(m.GeneralLedgerAccount, m.GeneralLedgerAccount.DefaultCostCenter),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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

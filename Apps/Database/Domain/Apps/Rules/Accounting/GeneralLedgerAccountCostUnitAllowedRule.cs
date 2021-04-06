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

    public class GeneralLedgerAccountCostUnitAllowedRule : Rule
    {
        public GeneralLedgerAccountCostUnitAllowedRule(MetaPopulation m) : base(m, new Guid("b1f59562-0942-4a2e-b022-4c6979536ac0")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.GeneralLedgerAccount, m.GeneralLedgerAccount.DefaultCostUnit),

                new RolePattern(m.GeneralLedgerAccount, m.GeneralLedgerAccount.AssignedCostUnitsAllowed),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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

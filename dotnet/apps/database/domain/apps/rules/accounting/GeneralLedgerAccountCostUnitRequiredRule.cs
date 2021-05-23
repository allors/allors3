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

    public class GeneralLedgerAccountCostUnitRequiredRule : Rule
    {
        public GeneralLedgerAccountCostUnitRequiredRule(MetaPopulation m) : base(m, new Guid("dcfae819-97cc-43bd-9c77-5ed4579f1656")) =>
            this.Patterns = new Pattern[]
            {
                m.GeneralLedgerAccount.RolePattern(v => v.CostUnitAccount),
                m.GeneralLedgerAccount.RolePattern(v => v.CostUnitRequired),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<GeneralLedgerAccount>())
            {
                if (!@this.CostUnitAccount && @this.CostUnitRequired)
                {
                    validation.AddError($"{@this}, {@this.Meta.CostUnitRequired}, {ErrorMessages.NotACostUnitAccount}");
                }
            }
        }
    }
}

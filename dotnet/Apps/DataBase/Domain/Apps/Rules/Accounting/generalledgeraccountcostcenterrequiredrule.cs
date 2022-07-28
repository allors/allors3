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

    public class GeneralLedgerAccountCostCenterRequiredRule : Rule
    {
        public GeneralLedgerAccountCostCenterRequiredRule(MetaPopulation m) : base(m, new Guid("80069580-b47d-4250-854d-69c222d6a1e3")) =>
            this.Patterns = new Pattern[]
            {
                m.GeneralLedgerAccount.RolePattern(v => v.CostCenterAccount),
                m.GeneralLedgerAccount.RolePattern(v => v.CostCenterRequired),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<GeneralLedgerAccount>())
            {
                if (!@this.CostCenterAccount && @this.CostCenterRequired)
                {
                    validation.AddError(@this, @this.Meta.CostCenterRequired, ErrorMessages.NotACostCenterAccount);
                }
            }
        }
    }
}

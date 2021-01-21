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

    public class GeneralLedgerAccountDerivation : DomainDerivation
    {
        public GeneralLedgerAccountDerivation(M m) : base(m, new Guid("e916d6c3-b31b-41e2-b7ef-3265977e0fea")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.GeneralLedgerAccount.AccountNumber),
                new ChangedPattern(m.GeneralLedgerAccount.DefaultCostCenter),
                new ChangedPattern(m.GeneralLedgerAccount.AssignedCostCentersAllowed),
                new ChangedPattern(m.GeneralLedgerAccount.CostCenterAccount),
                new ChangedPattern(m.GeneralLedgerAccount.CostCenterRequired),
                new ChangedPattern(m.GeneralLedgerAccount.DefaultCostUnit),
                new ChangedPattern(m.GeneralLedgerAccount.AssignedCostUnitsAllowed),
                new ChangedPattern(m.GeneralLedgerAccount.CostUnitAccount),
                new ChangedPattern(m.GeneralLedgerAccount.CostUnitRequired),
                new ChangedPattern(m.ChartOfAccounts.GeneralLedgerAccounts) { Steps =  new IPropertyType[] { m.ChartOfAccounts.GeneralLedgerAccounts } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<GeneralLedgerAccount>())
            {
                @this.DerivedCostCentersAllowed = @this.AssignedCostCentersAllowed;
                @this.AddDerivedCostCentersAllowed(@this.DefaultCostCenter);

                @this.DerivedCostUnitsAllowed = @this.AssignedCostUnitsAllowed;
                @this.AddDerivedCostUnitsAllowed(@this.DefaultCostUnit);

                if (@this.ExistChartOfAccountsWhereGeneralLedgerAccount)
                {
                    var generalLedgerAccounts = @this.ChartOfAccountsWhereGeneralLedgerAccount.GeneralLedgerAccounts.Where(v => v.AccountNumber == @this.AccountNumber);

                    if (generalLedgerAccounts.Count() > 1)
                    {
                        validation.AddError($"{@this}, {@this.Meta.AccountNumber}, {ErrorMessages.AccountNumberUniqueWithinChartOfAccounts}");
                    }
                }

                if (!@this.CostCenterAccount && @this.CostCenterRequired)
                {
                    validation.AddError($"{@this}, {@this.Meta.CostCenterRequired}, {ErrorMessages.NotACostCenterAccount}");
                }

                if (!@this.CostUnitAccount && @this.CostUnitRequired)
                {
                    validation.AddError($"{@this}, {@this.Meta.CostUnitRequired}, {ErrorMessages.NotACostUnitAccount}");
                }
            }
        }
    }
}

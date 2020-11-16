// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Allors.Meta;
    using Derivations;
    using Resources;

    public class GeneralLedgerAccountDerivation : DomainDerivation
    {
        public GeneralLedgerAccountDerivation(M m) : base(m, new Guid("e916d6c3-b31b-41e2-b7ef-3265977e0fea")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.ChartOfAccounts.GeneralLedgerAccounts) { Steps =  new IPropertyType[] { m.ChartOfAccounts.GeneralLedgerAccounts } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<GeneralLedgerAccount>())
            {
                if (@this.ExistChartOfAccountsWhereGeneralLedgerAccount)
                {
                    var extent = @this.Strategy.Session.Extent<GeneralLedgerAccount>();
                    extent.Filter.AddEquals(@this.Meta.ChartOfAccountsWhereGeneralLedgerAccount, @this.ChartOfAccountsWhereGeneralLedgerAccount);
                    extent.Filter.AddEquals(@this.Meta.AccountNumber, @this.AccountNumber);

                    if (extent.Count > 1)
                    {
                        validation.AddError($"{@this}, {@this.Meta.AccountNumber}, {ErrorMessages.AccountNumberUniqueWithinChartOfAccounts}");
                    }
                }

                if (!@this.CostCenterAccount && @this.CostCenterRequired)
                {
                    validation.AddError($"{@this}, {@this.Meta.CostCenterRequired}, {ErrorMessages.NotACostCenterAccount}");
                }

                if (@this.CostCenterAccount && @this.ExistDefaultCostCenter)
                {
                    if (!@this.CostCentersAllowed.Contains(@this.DefaultCostCenter))
                    {
                        validation.AddError($"{@this}, {@this.Meta.DefaultCostCenter}, {ErrorMessages.CostCenterNotAllowed}");
                    }
                }

                if (!@this.CostUnitAccount && @this.CostUnitRequired)
                {
                    validation.AddError($"{@this}, {@this.Meta.CostCenterRequired}, {ErrorMessages.NotACostUnitAccount}");
                }

                if (@this.CostUnitAccount && @this.ExistDefaultCostUnit)
                {
                    if (!@this.CostUnitsAllowed.Contains(@this.DefaultCostUnit))
                    {
                        validation.AddError($"{@this}, {@this.Meta.DefaultCostUnit}, {ErrorMessages.CostUnitNotAllowed}");
                    }
                }
            }
        }
    }
}

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
                new AssociationPattern(m.GeneralLedgerAccount.AccountNumber),
                new AssociationPattern(m.GeneralLedgerAccount.DefaultCostCenter),
                new AssociationPattern(m.GeneralLedgerAccount.AssignedCostCentersAllowed),
                new AssociationPattern(m.GeneralLedgerAccount.CostCenterAccount),
                new AssociationPattern(m.GeneralLedgerAccount.CostCenterRequired),
                new AssociationPattern(m.GeneralLedgerAccount.DefaultCostUnit),
                new AssociationPattern(m.GeneralLedgerAccount.AssignedCostUnitsAllowed),
                new AssociationPattern(m.GeneralLedgerAccount.CostUnitAccount),
                new AssociationPattern(m.GeneralLedgerAccount.CostUnitRequired),
                new RolePattern(m.ChartOfAccounts.GeneralLedgerAccounts),
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

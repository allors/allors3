// <copyright file="GeneralLedgerAccount.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class GeneralLedgerAccount
    {
        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistCashAccount)
            {
                this.CashAccount = false;
            }

            if (!this.ExistCostCenterAccount)
            {
                this.CostCenterAccount = false;
            }

            if (!this.ExistCostCenterRequired)
            {
                this.CostCenterRequired = false;
            }

            if (!this.ExistCostUnitAccount)
            {
                this.CostUnitAccount = false;
            }

            if (!this.ExistCostUnitRequired)
            {
                this.CostUnitRequired = false;
            }

            if (!this.ExistReconciliationAccount)
            {
                this.ReconciliationAccount = false;
            }

            if (!this.ExistProtected)
            {
                this.Protected = false;
            }
        }
    }
}

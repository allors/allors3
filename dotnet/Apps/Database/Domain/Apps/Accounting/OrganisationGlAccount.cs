// <copyright file="OrganisationGlAccount.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class OrganisationGlAccount
    {
        public bool IsDeletable => !this.GeneralLedgerAccount.ExistAccountingTransactionDetailsWhereGeneralLedgerAccount;

        public bool IsNeutralAccount() =>
            !this.IsBankAccount() && !this.IsCashAccount() && !this.IsCostAccount() && !this.IsCostAccount()
            && !this.IsCreditorAccount() && !this.IsDebtorAccount() && !this.IsInventoryAccount();

        public bool IsBankAccount()
        {
            if (this.HasBankStatementTransactions)
            {
                return true;
            }

            return false;
        }

        public bool IsDebtorAccount() => false;

        public bool IsCreditorAccount() => false;

        public bool IsInventoryAccount() => false;

        public bool IsTurnOverAccount() => false;

        public bool IsCostAccount() => false;

        public bool IsCashAccount() => false;

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistFromDate)
            {
                this.FromDate = this.Transaction().Now();
            }

            this.HasBankStatementTransactions = false;
        }

        public void AppsOnInit(ObjectOnInit method)
        {
            var internalOrganisations = new Organisations(this.Strategy.Transaction).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

            if (!this.ExistInternalOrganisation && internalOrganisations.Count() == 1)
            {
                this.InternalOrganisation = internalOrganisations.First();
            }
        }
    }
}

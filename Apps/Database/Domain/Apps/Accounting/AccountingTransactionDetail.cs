// <copyright file="AccountingTransactionDetail.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class AccountingTransactionDetail
    {
        public string AppsDebitCreditString => this.Debit ? "Debit" : "Credit";

        public void AppsOnPreDerive(ObjectOnPreDerive method)
        {
            var (iteration, changeSet, derivedObjects) = method;

            if (iteration.IsMarked(this) || changeSet.IsCreated(this) || changeSet.HasChangedRoles(this))
            {
                iteration.AddDependency(this, this.OrganisationGlAccountBalance.AccountingPeriod);
                iteration.Mark(this.OrganisationGlAccountBalance.AccountingPeriod);
            }
        }
    }
}

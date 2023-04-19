// <copyright file="GeneralLedgerAccount.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class GeneralLedgerAccount
    {
        public bool IsDeletable => !this.OrganisationGlAccountsWhereGeneralLedgerAccount.Any(v => v.ExistAccountingTransactionDetailsWhereOrganisationGlAccount);

        public void AppsDelete(DeletableDelete method)
        {
            foreach(var @this in this.OrganisationGlAccountsWhereGeneralLedgerAccount)
            {
                @this.Delete();
            }
        }
    }
}

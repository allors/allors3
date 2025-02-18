// <copyright file="AccountingTransaction.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class AccountingTransaction
    {
        public bool IsDeletable => this.InternalOrganisation.ExportAccounting && !this.Exported;

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistEntryDate)
            {
                this.EntryDate = this.Transaction().Now();
            }
        }

        public void AppsOnInit(ObjectOnInit method)
        {
            // TODO: Don't extent for InternalOrganisations
            var internalOrganisations = new Organisations(this.Strategy.Transaction).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

            if (!this.ExistInternalOrganisation && internalOrganisations.Length == 1)
            {
                this.InternalOrganisation = internalOrganisations.First();
            }
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (this.IsDeletable)
            {
                foreach (var @this in this.AccountingTransactionDetails)
                {
                    // Detail is not Deletable by itself
                    @this.Strategy.Delete();
                }
            }
        }
    }
}

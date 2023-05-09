// <copyright file="VatRate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class VatRate
    {
        public bool IsDeletable => !this.ExistInvoicesWhereDerivedVatRate
            && !this.ExistInvoiceVatRateItemsWhereVatRate
            && !this.ExistOrdersWhereDerivedVatRate
            && !this.ExistPriceablesWhereVatRate
            && !this.ExistPriceableVersionsWhereVatRate
            && !this.ExistQuotesWhereDerivedVatRate;

        public void AppsDelete(DeletableDelete method)
        {
            if (!this.IsDeletable)
            {
                throw new Exception("Cannot delete Vat Rate");
            }

            foreach (var @this in this.InternalOrganisationVatRateSettingsesWhereVatRate)
            {
                @this.Delete();
            }
        }
    }
}

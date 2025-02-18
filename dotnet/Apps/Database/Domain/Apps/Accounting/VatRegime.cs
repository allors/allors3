// <copyright file="VatRegime.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class VatRegime
    {
        public bool IsDeletable => !this.ExistInvoicesWhereDerivedVatRegime
            && !this.ExistInvoiceVersionsWhereDerivedVatRegime
            && !this.ExistOrdersWhereDerivedVatRegime
            && !this.ExistOrderVersionsWhereDerivedVatRegime
            && !this.ExistPriceablesWhereDerivedVatRegime
            && !this.ExistPriceableVersionsWhereDerivedVatRegime
            && !this.ExistQuotesWhereDerivedVatRegime
            && !this.ExistQuoteVersionsWhereDerivedVatRegime
            && !this.ExistProductFeaturesWhereVatRegime
            && !this.ExistProductsWhereVatRegime;

        public void AppsDelete(DeletableDelete method)
        {
            if (this.IsDeletable)
            {
                foreach (var @this in this.VatRates)
                {
                    @this.Delete();
                }

                if (this.ExistVatClause)
                {
                    this.VatClause.Delete();
                }
            }
        }
    }
}

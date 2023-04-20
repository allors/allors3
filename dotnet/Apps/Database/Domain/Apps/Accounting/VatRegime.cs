// <copyright file="VatRegime.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class VatRegime
    {
        public bool IsDeletable => !this.ExistCountriesWhereDerivedVatRegime
            && !this.ExistInvoicesWhereDerivedVatRegime
            && !this.ExistInvoiceVersionsWhereDerivedVatRegime
            && !this.ExistInternalOrganisationVatRegimeSettingsesWhereVatRegime
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

                this.VatClause.Delete();
            }
        }
    }
}

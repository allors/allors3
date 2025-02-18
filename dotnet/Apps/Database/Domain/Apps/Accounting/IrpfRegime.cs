// <copyright file="IrpfRegime.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class IrpfRegime
    {
        public bool IsDeletable => !this.ExistInvoiceItemsWhereDerivedIrpfRegime
            && !this.ExistInvoiceItemVersionsWhereDerivedIrpfRegime
            && !this.ExistInvoicesWhereDerivedIrpfRegime
            && !this.ExistInvoiceVersionsWhereDerivedIrpfRegime
            && !this.ExistOrderItemsWhereDerivedIrpfRegime
            && !this.ExistOrderItemVersionsWhereDerivedIrpfRegime
            && !this.ExistOrdersWhereDerivedIrpfRegime
            && !this.ExistOrderVersionsWhereDerivedIrpfRegime
            && !this.ExistQuoteItemsWhereDerivedIrpfRegime
            && !this.ExistQuoteItemVersionsWhereDerivedIrpfRegime
            && !this.ExistQuotesWhereDerivedIrpfRegime
            && !this.ExistQuoteVersionsWhereDerivedIrpfRegime;

        public void AppsDelete(DeletableDelete method)
        {
            if (this.IsDeletable)
            {
                foreach (var @this in this.IrpfRates)
                {
                    @this.Delete();
                }
            }
        }
    }
}

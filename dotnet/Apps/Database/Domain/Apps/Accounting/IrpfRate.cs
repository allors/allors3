// <copyright file="IrpfRate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class IrpfRate
    {
        public bool IsDeletable => !this.ExistInvoiceItemsWhereIrpfRate
            && !this.ExistInvoiceItemVersionsWhereIrpfRate
            && !this.ExistInvoicesWhereDerivedIrpfRate
            && !this.ExistOrderItemsWhereIrpfRate
            && !this.ExistOrderItemVersionsWhereIrpfRate
            && !this.ExistOrdersWhereDerivedIrpfRate
            && !this.ExistQuoteItemsWhereIrpfRate
            && !this.ExistQuoteItemVersionsWhereIrpfRate
            && !this.ExistQuotesWhereDerivedIrpfRate;

        public void AppsDelete(DeletableDelete method)
        {
            if (this.IsDeletable)
            {
                this.Delete();
            }
        }
    }
}

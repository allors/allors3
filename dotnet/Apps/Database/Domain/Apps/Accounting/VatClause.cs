// <copyright file="VatClause.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class VatClause
    {
        public bool IsDeletable => !this.ExistQuotesWhereDerivedVatClause
            && !this.ExistQuoteVersionsWhereDerivedVatClause
            && !this.ExistSalesInvoicesWhereDerivedVatClause
            && !this.ExistSalesInvoiceVersionsWhereDerivedVatClause
            && !this.ExistSalesOrdersWhereDerivedVatClause
            && !this.ExistSalesOrderVersionsWhereDerivedVatClause;

        public void AppsDelete(DeletableDelete method)
        {
            if (this.IsDeletable)
            {
                this.Delete();
            }
        }
    }
}

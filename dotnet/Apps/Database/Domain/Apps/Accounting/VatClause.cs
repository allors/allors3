// <copyright file="VatClause.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

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
            if (!this.IsDeletable)
            {
                throw new Exception("Cannot delete Vat Clause");
            }
        }
    }
}

// <copyright file="PaymentApplication.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class PaymentApplication
    {
        public void AppsDelete(DeletableDelete method)
        {
            if (this.ExistInvoice)
            {
                this.Invoice.DerivationTrigger = Guid.NewGuid();
            }

            if (this.ExistInvoiceItem)
            {
                this.InvoiceItem.DerivationTrigger = Guid.NewGuid();
            }
        }
    }
}

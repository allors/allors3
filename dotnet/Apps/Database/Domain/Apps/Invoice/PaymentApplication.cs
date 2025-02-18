// <copyright file="PaymentApplication.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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
            if (this.ExistOrder)
            {
                this.Order.DerivationTrigger = Guid.NewGuid();
            }

            if (this.ExistOrderItem)
            {
                this.OrderItem.DerivationTrigger = Guid.NewGuid();
            }
        }
    }
}

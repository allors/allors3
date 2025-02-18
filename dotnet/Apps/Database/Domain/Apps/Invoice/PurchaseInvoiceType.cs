// <copyright file="PurchaseInvoiceType.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PurchaseInvoiceType
    {
        public bool IsPurchaseInvoice => this.UniqueId == PurchaseInvoiceTypes.PurchaseInvoiceId;

        public bool IsPurchaseReturn => this.UniqueId == PurchaseInvoiceTypes.PurchaseReturnId;
    }
}

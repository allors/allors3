// <copyright file="PurchaseInvoiceType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

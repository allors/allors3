// <copyright file="SalesInvoiceType.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SalesInvoiceType
    {
        public bool IsSalesInvoice => this.UniqueId == SalesInvoiceTypes.SalesInvoiceId;

        public bool IsCreditNote => this.UniqueId == SalesInvoiceTypes.CreditNoteId;
    }
}

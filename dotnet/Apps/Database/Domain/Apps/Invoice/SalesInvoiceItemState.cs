// <copyright file="SalesInvoiceItemState.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SalesInvoiceItemState
    {
        public bool IsPaid => this.UniqueId == SalesInvoiceItemStates.PaidId;

        public bool IsNotPaid => this.UniqueId == SalesInvoiceItemStates.NotPaidId;

        public bool IsPartiallyPaid => this.UniqueId == SalesInvoiceItemStates.PartiallyPaidId;

        public bool IsReadyForPosting => this.UniqueId == SalesInvoiceItemStates.ReadyForPostingId;

        public bool IsWrittenOff => this.UniqueId == SalesInvoiceItemStates.WrittenOffId;

        public bool IsCancelledByInvoice => this.UniqueId == SalesInvoiceItemStates.CancelledByInvoiceId;
    }
}

// <copyright file="FiscalYearInvoiceNumber.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    // TODO: Why not use Counters?
    public partial class FiscalYearInvoiceNumber
    {
        public int DeriveNextSalesInvoiceNumber() => this.NextSalesInvoiceNumber++;

        public int DeriveNextCreditNoteNumber() => this.NextCreditNoteNumber++;
    }
}

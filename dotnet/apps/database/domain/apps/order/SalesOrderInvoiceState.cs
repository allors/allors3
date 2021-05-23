// <copyright file="SalesOrderInvoiceState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SalesOrderInvoiceState
    {
        public bool IsNotInvoiced => Equals(this.UniqueId, SalesOrderInvoiceStates.NotInvoicedId);

        public bool IsPartiallyInvoiced => Equals(this.UniqueId, SalesOrderInvoiceStates.PartiallyInvoicedId);

        public bool IsInvoiced => Equals(this.UniqueId, SalesOrderInvoiceStates.InvoicedId);
    }
}

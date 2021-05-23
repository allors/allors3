// <copyright file="SalesOrderItemInvoiceState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SalesOrderItemInvoiceState
    {
        public bool IsNotInvoiced => Equals(this.UniqueId, SalesOrderItemInvoiceStates.NotInvoicedId);

        public bool IsPartiallyInvoiced => Equals(this.UniqueId, SalesOrderItemInvoiceStates.PartiallyInvoicedId);

        public bool IsInvoiced => Equals(this.UniqueId, SalesOrderItemInvoiceStates.InvoicedId);
    }
}

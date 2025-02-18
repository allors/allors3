// <copyright file="PurchaseOrderModel.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.PurchaseInvoiceModel
{
    public class PurchaseOrderModel
    {
        public PurchaseOrderModel(PurchaseOrder order) => this.OrderNumber = order.OrderNumber;

        public string OrderNumber { get; }
    }
}

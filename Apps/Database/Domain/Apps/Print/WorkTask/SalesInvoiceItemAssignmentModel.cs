// <copyright file="SalesInvoiceItemAssignmentModel.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.WorkTaskModel
{
    using System.Globalization;

    public class SalesInvoiceItemAssignmentModel
    {
        public SalesInvoiceItemAssignmentModel(WorkEffortSalesInvoiceItemAssignment salesInvoiceItemAssignment)
        {
            this.Description = salesInvoiceItemAssignment.SalesInvoiceItem.Description;
            this.Quantity = salesInvoiceItemAssignment.SalesInvoiceItem.Quantity;
            this.UnitSellingPrice = salesInvoiceItemAssignment.SalesInvoiceItem.AssignedUnitPrice.Value.ToString("N2", new CultureInfo("nl-BE"));
            this.SellingPrice = Rounder.RoundDecimal(this.Quantity * salesInvoiceItemAssignment.SalesInvoiceItem.AssignedUnitPrice.Value, 2).ToString("N2", new CultureInfo("nl-BE"));
        }

        public string Description { get; }

        public decimal Quantity { get; }

        public string UnitSellingPrice { get; }

        public string SellingPrice { get; }
    }
}

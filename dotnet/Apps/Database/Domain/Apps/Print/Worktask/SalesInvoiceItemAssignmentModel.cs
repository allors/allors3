// <copyright file="SalesInvoiceItemAssignmentModel.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.WorkTaskModel
{
    using System.Globalization;

    public class SalesInvoiceItemAssignmentModel
    {
        public SalesInvoiceItemAssignmentModel(WorkEffortInvoiceItemAssignment workEffortInvoiceItemAssignment)
        {
            var amount = workEffortInvoiceItemAssignment.WorkEffortInvoiceItem.Amount ?? 0M;

            this.Type = workEffortInvoiceItemAssignment.WorkEffortInvoiceItem.InvoiceItemType.Name;
            this.Description = workEffortInvoiceItemAssignment.WorkEffortInvoiceItem.Description;
            this.Quantity = 1;
            this.UnitSellingPrice = amount.ToString("N2", new CultureInfo("nl-BE"));
            this.SellingPrice = Rounder.RoundDecimal(this.Quantity * amount, 2).ToString("N2", new CultureInfo("nl-BE"));
        }

        public string Type { get; }

        public string Description { get; }

        public decimal Quantity { get; }

        public string UnitSellingPrice { get; }

        public string SellingPrice { get; }
    }
}

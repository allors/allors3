// <copyright file="SalesInvoiceItemAssignmentModel.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.WorkTaskModel
{
    using System.Globalization;

    public class WorkEffortInvoiceItemAssignmentModel
    {
        public WorkEffortInvoiceItemAssignmentModel(WorkEffortInvoiceItemAssignment workEffortInvoiceItemAssignment)
        {
            var amount = workEffortInvoiceItemAssignment.WorkEffortInvoiceItem.Amount ?? 0M;

            this.Description = workEffortInvoiceItemAssignment.WorkEffortInvoiceItem.Description;
            this.Quantity = 1;
            this.UnitSellingPrice = amount.ToString("N2", new CultureInfo("nl-BE"));
            this.SellingPrice = Rounder.RoundDecimal(this.Quantity * amount, 2).ToString("N2", new CultureInfo("nl-BE"));
        }

        public string Description { get; }

        public decimal Quantity { get; }

        public string UnitSellingPrice { get; }

        public string SellingPrice { get; }
    }
}

// <copyright file="WorkEffortExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

    public static partial class WorkEffortExtensions
    {
        public static DateTime? FromDate(this WorkEffort @this) => @this.ActualStart ?? @this.ScheduledStart;

        public static DateTime? ThroughDate(this WorkEffort @this) => @this.ActualCompletion ?? @this.ScheduledCompletion;

        public static TimeEntry[] BillableTimeEntries(this WorkEffort @this) => @this.ServiceEntriesWhereWorkEffort.OfType<TimeEntry>()
            .Where(v => v.IsBillable
                        && (!v.BillableAmountOfTime.HasValue && v.AmountOfTime.HasValue) || v.BillableAmountOfTime.HasValue)
            .Select(v => v)
            .ToArray();

        public static void AppsOnBuild(this WorkEffort @this, ObjectOnBuild method)
        {
            if (!@this.ExistWorkEffortState)
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Session).Created;
            }

            if (!@this.ExistOwner && @this.Strategy.Session.Context().User is Person owner)
            {
                @this.Owner = owner;
            }

            @this.DerivationTrigger = Guid.NewGuid();
        }

        public static void AppsOnInit(this WorkEffort @this, ObjectOnInit method)
        {
            var internalOrganisations = new Organisations(@this.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

            if (!@this.ExistTakenBy && internalOrganisations.Count() == 1)
            {
                @this.TakenBy = internalOrganisations.First();
            }
        }

        public static void AppsComplete(this WorkEffort @this, WorkEffortComplete method)
        {
            if (!method.Result.HasValue)
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Session).Completed;
                method.Result = true;
            }
        }

        public static void AppsCancel(this WorkEffort @this, WorkEffortCancel cancel) => @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Session).Cancelled;

        public static void AppsReopen(this WorkEffort @this, WorkEffortReopen reopen)
        {
            if (@this.ExistActualStart)
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Session).InProgress;
            }
            else
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Session).Created;
            }
        }

        public static void AppsRevise(this WorkEffort @this, WorkEffortRevise method)
        {
            if (@this.ExistActualStart)
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Session).InProgress;
            }
            else
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Session).Created;
            }
        }

        public static void AppsInvoice(this WorkEffort @this, WorkEffortInvoice method)
        {
            if (!method.Result.HasValue)
            {
                if (@this.CanInvoice)
                {
                    @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Session).Finished;
                    @this.InvoiceThis();
                }

                method.Result = true;
            }
        }

        private static SalesInvoice InvoiceThis(this WorkEffort @this)
        {
            var salesInvoice = new SalesInvoiceBuilder(@this.Strategy.Session)
                .WithBilledFrom(@this.TakenBy)
                .WithBillToCustomer(@this.Customer)
                .WithBillToContactPerson(@this.ContactPerson)
                .WithInvoiceDate(@this.Strategy.Session.Now())
                .WithSalesInvoiceType(new SalesInvoiceTypes(@this.Strategy.Session).SalesInvoice)
                .Build();

            CreateInvoiceItems(@this, salesInvoice);
            foreach (WorkEffort childWorkEffort in @this.Children)
            {
                CreateInvoiceItems(childWorkEffort, salesInvoice);
            }

            return salesInvoice;
        }

        private static void CreateInvoiceItems(this WorkEffort @this, SalesInvoice salesInvoice)
        {
            var session = @this.Strategy.Session;

            var timeEntriesByBillingRate = @this.ServiceEntriesWhereWorkEffort.OfType<TimeEntry>()
                .Where(v => (v.IsBillable && !v.BillableAmountOfTime.HasValue && v.AmountOfTime.HasValue) || v.BillableAmountOfTime.HasValue)
                .GroupBy(v => v.BillingRate)
                .Select(v => v)
                .ToArray();

            foreach (var rateGroup in timeEntriesByBillingRate)
            {
                var timeEntries = rateGroup.ToArray();

                var invoiceItem = new SalesInvoiceItemBuilder(session)
                    .WithInvoiceItemType(new InvoiceItemTypes(session).Time)
                    .WithAssignedUnitPrice(rateGroup.Key)
                    .WithQuantity(timeEntries.Sum(v => v.BillableAmountOfTime ?? v.AmountOfTime ?? 0.0m))
                    .Build();

                salesInvoice.AddSalesInvoiceItem(invoiceItem);

                foreach (var billableEntry in timeEntries)
                {
                    new TimeEntryBillingBuilder(session)
                        .WithTimeEntry(billableEntry)
                        .WithInvoiceItem(invoiceItem)
                        .Build();
                }
            }

            foreach (WorkEffortInventoryAssignment workEffortInventoryAssignment in @this.WorkEffortInventoryAssignmentsWhereAssignment)
            {
                var part = workEffortInventoryAssignment.InventoryItem.Part;

                var quantity = workEffortInventoryAssignment.DerivedBillableQuantity != 0 ? workEffortInventoryAssignment.DerivedBillableQuantity : workEffortInventoryAssignment.Quantity;

                var invoiceItem = new SalesInvoiceItemBuilder(session)
                    .WithInvoiceItemType(new InvoiceItemTypes(session).PartItem)
                    .WithPart(part)
                    .WithAssignedUnitPrice(workEffortInventoryAssignment.UnitSellingPrice)
                    .WithQuantity(quantity)
                    .WithCostOfGoodsSold(workEffortInventoryAssignment.CostOfGoodsSold)
                    .Build();

                salesInvoice.AddSalesInvoiceItem(invoiceItem);

                new WorkEffortBillingBuilder(session)
                    .WithWorkEffort(@this)
                    .WithInvoiceItem(invoiceItem)
                    .Build();
            }
        }

        private static void AppsCalculateTotalRevenue(this WorkEffort @this, WorkEffortCalculateTotalRevenue method)
        {
            if (!method.Result.HasValue)
            {
                @this.TotalLabourRevenue = Math.Round(@this.BillableTimeEntries().Sum(v => v.BillingAmount), 2);
                @this.TotalMaterialRevenue = Math.Round(@this.WorkEffortInventoryAssignmentsWhereAssignment.Where(v => v.DerivedBillableQuantity > 0).Sum(v => v.DerivedBillableQuantity * v.UnitSellingPrice), 2);
                @this.TotalSubContractedRevenue = Math.Round(@this.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment.Sum(v => v.Quantity * v.UnitSellingPrice), 2);
                var totalRevenue = Math.Round(@this.TotalLabourRevenue + @this.TotalMaterialRevenue + @this.TotalSubContractedRevenue, 2);

                method.Result = true;

                @this.GrandTotal = totalRevenue;
                @this.TotalRevenue = @this.Customer.Equals(@this.ExecutedBy) ? 0M : totalRevenue;
            }
        }
    }
}

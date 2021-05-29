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
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).Created;
            }

            if (!@this.ExistOwner && @this.Strategy.Transaction.Services().User is Person owner)
            {
                @this.Owner = owner;
            }

            @this.DerivationTrigger = Guid.NewGuid();
        }

        public static void AppsOnInit(this WorkEffort @this, ObjectOnInit method)
        {
            var internalOrganisations = new Organisations(@this.Strategy.Transaction).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

            if (!@this.ExistTakenBy && internalOrganisations.Count() == 1)
            {
                @this.TakenBy = internalOrganisations.First();
            }
        }

        public static void AppsComplete(this WorkEffort @this, WorkEffortComplete method)
        {
            @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).Completed;
            method.StopPropagation = true;
        }

        public static void AppsCancel(this WorkEffort @this, WorkEffortCancel method)
        {
            @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).Cancelled;
            method.StopPropagation = true;
        }

        public static void AppsReopen(this WorkEffort @this, WorkEffortReopen method)
        {
            if (@this.ExistActualStart)
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).InProgress;
            }
            else
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).Created;
            }

            method.StopPropagation = true;
        }

        public static void AppsRevise(this WorkEffort @this, WorkEffortRevise method)
        {
            if (@this.ExistActualStart)
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).InProgress;
            }
            else
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).Created;
            }

            method.StopPropagation = true;
        }

        public static void AppsInvoice(this WorkEffort @this, WorkEffortInvoice method)
        {
            if (@this.CanInvoice)
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).Finished;

                var salesInvoice = new SalesInvoiceBuilder(@this.Strategy.Transaction)
                    .WithBilledFrom(@this.TakenBy)
                    .WithBillToCustomer(@this.Customer)
                    .WithBillToContactPerson(@this.ContactPerson)
                    .WithInvoiceDate(@this.Strategy.Transaction.Now())
                    .WithSalesInvoiceType(new SalesInvoiceTypes(@this.Strategy.Transaction).SalesInvoice)
                    .Build();

                CreateInvoiceItems(@this, salesInvoice);
                foreach (WorkEffort childWorkEffort in @this.Children)
                {
                    CreateInvoiceItems(childWorkEffort, salesInvoice);
                }

                @this.CanInvoice = false;
            }

            method.StopPropagation = true;
        }

        private static void CreateInvoiceItems(this WorkEffort @this, SalesInvoice salesInvoice)
        {
            var transaction = @this.Strategy.Transaction;

            var timeEntriesByBillingRate = @this.ServiceEntriesWhereWorkEffort.OfType<TimeEntry>()
                .Where(v => (v.IsBillable && !v.BillableAmountOfTime.HasValue && v.AmountOfTime.HasValue) || v.BillableAmountOfTime.HasValue)
                .GroupBy(v => v.BillingRate)
                .Select(v => v)
                .ToArray();

            foreach (var rateGroup in timeEntriesByBillingRate)
            {
                var timeEntries = rateGroup.ToArray();

                var invoiceItem = new SalesInvoiceItemBuilder(transaction)
                    .WithInvoiceItemType(new InvoiceItemTypes(transaction).Time)
                    .WithAssignedUnitPrice(rateGroup.Key)
                    .WithQuantity(timeEntries.Sum(v => v.BillableAmountOfTime ?? v.AmountOfTime ?? 0.0m))
                    .Build();

                salesInvoice.AddSalesInvoiceItem(invoiceItem);

                foreach (var billableEntry in timeEntries)
                {
                    new TimeEntryBillingBuilder(transaction)
                        .WithTimeEntry(billableEntry)
                        .WithInvoiceItem(invoiceItem)
                        .Build();
                }
            }

            foreach (WorkEffortInventoryAssignment workEffortInventoryAssignment in @this.WorkEffortInventoryAssignmentsWhereAssignment)
            {
                var part = workEffortInventoryAssignment.InventoryItem.Part;

                var quantity = workEffortInventoryAssignment.DerivedBillableQuantity != 0 ? workEffortInventoryAssignment.DerivedBillableQuantity : workEffortInventoryAssignment.Quantity;

                var invoiceItem = new SalesInvoiceItemBuilder(transaction)
                    .WithInvoiceItemType(new InvoiceItemTypes(transaction).PartItem)
                    .WithPart(part)
                    .WithAssignedUnitPrice(workEffortInventoryAssignment.UnitSellingPrice)
                    .WithQuantity(quantity)
                    .WithCostOfGoodsSold(workEffortInventoryAssignment.CostOfGoodsSold)
                    .Build();

                salesInvoice.AddSalesInvoiceItem(invoiceItem);

                new WorkEffortBillingBuilder(transaction)
                    .WithWorkEffort(@this)
                    .WithInvoiceItem(invoiceItem)
                    .Build();
            }

            foreach (WorkEffortSalesInvoiceItemAssignment workEffortSalesInvoiceItemAssignment in @this.WorkEffortSalesInvoiceItemAssignmentsWhereAssignment)
            {
                var clone = workEffortSalesInvoiceItemAssignment.SalesInvoiceItem.Clone();

                salesInvoice.AddSalesInvoiceItem(clone);

                new WorkEffortBillingBuilder(transaction)
                    .WithWorkEffort(@this)
                    .WithInvoiceItem(clone)
                    .Build();
            }
        }
    }
}

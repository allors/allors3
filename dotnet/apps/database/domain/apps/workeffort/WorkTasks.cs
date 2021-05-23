// <copyright file="WorkTasks.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    using Meta;

    public partial class WorkTasks
    {
        public static void AppsMonthly(ITransaction transaction)
        {
            var m = transaction.Database.Context().M;

            var customers = new Parties(transaction).Extent();
            customers.Filter.AddEquals(m.Party.CollectiveWorkEffortInvoice, true);

            var workTasks = new WorkTasks(transaction).Extent();
            workTasks.Filter.AddEquals(m.WorkEffort.WorkEffortState, new WorkEffortStates(transaction).Completed);
            workTasks.Filter.AddContainedIn(m.WorkEffort.Customer, (Extent)customers);

            var workTasksByCustomer = workTasks.Select(v => v.Customer).Distinct()
                .ToDictionary(v => v, v => v.WorkEffortsWhereCustomer.Where(w => w.WorkEffortState.Equals(new WorkEffortStates(transaction).Completed)).ToArray());

            SalesInvoice salesInvoice = null;

            foreach (var customerWorkTasks in workTasksByCustomer)
            {
                var customer = customerWorkTasks.Key;

                var customerWorkTasksByInternalOrganisation = customerWorkTasks.Value
                    .GroupBy(v => v.TakenBy)
                    .Select(v => v)
                    .ToArray();

                if (customerWorkTasks.Value.Any(v => v.CanInvoice))
                {
                    foreach (var group in customerWorkTasksByInternalOrganisation)
                    {
                        if (group.Any(v => v.CanInvoice))
                        {
                            salesInvoice = new SalesInvoiceBuilder(transaction)
                                .WithBilledFrom(group.Key)
                                .WithBillToCustomer(customer)
                                .WithInvoiceDate(transaction.Now())
                                .WithSalesInvoiceType(new SalesInvoiceTypes(transaction).SalesInvoice)
                                .Build();
                        }

                        var timeEntriesByBillingRate = group.SelectMany(v => v.ServiceEntriesWhereWorkEffort.OfType<TimeEntry>())
                            .Where(v => (v.IsBillable && !v.BillableAmountOfTime.HasValue && v.AmountOfTime.HasValue) || v.BillableAmountOfTime.HasValue)
                            .GroupBy(v => v.BillingRate)
                            .Select(v => v)
                            .ToArray();

                        foreach (var rateGroup in timeEntriesByBillingRate)
                        {
                            var timeEntries = rateGroup.ToArray();

                            var invoiceItem = new SalesInvoiceItemBuilder(transaction)
                                .WithInvoiceItemType(new InvoiceItemTypes(transaction).Service)
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

                        foreach (var workEffort in group)
                        {
                            if (workEffort.CanInvoice)
                            {
                                if (string.IsNullOrEmpty(salesInvoice.CustomerReference))
                                {
                                    salesInvoice.CustomerReference = $"WorkOrder(s): {workEffort.WorkEffortNumber}";
                                }
                                else
                                {
                                    salesInvoice.CustomerReference += $", {workEffort.WorkEffortNumber}";
                                }

                                foreach (WorkEffortInventoryAssignment inventoryAssignment in workEffort.WorkEffortInventoryAssignmentsWhereAssignment)
                                {
                                    var part = inventoryAssignment.InventoryItem.Part;

                                    var invoiceItem = new SalesInvoiceItemBuilder(transaction)
                                        .WithInvoiceItemType(new InvoiceItemTypes(transaction).PartItem)
                                        .WithPart(part)
                                        .WithAssignedUnitPrice(inventoryAssignment.UnitSellingPrice)
                                        .WithQuantity(inventoryAssignment.DerivedBillableQuantity)
                                        .Build();

                                    salesInvoice.AddSalesInvoiceItem(invoiceItem);

                                    new WorkEffortBillingBuilder(transaction)
                                        .WithWorkEffort(workEffort)
                                        .WithInvoiceItem(invoiceItem)
                                        .Build();
                                }

                                foreach (WorkEffortPurchaseOrderItemAssignment purchaseOrderItemAssignment in workEffort.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment)
                                {
                                    var invoiceItem = new SalesInvoiceItemBuilder(transaction)
                                        .WithInvoiceItemType(new InvoiceItemTypes(transaction).Service)
                                        .WithAssignedUnitPrice(purchaseOrderItemAssignment.UnitSellingPrice)
                                        .WithQuantity(purchaseOrderItemAssignment.Quantity)
                                        .Build();

                                    salesInvoice.AddSalesInvoiceItem(invoiceItem);

                                    new WorkEffortBillingBuilder(transaction)
                                        .WithWorkEffort(workEffort)
                                        .WithInvoiceItem(invoiceItem)
                                        .Build();
                                }

                                workEffort.WorkEffortState = new WorkEffortStates(transaction).Finished;
                            }
                        }
                    }
                }
            }
        }

        protected override void AppsSecure(Security config)
        {
            var created = new WorkEffortStates(this.Transaction).Created;
            var inProgress = new WorkEffortStates(this.Transaction).InProgress;
            var cancelled = new WorkEffortStates(this.Transaction).Cancelled;
            var completed = new WorkEffortStates(this.Transaction).Completed;
            var finished = new WorkEffortStates(this.Transaction).Finished;

            var cancel = this.Meta.Cancel;
            var reopen = this.Meta.Reopen;
            var complete = this.Meta.Complete;
            var invoice = this.Meta.Invoice;
            var revise = this.Meta.Revise;

            config.Deny(this.ObjectType, created, reopen, complete, invoice, revise);
            config.Deny(this.ObjectType, inProgress, cancel, reopen, revise);
            config.Deny(this.ObjectType, cancelled, cancel, invoice, complete, revise);
            config.Deny(this.ObjectType, completed, cancel, reopen, complete);
            config.Deny(this.ObjectType, finished, cancel, reopen, complete, invoice, revise);

            var except = new HashSet<IOperandType>
            {
                this.Meta.ElectronicDocuments,
                this.Meta.Print,
            };

            config.DenyExcept(this.ObjectType, cancelled, except, Operations.Write);
            config.DenyExcept(this.ObjectType, completed, except, Operations.Write);
            config.DenyExcept(this.ObjectType, finished, except, Operations.Write);

            config.Deny(this.M.TimeEntry, cancelled, Operations.Write);
            config.Deny(this.M.TimeEntry, finished, Operations.Write);
            config.Deny(this.M.TimeEntry, completed, Operations.Write);
            config.Deny(this.M.WorkEffortAssignmentRate, cancelled, Operations.Write);
            config.Deny(this.M.WorkEffortAssignmentRate, finished, Operations.Write);
            config.Deny(this.M.WorkEffortAssignmentRate, completed, Operations.Write);
            config.Deny(this.M.WorkEffortInventoryAssignment, cancelled, Operations.Write);
            config.Deny(this.M.WorkEffortInventoryAssignment, finished, Operations.Write);
            config.Deny(this.M.WorkEffortInventoryAssignment, completed, Operations.Write);
            config.Deny(this.M.WorkEffortPartyAssignment, cancelled, Operations.Write);
            config.Deny(this.M.WorkEffortPartyAssignment, finished, Operations.Write);
            config.Deny(this.M.WorkEffortPartyAssignment, completed, Operations.Write);
            config.Deny(this.M.WorkEffortPurchaseOrderItemAssignment, cancelled, Operations.Write);
            config.Deny(this.M.WorkEffortPurchaseOrderItemAssignment, finished, Operations.Write);
            config.Deny(this.M.WorkEffortPurchaseOrderItemAssignment, completed, Operations.Write);
            config.Deny(this.M.WorkEffortFixedAssetAssignment, cancelled, Operations.Write);
            config.Deny(this.M.WorkEffortFixedAssetAssignment, finished, Operations.Write);
            config.Deny(this.M.WorkEffortFixedAssetAssignment, completed, Operations.Write);
        }
    }
}

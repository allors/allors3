
// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class WorkTaskDerivation : DomainDerivation
    {
        public WorkTaskDerivation(M m) : base(m, new Guid("12794dc5-8a79-4983-b480-4324602ae717")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.WorkTask.TakenBy),
            new RolePattern(m.WorkTask.ExecutedBy),
            new RolePattern(m.WorkTask.ActualStart),
            new RolePattern(m.WorkTask.WorkEffortState),
            new RolePattern(m.TimeEntry.FromDate) { Steps = new IPropertyType[] { m.TimeEntry.WorkEffort} },
            new RolePattern(m.TimeEntry.ThroughDate) { Steps = new IPropertyType[] { m.TimeEntry.WorkEffort} },
            new RolePattern(m.TimeEntry.WorkEffort) { Steps = new IPropertyType[] { m.TimeEntry.WorkEffort} },
            new RolePattern(m.TimeSheet.TimeEntries) { Steps = new IPropertyType[] { m.TimeSheet.TimeEntries, m.TimeEntry.WorkEffort} },
            new AssociationPattern(m.WorkEffortInventoryAssignment.Assignment),
            new RolePattern(m.WorkEffortInventoryAssignment.Quantity) { Steps = new IPropertyType[] { m.WorkEffortInventoryAssignment.Assignment } },
            new RolePattern(m.WorkEffortInventoryAssignment.InventoryItem) { Steps = new IPropertyType[] { m.WorkEffortInventoryAssignment.Assignment } },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkTask>())
            {
                if (@this.ExistCurrentVersion
                    && @this.CurrentVersion.ExistTakenBy
                    && @this.TakenBy != @this.CurrentVersion.TakenBy)
                {
                    validation.AddError($"{@this} {this.M.WorkTask.TakenBy} {ErrorMessages.InternalOrganisationChanged}");
                }

                @this.ResetPrintDocument();

                if (!@this.ExistWorkEffortNumber && @this.ExistTakenBy)
                {
                    var year = @this.Transaction().Now().Year;
                    @this.WorkEffortNumber = @this.TakenBy.NextWorkEffortNumber(year);

                    var fiscalYearInternalOrganisationSequenceNumbers = @this.TakenBy.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = @this.TakenBy.WorkEffortSequence.IsEnforcedSequence ? @this.TakenBy.WorkEffortNumberPrefix : fiscalYearInternalOrganisationSequenceNumbers.WorkEffortNumberPrefix;
                    @this.SortableWorkEffortNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.WorkEffortNumber, year.ToString());
                }

                if (!@this.ExistExecutedBy && @this.ExistTakenBy)
                {
                    @this.ExecutedBy = @this.TakenBy;
                }

                foreach (ServiceEntry serviceEntry in @this.ServiceEntriesWhereWorkEffort)
                {
                    if (serviceEntry is TimeEntry timeEntry)
                    {
                        var from = timeEntry.FromDate;
                        var through = timeEntry.ThroughDate;
                        var worker = timeEntry.TimeSheetWhereTimeEntry?.Worker;
                        var facility = timeEntry.WorkEffort.Facility;

                        var matchingAssignment = @this.WorkEffortPartyAssignmentsWhereAssignment.FirstOrDefault
                            (a => a.Assignment.Equals(@this)
                            && a.Party.Equals(worker)
                            && ((a.ExistFacility && a.Facility.Equals(facility)) || (!a.ExistFacility && facility == null))
                            && (!a.ExistFromDate || (a.ExistFromDate && (a.FromDate <= from)))
                            && (!a.ExistThroughDate || (a.ExistThroughDate && (a.ThroughDate >= through))));

                        if (matchingAssignment == null)
                        {
                            if (@this.TakenBy?.RequireExistingWorkEffortPartyAssignment == true)
                            {
                                var message = $"No Work Effort Party Assignment matches Worker: {worker}, Facility: {facility}" +
                                    $", Work Effort: {@this}, From: {from}, Through {through}";
                                validation.AddError($"{@this}, {@this.M.WorkEffort.WorkEffortPartyAssignmentsWhereAssignment}, {message}");
                            }
                            else if (worker != null) // Sync a new WorkEffortPartyAssignment
                            {
                                new WorkEffortPartyAssignmentBuilder(@this.Strategy.Transaction)
                                    .WithAssignment(@this)
                                    .WithParty(worker)
                                    .WithFacility(facility)
                                    .Build();
                            }
                        }
                    }
                }

                @this.ActualHours = 0M;

                foreach (ServiceEntry serviceEntry in @this.ServiceEntriesWhereWorkEffort)
                {
                    if (serviceEntry is TimeEntry timeEntry)
                    {
                        @this.ActualHours += timeEntry.ActualHours;

                        if (!@this.ExistActualStart)
                        {
                            @this.ActualStart = timeEntry.FromDate;
                        }
                        else if (timeEntry.FromDate < @this.ActualStart)
                        {
                            @this.ActualStart = timeEntry.FromDate;
                        }

                        if (!@this.ExistActualCompletion)
                        {
                            @this.ActualCompletion = timeEntry.ThroughDate;
                        }
                        else if (timeEntry.ThroughDate > @this.ActualCompletion)
                        {
                            @this.ActualCompletion = timeEntry.ThroughDate;
                        }
                    }
                }

                if (@this.ExistActualStart && @this.WorkEffortState.IsCreated)
                {
                    @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).InProgress;
                }

                if (@this.WorkEffortState.IsFinished && @this.CanInvoice)
                {
                    @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).Completed;
                }

                foreach (WorkEffortInventoryAssignment inventoryAssignment in @this.WorkEffortInventoryAssignmentsWhereAssignment)
                {
                    foreach (InventoryTransactionReason createReason in @this.WorkEffortState.InventoryTransactionReasonsToCreate)
                    {
                        SyncInventoryTransactions(validation, inventoryAssignment, inventoryAssignment.InventoryItem, inventoryAssignment.Quantity, createReason, false);
                    }

                    foreach (InventoryTransactionReason cancelReason in @this.WorkEffortState.InventoryTransactionReasonsToCancel)
                    {
                        SyncInventoryTransactions(validation,inventoryAssignment, inventoryAssignment.InventoryItem, inventoryAssignment.Quantity, cancelReason, true);
                    }
                }

                static void SyncInventoryTransactions(IDomainValidation validation,WorkEffortInventoryAssignment workEffortInventoryAssignment, InventoryItem inventoryItem, decimal initialQuantity, InventoryTransactionReason reason, bool isCancellation)
                {
                    var adjustmentQuantity = 0M;
                    var existingQuantity = 0M;
                    var matchingTransactions = workEffortInventoryAssignment.InventoryItemTransactions
                        .Where(t => t.Reason.Equals(reason) && t.Part.Equals(inventoryItem.Part)).ToArray();

                    if (matchingTransactions.Length > 0)
                    {
                        existingQuantity = matchingTransactions.Sum(t => t.Quantity);
                    }

                    if (isCancellation)
                    {
                        adjustmentQuantity = 0 - existingQuantity;
                    }
                    else
                    {
                        adjustmentQuantity = initialQuantity - existingQuantity;

                        if (inventoryItem is NonSerialisedInventoryItem nonserialisedInventoryItem && nonserialisedInventoryItem.QuantityOnHand < adjustmentQuantity)
                        {
                            validation.AddError($"{workEffortInventoryAssignment}, {workEffortInventoryAssignment.M.NonSerialisedInventoryItem.QuantityOnHand}, {ErrorMessages.InsufficientStock}");
                        }
                    }

                    if (adjustmentQuantity != 0)
                    {
                        workEffortInventoryAssignment.AddInventoryItemTransaction(new InventoryItemTransactionBuilder(workEffortInventoryAssignment.Transaction())
                            .WithPart(inventoryItem.Part)
                            .WithFacility(inventoryItem.Facility)
                            .WithQuantity(adjustmentQuantity)
                            .WithCost(inventoryItem.Part.PartWeightedAverage.AverageCost)
                            .WithReason(reason)
                            .Build());
                    }
                }
            }
        }
    }
}

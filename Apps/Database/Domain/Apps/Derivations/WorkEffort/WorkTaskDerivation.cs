// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Database.Derivations;
    using Resources;

    public class WorkTaskDerivation : DomainDerivation
    {
        public WorkTaskDerivation(M m) : base(m, new Guid("12794dc5-8a79-4983-b480-4324602ae717")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.WorkTask.TransitionalDeniedPermissions),
            new ChangedPattern(this.M.WorkTask.TakenBy),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkTask>())
            {
                @this.ResetPrintDocument();

                if (!@this.ExistDerivationTrigger)
                {
                    @this.DerivationTrigger = Guid.NewGuid();
                }

                var internalOrganisations = new Organisations(@this.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                if (!@this.ExistTakenBy && internalOrganisations.Count() == 1)
                {
                    @this.TakenBy = internalOrganisations.First();
                }

                if (!@this.ExistWorkEffortNumber && @this.ExistTakenBy)
                {
                    @this.WorkEffortNumber = @this.TakenBy.NextWorkEffortNumber();
                    @this.SortableWorkEffortNumber = NumberFormatter.SortableNumber(@this.TakenBy.WorkEffortPrefix, @this.WorkEffortNumber, @this.Strategy.Session.Now().Year.ToString());
                }

                if (!@this.ExistExecutedBy && @this.ExistTakenBy)
                {
                    @this.ExecutedBy = @this.TakenBy;
                }

                VerifyWorkEffortPartyAssignments(@this, validation);
                DeriveActualHoursAndDates(@this);

                if (@this.ExistActualStart && @this.WorkEffortState.IsCreated)
                {
                    @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Session).InProgress;
                }

                DeriveCanInvoice(@this);

                if (@this.WorkEffortState.IsFinished && @this.CanInvoice)
                {
                    @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Session).Completed;
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

                static void DeriveCanInvoice(WorkEffort @this)
                {
                    // when proforma invoice is deleted then WorkEffortBillingsWhereWorkEffort do not exist and WorkEffortState is Finished
                    if (@this.WorkEffortState.Equals(new WorkEffortStates(@this.Strategy.Session).Completed)
                        || @this.WorkEffortState.Equals(new WorkEffortStates(@this.Strategy.Session).Finished))
                    {
                        @this.CanInvoice = true;

                        if (@this.ExistWorkEffortBillingsWhereWorkEffort)
                        {
                            @this.CanInvoice = false;
                        }

                        if (@this.CanInvoice)
                        {
                            foreach (TimeEntry timeEntry in @this.ServiceEntriesWhereWorkEffort)
                            {
                                if (!timeEntry.ExistThroughDate)
                                {
                                    @this.CanInvoice = false;
                                    break;
                                }

                                if (timeEntry.ExistTimeEntryBillingsWhereTimeEntry)
                                {
                                    @this.CanInvoice = false;
                                }
                            }
                        }

                        if (@this.ExistWorkEffortWhereChild)
                        {
                            @this.CanInvoice = false;
                        }

                        if (@this.CanInvoice)
                        {
                            foreach (WorkEffort child in @this.Children)
                            {
                                if (!(child.WorkEffortState.Equals(new WorkEffortStates(@this.Strategy.Session).Completed)
                                    || child.WorkEffortState.Equals(new WorkEffortStates(@this.Strategy.Session).Finished)))
                                {
                                    @this.CanInvoice = false;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        @this.CanInvoice = false;
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
                        workEffortInventoryAssignment.AddInventoryItemTransaction(new InventoryItemTransactionBuilder(workEffortInventoryAssignment.Session())
                            .WithPart(inventoryItem.Part)
                            .WithFacility(inventoryItem.Facility)
                            .WithQuantity(adjustmentQuantity)
                            .WithCost(inventoryItem.Part.PartWeightedAverage.AverageCost)
                            .WithReason(reason)
                            .Build());
                    }
                }

                static void DeriveActualHoursAndDates(WorkEffort @this)
                {
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
                }

                static void VerifyWorkEffortPartyAssignments(WorkEffort @this, IDomainValidation validation)
                {
                    var m = @this.Strategy.Session.Database.State().M;

                    var existingAssignmentRequired = @this.TakenBy?.RequireExistingWorkEffortPartyAssignment == true;
                    var existingAssignments = @this.WorkEffortPartyAssignmentsWhereAssignment.ToArray();

                    foreach (ServiceEntry serviceEntry in @this.ServiceEntriesWhereWorkEffort)
                    {
                        if (serviceEntry is TimeEntry timeEntry)
                        {
                            var from = timeEntry.FromDate;
                            var through = timeEntry.ThroughDate;
                            var worker = timeEntry.TimeSheetWhereTimeEntry?.Worker;
                            var facility = timeEntry.WorkEffort.Facility;

                            var matchingAssignment = existingAssignments.FirstOrDefault
                                (a => a.Assignment.Equals(@this)
                                && a.Party.Equals(worker)
                                && ((a.ExistFacility && a.Facility.Equals(facility)) || (!a.ExistFacility && facility == null))
                                && (!a.ExistFromDate || (a.ExistFromDate && (a.FromDate <= from)))
                                && (!a.ExistThroughDate || (a.ExistThroughDate && (a.ThroughDate >= through))));

                            if (matchingAssignment == null)
                            {
                                if (existingAssignmentRequired)
                                {
                                    var message = $"No Work Effort Party Assignment matches Worker: {worker}, Facility: {facility}" +
                                        $", Work Effort: {@this}, From: {from}, Through {through}";
                                    validation.AddError($"{@this}, {m.WorkEffort.WorkEffortPartyAssignmentsWhereAssignment}, {message}");
                                }
                                else if (worker != null) // Sync a new WorkEffortPartyAssignment
                                {
                                    new WorkEffortPartyAssignmentBuilder(@this.Strategy.Session)
                                        .WithAssignment(@this)
                                        .WithParty(worker)
                                        .WithFacility(facility)
                                        .Build();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

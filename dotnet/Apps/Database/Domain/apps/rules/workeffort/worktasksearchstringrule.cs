// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class WorkTaskSearchStringRule : Rule
    {
        public WorkTaskSearchStringRule(MetaPopulation m) : base(m, new Guid("e7549768-280e-480b-8f33-c2f3006dc1e5")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffort.RolePattern(v => v.WorkEffortNumber, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.Name, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.Description, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.WorkDone, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.SpecialTerms, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.WorkEffortState, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.WorkEffortType, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.TakenBy, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.ExecutedBy, m.WorkTask),
                m.Organisation.RolePattern(v => v.DisplayName, v => v.WorkEffortsWhereExecutedBy.WorkEffort, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.Customer, m.WorkTask),
                m.Party.RolePattern(v => v.DisplayName, v => v.WorkEffortsWhereCustomer.WorkEffort, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.Facility, m.WorkTask),
                m.Facility.RolePattern(v => v.Name, v => v.WorkEffortsWhereFacility.WorkEffort, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.FullfillContactMechanism, m.WorkTask),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.WorkEffortsWhereFullfillContactMechanism.WorkEffort, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.ContactPerson, m.WorkTask),
                m.Person.RolePattern(v => v.DisplayName, v => v.WorkEffortsWhereContactPerson.WorkEffort, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.Owner, m.WorkTask),
                m.Person.RolePattern(v => v.DisplayName, v => v.WorkEffortsWhereOwner.WorkEffort, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.Priority, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.WorkEffortPurposes, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.Children, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.DeliverablesProduced, m.WorkTask),
                m.Deliverable.RolePattern(v => v.Name, v => v.WorkEffortWhereDeliverablesProduced.WorkEffort, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.OrderItemFulfillment, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.PrivateElectronicDocuments, m.WorkTask),
                m.WorkEffort.RolePattern(v => v.PublicElectronicDocuments, m.WorkTask),

                m.WorkEffort.AssociationPattern(v => v.CommunicationEventsWhereWorkEffort, m.WorkTask),
                m.WorkEffort.AssociationPattern(v => v.QuoteItemsWhereWorkEffort, m.WorkTask),
                m.WorkEffort.AssociationPattern(v => v.SalesInvoicesWhereWorkEffort, m.WorkTask),
                m.WorkEffort.AssociationPattern(v => v.WorkEffortFixedAssetAssignmentsWhereAssignment, m.WorkTask),
                m.WorkEffort.AssociationPattern(v => v.WorkEffortAssignmentRatesWhereWorkEffort, m.WorkTask),
                m.WorkEffort.AssociationPattern(v => v.WorkEffortPartyAssignmentsWhereAssignment, m.WorkTask),
                m.WorkEffort.AssociationPattern(v => v.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment, m.WorkTask),
                m.WorkEffort.AssociationPattern(v => v.WorkRequirementFulfillmentsWhereFullfillmentOf, m.WorkTask),
                m.WorkEffort.AssociationPattern(v => v.WorkEffortInventoryAssignmentsWhereAssignment, m.WorkTask),
                m.Part.RolePattern(v => v.DisplayName, v => v.InventoryItemsWherePart.InventoryItem.WorkEffortInventoryAssignmentsWhereInventoryItem.WorkEffortInventoryAssignment.Assignment.WorkEffort, m.WorkTask),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkTask>())
            {
                var array = new string[] {
                    @this.WorkEffortNumber,
                    @this.Name,
                    @this.Description,
                    @this.WorkDone,
                    @this.SpecialTerms,
                    @this.WorkEffortState?.Name,
                    @this.WorkEffortType?.Name,
                    @this.WorkEffortNumber,
                    @this.TakenBy?.DisplayName,
                    @this.ExecutedBy?.DisplayName,
                    @this.Customer?.DisplayName,
                    @this.Facility?.Name,
                    @this.FullfillContactMechanism?.DisplayName,
                    @this.ContactPerson?.DisplayName,
                    @this.Owner?.DisplayName,
                    @this.Priority?.Name,
                    @this.OrderItemFulfillment?.OrderWhereValidOrderItem?.OrderNumber,
                    string.Join(" ", @this.WorkEffortPurposes?.Select(v => v.Name)),
                    string.Join(" ", @this.Children?.Select(v => v.WorkEffortNumber)),
                    string.Join(" ", @this.DeliverablesProduced?.Select(v => v.Name)),
                    string.Join(" ", @this.DeliverablesProduced?.Select(v => v.DeliverableType?.Name)),
                    string.Join(" ", @this.PrivateElectronicDocuments?.Select(v => v.Name)),
                    string.Join(" ", @this.PublicElectronicDocuments?.Select(v => v.Name)),
                    string.Join(" ", @this.CommunicationEventsWhereWorkEffort?.Select(v => v.InvolvedParties?.Select(v => v.DisplayName))),
                    string.Join(" ", @this.QuoteItemsWhereWorkEffort?.Select(v => v.QuoteWhereQuoteItem?.QuoteNumber)),
                    string.Join(" ", @this.SalesInvoicesWhereWorkEffort?.Select(v => v.InvoiceNumber)),
                    string.Join(" ", @this.WorkEffortFixedAssetAssignmentsWhereAssignment?.Select(v => v.FixedAsset?.DisplayName)),
                    string.Join(" ", @this.WorkEffortAssignmentRatesWhereWorkEffort?.Select(v => v.RateType?.Name)),
                    string.Join(" ", @this.WorkEffortPartyAssignmentsWhereAssignment?.Select(v => v.Party?.DisplayName)),
                    string.Join(" ", @this.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment?.Select(v => v.PurchaseOrderItem?.PurchaseOrderWherePurchaseOrderItem.OrderNumber)),
                    string.Join(" ", @this.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment?.Select(v => v.PurchaseOrder.OrderNumber)),
                    string.Join(" ", @this.WorkRequirementFulfillmentsWhereFullfillmentOf?.Select(v => v.FullfilledBy.RequirementNumber)),
                    string.Join(" ", @this.WorkEffortInventoryAssignmentsWhereAssignment?.Select(v => v.InventoryItem?.Part.DisplayName)),
                };

                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
        }
    }
}

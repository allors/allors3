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

    public class PersonDeniedPermissionRule : Rule
    {
        public PersonDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("a88d6239-030c-47a2-8995-e9cbb83d20c7")) =>
            this.Patterns = new Pattern[]
        {
            new AssociationPattern(m.TimeSheet.Worker) { OfType = m.Person },
            new AssociationPattern(m.ExternalAccountingTransaction.FromParty) { OfType = m.Person },
            new AssociationPattern(m.ExternalAccountingTransaction.ToParty) { OfType = m.Person },
            new AssociationPattern(m.Shipment.ShipFromParty) { OfType = m.Person },
            new AssociationPattern(m.Shipment.ShipToParty) { OfType = m.Person },
            new AssociationPattern(m.Payment.Receiver) { OfType = m.Person },
            new AssociationPattern(m.Payment.Sender) { OfType = m.Person },
            new AssociationPattern(m.Engagement.BillToParty) { OfType = m.Person },
            new AssociationPattern(m.Engagement.PlacingParty) { OfType = m.Person },
            new AssociationPattern(m.UnifiedGood.ManufacturedBy) { OfType = m.Person },
            new AssociationPattern(m.UnifiedGood.SuppliedBy) { OfType = m.Person },
            new AssociationPattern(m.PartyFixedAssetAssignment.Party) { OfType = m.Person },
            new AssociationPattern(m.PickList.ShipToParty) { OfType = m.Person },
            new AssociationPattern(m.Quote.Receiver) { OfType = m.Person },
            new AssociationPattern(m.PurchaseInvoice.BilledFrom) { OfType = m.Person },
            new AssociationPattern(m.PurchaseInvoice.ShipToCustomer) { OfType = m.Person },
            new AssociationPattern(m.PurchaseInvoice.BillToEndCustomer) { OfType = m.Person },
            new AssociationPattern(m.PurchaseInvoice.ShipToEndCustomer) { OfType = m.Person },
            new AssociationPattern(m.PurchaseOrder.TakenViaSupplier) { OfType = m.Person },
            new AssociationPattern(m.PurchaseOrder.TakenViaSubcontractor) { OfType = m.Person },
            new AssociationPattern(m.Request.Originator) { OfType = m.Person },
            new AssociationPattern(m.Requirement.Authorizer) { OfType = m.Person },
            new AssociationPattern(m.Requirement.NeededFor) { OfType = m.Person },
            new AssociationPattern(m.Requirement.Originator) { OfType = m.Person },
            new AssociationPattern(m.Requirement.ServicedBy) { OfType = m.Person },
            new AssociationPattern(m.SalesInvoice.BillToCustomer) { OfType = m.Person },
            new AssociationPattern(m.SalesInvoice.BillToEndCustomer) { OfType = m.Person },
            new AssociationPattern(m.SalesInvoice.ShipToCustomer) { OfType = m.Person },
            new AssociationPattern(m.SalesInvoice.ShipToEndCustomer) { OfType = m.Person },
            new AssociationPattern(m.SalesOrder.BillToCustomer) { OfType = m.Person },
            new AssociationPattern(m.SalesOrder.BillToEndCustomer) { OfType = m.Person },
            new AssociationPattern(m.SalesOrder.ShipToCustomer) { OfType = m.Person },
            new AssociationPattern(m.SalesOrder.ShipToEndCustomer) { OfType = m.Person },
            new AssociationPattern(m.SalesOrder.PlacingCustomer) { OfType = m.Person },
            new AssociationPattern(m.SalesOrderItem.AssignedShipToParty) { OfType = m.Person },
            new AssociationPattern(m.SerialisedItem.SuppliedBy) { OfType = m.Person },
            new AssociationPattern(m.SerialisedItem.OwnedBy) { OfType = m.Person },
            new AssociationPattern(m.SerialisedItem.RentedBy) { OfType = m.Person },
            new AssociationPattern(m.WorkEffort.Customer) { OfType = m.Person },
            new AssociationPattern(m.WorkEffortPartyAssignment.Party) { OfType = m.Person },
            new AssociationPattern(m.Cash.PersonResponsible) { OfType = m.Person },
            new AssociationPattern(m.CommunicationEvent.Owner) { OfType = m.Person },
            new AssociationPattern(m.EngagementItem.CurrentAssignedProfessional) { OfType = m.Person },
            new AssociationPattern(m.Employment.Employee) { OfType = m.Person },
            new AssociationPattern(m.EngineeringChange.Authorizer) { OfType = m.Person },
            new AssociationPattern(m.EngineeringChange.Designer) { OfType = m.Person },
            new AssociationPattern(m.EngineeringChange.Requestor) { OfType = m.Person },
            new AssociationPattern(m.EngineeringChange.Tester) { OfType = m.Person },
            new AssociationPattern(m.EventRegistration.Person) { OfType = m.Person },
            new AssociationPattern(m.OwnCreditCard.Owner) { OfType = m.Person },
            new AssociationPattern(m.PerformanceNote.Employee) { OfType = m.Person },
            new AssociationPattern(m.PerformanceNote.GivenByManager) { OfType = m.Person },
            new AssociationPattern(m.PerformanceReview.Employee) { OfType = m.Person },
            new AssociationPattern(m.PerformanceReview.Manager) { OfType = m.Person },
            new AssociationPattern(m.PickList.Picker) { OfType = m.Person },
            new AssociationPattern(m.PositionFulfillment.Person) { OfType = m.Person },
            new AssociationPattern(m.ProfessionalAssignment.Professional) { OfType = m.Person },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Person>())
            {
                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Delete);
                if (@this.IsDeletable)
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }
            }
        }
    }
}

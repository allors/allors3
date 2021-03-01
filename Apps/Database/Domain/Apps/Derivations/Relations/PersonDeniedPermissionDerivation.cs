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

    public class PersonDeniedPermissionDerivation : DomainDerivation
    {
        public PersonDeniedPermissionDerivation(M m) : base(m, new Guid("a88d6239-030c-47a2-8995-e9cbb83d20c7")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.TimeSheet.Worker) { OfType = m.Person.Class },
            new RolePattern(m.ExternalAccountingTransaction.FromParty) { OfType = m.Person.Class },
            new RolePattern(m.ExternalAccountingTransaction.ToParty) { OfType = m.Person.Class },
            new RolePattern(m.Shipment.ShipFromParty) { OfType = m.Person.Class },
            new RolePattern(m.Shipment.ShipToParty) { OfType = m.Person.Class },
            new RolePattern(m.Payment.Receiver) { OfType = m.Person.Class },
            new RolePattern(m.Payment.Sender) { OfType = m.Person.Class },
            new RolePattern(m.Engagement.BillToParty) { OfType = m.Person.Class },
            new RolePattern(m.Engagement.PlacingParty) { OfType = m.Person.Class },
            new RolePattern(m.UnifiedGood.ManufacturedBy) { OfType = m.Person.Class },
            new RolePattern(m.UnifiedGood.SuppliedBy) { OfType = m.Person.Class },
            new RolePattern(m.PartyFixedAssetAssignment.Party) { OfType = m.Person.Class },
            new RolePattern(m.PickList.ShipToParty) { OfType = m.Person.Class },
            new RolePattern(m.Quote.Receiver) { OfType = m.Person.Class },
            new RolePattern(m.PurchaseInvoice.BilledFrom) { OfType = m.Person.Class },
            new RolePattern(m.PurchaseInvoice.ShipToCustomer) { OfType = m.Person.Class },
            new RolePattern(m.PurchaseInvoice.BillToEndCustomer) { OfType = m.Person.Class },
            new RolePattern(m.PurchaseInvoice.ShipToEndCustomer) { OfType = m.Person.Class },
            new RolePattern(m.PurchaseOrder.TakenViaSupplier) { OfType = m.Person.Class },
            new RolePattern(m.PurchaseOrder.TakenViaSubcontractor) { OfType = m.Person.Class },
            new RolePattern(m.Request.Originator) { OfType = m.Person.Class },
            new RolePattern(m.Requirement.Authorizer) { OfType = m.Person.Class },
            new RolePattern(m.Requirement.NeededFor) { OfType = m.Person.Class },
            new RolePattern(m.Requirement.Originator) { OfType = m.Person.Class },
            new RolePattern(m.Requirement.ServicedBy) { OfType = m.Person.Class },
            new RolePattern(m.SalesInvoice.BillToCustomer) { OfType = m.Person.Class },
            new RolePattern(m.SalesInvoice.BillToEndCustomer) { OfType = m.Person.Class },
            new RolePattern(m.SalesInvoice.ShipToCustomer) { OfType = m.Person.Class },
            new RolePattern(m.SalesInvoice.ShipToEndCustomer) { OfType = m.Person.Class },
            new RolePattern(m.SalesOrder.BillToCustomer) { OfType = m.Person.Class },
            new RolePattern(m.SalesOrder.BillToEndCustomer) { OfType = m.Person.Class },
            new RolePattern(m.SalesOrder.ShipToCustomer) { OfType = m.Person.Class },
            new RolePattern(m.SalesOrder.ShipToEndCustomer) { OfType = m.Person.Class },
            new RolePattern(m.SalesOrder.PlacingCustomer) { OfType = m.Person.Class },
            new RolePattern(m.SalesOrderItem.AssignedShipToParty) { OfType = m.Person.Class },
            new RolePattern(m.SerialisedItem.SuppliedBy) { OfType = m.Person.Class },
            new RolePattern(m.SerialisedItem.OwnedBy) { OfType = m.Person.Class },
            new RolePattern(m.SerialisedItem.RentedBy) { OfType = m.Person.Class },
            new RolePattern(m.WorkEffort.Customer) { OfType = m.Person.Class },
            new RolePattern(m.WorkEffortPartyAssignment.Party) { OfType = m.Person.Class },
            new RolePattern(m.Cash.PersonResponsible) { OfType = m.Person.Class },
            new RolePattern(m.CommunicationEvent.Owner) { OfType = m.Person.Class },
            new RolePattern(m.EngagementItem.CurrentAssignedProfessional) { OfType = m.Person.Class },
            new RolePattern(m.Employment.Employee) { OfType = m.Person.Class },
            new RolePattern(m.EngineeringChange.Authorizer) { OfType = m.Person.Class },
            new RolePattern(m.EngineeringChange.Designer) { OfType = m.Person.Class },
            new RolePattern(m.EngineeringChange.Requestor) { OfType = m.Person.Class },
            new RolePattern(m.EngineeringChange.Tester) { OfType = m.Person.Class },
            new RolePattern(m.EventRegistration.Person) { OfType = m.Person.Class },
            new RolePattern(m.OwnCreditCard.Owner) { OfType = m.Person.Class },
            new RolePattern(m.PerformanceNote.Employee) { OfType = m.Person.Class },
            new RolePattern(m.PerformanceNote.GivenByManager) { OfType = m.Person.Class },
            new RolePattern(m.PerformanceReview.Employee) { OfType = m.Person.Class },
            new RolePattern(m.PerformanceReview.Manager) { OfType = m.Person.Class },
            new RolePattern(m.PickList.Picker) { OfType = m.Person.Class },
            new RolePattern(m.PositionFulfillment.Person) { OfType = m.Person.Class },
            new RolePattern(m.ProfessionalAssignment.Professional) { OfType = m.Person.Class },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Person>())
            {
                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta.ObjectType, @this.Meta.Delete);
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

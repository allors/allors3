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
            m.Person.AssociationPattern(v => v.TimeSheetWhereWorker, null , m.Person),
            m.Party.AssociationPattern(v => v.ExternalAccountingTransactionsWhereFromParty, null , m.Person),
            m.Party.AssociationPattern(v => v.ExternalAccountingTransactionsWhereToParty, null , m.Person),
            m.Party.AssociationPattern(v => v.ShipmentsWhereShipFromParty, null , m.Person),
            m.Party.AssociationPattern(v => v.ShipmentsWhereShipToParty, null , m.Person),
            m.Party.AssociationPattern(v => v.PaymentsWhereReceiver, null , m.Person),
            m.Party.AssociationPattern(v => v.PaymentsWhereSender, null , m.Person),
            m.Party.AssociationPattern(v => v.EngagementsWhereBillToParty, null , m.Person),
            m.Party.AssociationPattern(v => v.EngagementsWherePlacingParty, null , m.Person),
            m.Party.AssociationPattern(v => v.PartsWhereManufacturedBy, null , m.Person),
            m.Party.AssociationPattern(v => v.PartsWhereSuppliedBy, null , m.Person),
            m.Party.AssociationPattern(v => v.PartyFixedAssetAssignmentsWhereParty, null , m.Person),
            m.Party.AssociationPattern(v => v.PickListsWhereShipToParty, null , m.Person),
            m.Party.AssociationPattern(v => v.QuotesWhereReceiver, null , m.Person),
            m.Party.AssociationPattern(v => v.PurchaseInvoicesWhereBilledFrom, null , m.Person),
            m.Party.AssociationPattern(v => v.PurchaseInvoicesWhereShipToCustomer, null , m.Person),
            m.Party.AssociationPattern(v => v.PurchaseInvoicesWhereBillToEndCustomer, null , m.Person),
            m.Party.AssociationPattern(v => v.PurchaseInvoicesWhereShipToEndCustomer, null , m.Person),
            m.Organisation.AssociationPattern(v => v.PurchaseOrdersWhereTakenViaSupplier, null , m.Person),
            m.Organisation.AssociationPattern(v => v.PurchaseOrdersWhereTakenViaSubcontractor, null , m.Person),
            m.Party.AssociationPattern(v => v.RequestsWhereOriginator, null , m.Person),
            m.Party.AssociationPattern(v => v.RequirementsWhereAuthorizer, null , m.Person),
            m.Party.AssociationPattern(v => v.RequirementsWhereNeededFor, null , m.Person),
            m.Party.AssociationPattern(v => v.RequirementsWhereOriginator, null , m.Person),
            m.Party.AssociationPattern(v => v.RequirementsWhereServicedBy, null , m.Person),
            m.Party.AssociationPattern(v => v.RequirementsWhereServicedBy, null , m.Person),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereBillToCustomer, null , m.Person),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereBillToEndCustomer, null , m.Person),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereShipToCustomer, null , m.Person),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereShipToEndCustomer, null , m.Person),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereBillToCustomer, null , m.Person),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereBillToEndCustomer, null , m.Person),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereShipToCustomer, null , m.Person),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereShipToEndCustomer, null , m.Person),
            m.Party.AssociationPattern(v => v.SalesOrdersWherePlacingCustomer, null , m.Person),
            m.Party.AssociationPattern(v => v.SalesOrderItemsWhereAssignedShipToParty, null , m.Person),
            m.Party.AssociationPattern(v => v.SerialisedItemsWhereSuppliedBy, null , m.Person),
            m.Party.AssociationPattern(v => v.SerialisedItemsWhereOwnedBy, null , m.Person),
            m.Party.AssociationPattern(v => v.SerialisedItemsWhereRentedBy, null , m.Person),
            m.Party.AssociationPattern(v => v.WorkEffortsWhereCustomer, null , m.Person),
            m.Party.AssociationPattern(v => v.WorkEffortPartyAssignmentsWhereParty, null , m.Person),
            m.Person.AssociationPattern(v => v.CashesWherePersonResponsible, null , m.Person),
            m.Person.AssociationPattern(v => v.CommunicationEventsWhereOwner, null , m.Person),
            m.Person.AssociationPattern(v => v.EngagementItemsWhereCurrentAssignedProfessional, null , m.Person),
            m.Person.AssociationPattern(v => v.EmploymentsWhereEmployee, null , m.Person),
            m.Person.AssociationPattern(v => v.EngineeringChangesWhereAuthorizer, null , m.Person),
            m.Person.AssociationPattern(v => v.EngineeringChangesWhereDesigner, null , m.Person),
            m.Person.AssociationPattern(v => v.EngineeringChangesWhereRequestor, null , m.Person),
            m.Person.AssociationPattern(v => v.EngineeringChangesWhereTester, null , m.Person),
            m.Person.AssociationPattern(v => v.EventRegistrationsWherePerson, null , m.Person),
            m.Person.AssociationPattern(v => v.OwnCreditCardsWhereOwner, null , m.Person),
            m.Person.AssociationPattern(v => v.PerformanceNotesWhereEmployee, null , m.Person),
            m.Person.AssociationPattern(v => v.PerformanceNotesWhereGivenByManager, null , m.Person),
            m.Person.AssociationPattern(v => v.PerformanceReviewsWhereEmployee, null , m.Person),
            m.Person.AssociationPattern(v => v.PerformanceReviewsWhereManager, null , m.Person),
            m.Person.AssociationPattern(v => v.PickListsWherePicker, null , m.Person),
            m.Person.AssociationPattern(v => v.PositionFulfillmentsWherePerson, null , m.Person),
            m.Person.AssociationPattern(v => v.ProfessionalAssignmentsWhereProfessional, null , m.Person),
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

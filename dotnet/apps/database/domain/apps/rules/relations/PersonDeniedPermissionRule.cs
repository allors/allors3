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
    using Derivations.Rules;

    public class PersonDeniedPermissionRule : Rule
    {
        public PersonDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("a88d6239-030c-47a2-8995-e9cbb83d20c7")) =>
            this.Patterns = new Pattern[]
        {
            m.Person.AssociationPattern(v => v.TimeSheetWhereWorker, m.Person),
            m.Party.AssociationPattern(v => v.ExternalAccountingTransactionsWhereFromParty, m.Person),
            m.Party.AssociationPattern(v => v.ExternalAccountingTransactionsWhereToParty, m.Person),
            m.Party.AssociationPattern(v => v.ShipmentsWhereShipFromParty, m.Person),
            m.Party.AssociationPattern(v => v.ShipmentsWhereShipToParty, m.Person),
            m.Party.AssociationPattern(v => v.PaymentsWhereReceiver, m.Person),
            m.Party.AssociationPattern(v => v.PaymentsWhereSender, m.Person),
            m.Party.AssociationPattern(v => v.EngagementsWhereBillToParty, m.Person),
            m.Party.AssociationPattern(v => v.EngagementsWherePlacingParty, m.Person),
            m.Party.AssociationPattern(v => v.PartsWhereManufacturedBy, m.Person),
            m.Party.AssociationPattern(v => v.PartsWhereSuppliedBy, m.Person),
            m.Party.AssociationPattern(v => v.PartyFixedAssetAssignmentsWhereParty, m.Person),
            m.Party.AssociationPattern(v => v.PickListsWhereShipToParty, m.Person),
            m.Party.AssociationPattern(v => v.QuotesWhereReceiver, m.Person),
            m.Party.AssociationPattern(v => v.PurchaseInvoicesWhereBilledFrom, m.Person),
            m.Party.AssociationPattern(v => v.PurchaseInvoicesWhereShipToCustomer, m.Person),
            m.Party.AssociationPattern(v => v.PurchaseInvoicesWhereBillToEndCustomer, m.Person),
            m.Party.AssociationPattern(v => v.PurchaseInvoicesWhereShipToEndCustomer, m.Person),
            m.Organisation.AssociationPattern(v => v.PurchaseOrdersWhereTakenViaSupplier, m.Person),
            m.Organisation.AssociationPattern(v => v.PurchaseOrdersWhereTakenViaSubcontractor, m.Person),
            m.Party.AssociationPattern(v => v.RequestsWhereOriginator, m.Person),
            m.Party.AssociationPattern(v => v.RequirementsWhereAuthorizer, m.Person),
            m.Party.AssociationPattern(v => v.RequirementsWhereNeededFor, m.Person),
            m.Party.AssociationPattern(v => v.RequirementsWhereOriginator, m.Person),
            m.Party.AssociationPattern(v => v.RequirementsWhereServicedBy, m.Person),
            m.Party.AssociationPattern(v => v.RequirementsWhereServicedBy, m.Person),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereBillToCustomer, m.Person),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereBillToEndCustomer, m.Person),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereShipToCustomer, m.Person),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereShipToEndCustomer, m.Person),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereBillToCustomer, m.Person),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereBillToEndCustomer, m.Person),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereShipToCustomer, m.Person),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereShipToEndCustomer, m.Person),
            m.Party.AssociationPattern(v => v.SalesOrdersWherePlacingCustomer, m.Person),
            m.Party.AssociationPattern(v => v.SalesOrderItemsWhereAssignedShipToParty, m.Person),
            m.Party.AssociationPattern(v => v.SerialisedItemsWhereSuppliedBy, m.Person),
            m.Party.AssociationPattern(v => v.SerialisedItemsWhereOwnedBy, m.Person),
            m.Party.AssociationPattern(v => v.SerialisedItemsWhereRentedBy, m.Person),
            m.Party.AssociationPattern(v => v.WorkEffortsWhereCustomer, m.Person),
            m.Party.AssociationPattern(v => v.WorkEffortPartyAssignmentsWhereParty, m.Person),
            m.Person.AssociationPattern(v => v.CashesWherePersonResponsible, m.Person),
            m.Person.AssociationPattern(v => v.CommunicationEventsWhereOwner, m.Person),
            m.Person.AssociationPattern(v => v.EngagementItemsWhereCurrentAssignedProfessional, m.Person),
            m.Person.AssociationPattern(v => v.EmploymentsWhereEmployee, m.Person),
            m.Person.AssociationPattern(v => v.EngineeringChangesWhereAuthorizer, m.Person),
            m.Person.AssociationPattern(v => v.EngineeringChangesWhereDesigner, m.Person),
            m.Person.AssociationPattern(v => v.EngineeringChangesWhereRequestor, m.Person),
            m.Person.AssociationPattern(v => v.EngineeringChangesWhereTester, m.Person),
            m.Person.AssociationPattern(v => v.EventRegistrationsWherePerson, m.Person),
            m.Person.AssociationPattern(v => v.OwnCreditCardsWhereOwner, m.Person),
            m.Person.AssociationPattern(v => v.PerformanceNotesWhereEmployee, m.Person),
            m.Person.AssociationPattern(v => v.PerformanceNotesWhereGivenByManager, m.Person),
            m.Person.AssociationPattern(v => v.PerformanceReviewsWhereEmployee, m.Person),
            m.Person.AssociationPattern(v => v.PerformanceReviewsWhereManager, m.Person),
            m.Person.AssociationPattern(v => v.PickListsWherePicker, m.Person),
            m.Person.AssociationPattern(v => v.PositionFulfillmentsWherePerson, m.Person),
            m.Person.AssociationPattern(v => v.ProfessionalAssignmentsWhereProfessional, m.Person),
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

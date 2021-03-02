// <copyright file="Person.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

    public partial class Person
    {
        public PrefetchPolicy PrefetchPolicy => new PrefetchPolicyBuilder()
            .WithRule(this.M.Person.OrganisationContactRelationshipsWhereContact)
            .WithRule(this.M.Person.PartyContactMechanisms)
            .WithRule(this.M.Person.TimeSheetWhereWorker)
            .WithRule(this.M.Person.EmploymentsWhereEmployee)
            .Build();

        public bool IsDeletable =>
            (!this.ExistTimeSheetWhereWorker || !this.TimeSheetWhereWorker.ExistTimeEntries)
            && !this.ExistExternalAccountingTransactionsWhereFromParty
            && !this.ExistExternalAccountingTransactionsWhereToParty
            && !this.ExistShipmentsWhereShipFromParty
            && !this.ExistShipmentsWhereShipToParty
            && !this.ExistPaymentsWhereReceiver
            && !this.ExistPaymentsWhereSender
            && !this.ExistEngagementsWhereBillToParty
            && !this.ExistEngagementsWherePlacingParty
            && !this.ExistPartsWhereManufacturedBy
            && !this.ExistPartsWhereSuppliedBy
            && !this.ExistPartyFixedAssetAssignmentsWhereParty
            && !this.ExistPickListsWhereShipToParty
            && !this.ExistQuotesWhereReceiver
            && !this.ExistPurchaseInvoicesWhereBilledFrom
            && !this.ExistPurchaseInvoicesWhereShipToCustomer
            && !this.ExistPurchaseInvoicesWhereBillToEndCustomer
            && !this.ExistPurchaseInvoicesWhereShipToEndCustomer
            && !this.ExistRequestsWhereOriginator
            && !this.ExistRequirementsWhereAuthorizer
            && !this.ExistRequirementsWhereNeededFor
            && !this.ExistRequirementsWhereOriginator
            && !this.ExistRequirementsWhereServicedBy
            && !this.ExistSalesInvoicesWhereBillToCustomer
            && !this.ExistSalesInvoicesWhereBillToEndCustomer
            && !this.ExistSalesInvoicesWhereShipToCustomer
            && !this.ExistSalesInvoicesWhereShipToEndCustomer
            && !this.ExistSalesOrdersWhereBillToCustomer
            && !this.ExistSalesOrdersWhereBillToEndCustomer
            && !this.ExistSalesOrdersWhereShipToCustomer
            && !this.ExistSalesOrdersWhereShipToEndCustomer
            && !this.ExistSalesOrdersWherePlacingCustomer
            && !this.ExistSalesOrderItemsWhereAssignedShipToParty
            && !this.ExistSerialisedItemsWhereSuppliedBy
            && !this.ExistSerialisedItemsWhereOwnedBy
            && !this.ExistSerialisedItemsWhereRentedBy
            && !this.ExistWorkEffortsWhereCustomer
            && !this.ExistWorkEffortPartyAssignmentsWhereParty
            && !this.ExistCashesWherePersonResponsible
            && !this.ExistCommunicationEventsWhereOwner
            && !this.ExistEngagementItemsWhereCurrentAssignedProfessional
            && !this.ExistEmploymentsWhereEmployee
            && !this.ExistEngineeringChangesWhereAuthorizer
            && !this.ExistEngineeringChangesWhereDesigner
            && !this.ExistEngineeringChangesWhereRequestor
            && !this.ExistEngineeringChangesWhereTester
            && !this.ExistEventRegistrationsWherePerson
            && !this.ExistOwnCreditCardsWhereOwner
            && !this.ExistPerformanceNotesWhereEmployee
            && !this.ExistPerformanceNotesWhereGivenByManager
            && !this.ExistPerformanceReviewsWhereEmployee
            && !this.ExistPerformanceReviewsWhereManager
            && !this.ExistPickListsWherePicker
            && !this.ExistPositionFulfillmentsWherePerson
            && !this.ExistProfessionalAssignmentsWhereProfessional;

        public bool AppsIsActiveEmployee(DateTime? date)
        {
            if (date == DateTime.MinValue)
            {
                return false;
            }

            return this.ExistEmploymentsWhereEmployee
                   && this.EmploymentsWhereEmployee
                       .Any(v => v.FromDate.Date <= date && (!v.ExistThroughDate || v.ThroughDate >= date));
        }

        public bool AppsIsActiveContact(DateTime? date)
        {
            if (date == DateTime.MinValue)
            {
                return false;
            }

            return this.ExistOrganisationContactRelationshipsWhereContact
                   && this.OrganisationContactRelationshipsWhereContact
                       .Any(v => v.FromDate.Date <= date && (!v.ExistThroughDate || v.ThroughDate >= date));
        }

        public void DeriveRelationships()
        {
            var now = this.Transaction().Now();
            var allOrganisationContactRelationships = this.OrganisationContactRelationshipsWhereContact;

            this.CurrentOrganisationContactRelationships = allOrganisationContactRelationships
                .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                .ToArray();

            this.InactiveOrganisationContactRelationships = allOrganisationContactRelationships
                .Except(this.CurrentOrganisationContactRelationships)
                .ToArray();
        }

        public void Sync(PartyContactMechanism[] organisationContactMechanisms)
        {
            foreach (var partyContactMechanism in organisationContactMechanisms)
            {
                this.RemoveCurrentOrganisationContactMechanism(partyContactMechanism.ContactMechanism);

                if (partyContactMechanism.FromDate <= this.Transaction().Now() &&
                    (!partyContactMechanism.ExistThroughDate || partyContactMechanism.ThroughDate >= this.Transaction().Now()))
                {
                    this.AddCurrentOrganisationContactMechanism(partyContactMechanism.ContactMechanism);
                }
            }
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (!this.IsDeletable)
            {
                return;
            }

            foreach (OrganisationContactRelationship deletable in this.OrganisationContactRelationshipsWhereContact)
            {
                deletable.Delete();
            }

            foreach (ProfessionalServicesRelationship deletable in this.ProfessionalServicesRelationshipsWhereProfessional)
            {
                deletable.Delete();
            }

            foreach (PartyFinancialRelationship deletable in this.PartyFinancialRelationshipsWhereFinancialParty)
            {
                deletable.Delete();
            }

            foreach (PartyContactMechanism deletable in this.PartyContactMechanisms)
            {
                var contactmechanism = deletable.ContactMechanism;

                deletable.Delete();

                if (!contactmechanism.ExistPartyContactMechanismsWhereContactMechanism)
                {
                    contactmechanism.Delete();
                }
            }

            foreach (CommunicationEvent deletable in this.CommunicationEventsWhereInvolvedParty)
            {
                deletable.Delete();
            }

            foreach (OrganisationContactRelationship deletable in this.OrganisationContactRelationshipsWhereContact)
            {
                deletable.Delete();
            }

            if (this.ExistTimeSheetWhereWorker)
            {
                this.TimeSheetWhereWorker.Delete();
            }

            if (this.ExistOwnerAccessControl)
            {
                this.OwnerAccessControl.Delete();
            }

            if (this.ExistOwnerSecurityToken)
            {
                this.OwnerSecurityToken.Delete();
            }
        }
    }
}

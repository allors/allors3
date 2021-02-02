// <copyright file="Organisation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class Organisation
    {
        public PrefetchPolicy PrefetchPolicy
        {
            get
            {
                var organisationContactRelationshipPrefetch = new PrefetchPolicyBuilder()
                    .WithRule(this.M.OrganisationContactRelationship.Contact)
                    .Build();

                var partyContactMechanismePrefetch = new PrefetchPolicyBuilder()
                    .WithRule(this.M.PartyContactMechanism.ContactMechanism)
                    .Build();

                return new PrefetchPolicyBuilder()
                    .WithRule(this.M.Organisation.RequestNumberCounter)
                    .WithRule(this.M.Organisation.QuoteNumberCounter)
                    .WithRule(this.M.Organisation.PurchaseInvoiceNumberCounter)
                    .WithRule(this.M.Organisation.PurchaseOrderNumberCounter)
                    .WithRule(this.M.Organisation.SubAccountCounter)
                    .WithRule(this.M.Organisation.IncomingShipmentNumberCounter)
                    .WithRule(this.M.Organisation.WorkEffortNumberCounter)
                    .WithRule(this.M.Organisation.InvoiceSequence)
                    .WithRule(this.M.Organisation.ContactsUserGroup)
                    .WithRule(this.M.Organisation.OrganisationContactRelationshipsWhereOrganisation, organisationContactRelationshipPrefetch)
                    .WithRule(this.M.Organisation.PartyContactMechanisms, partyContactMechanismePrefetch)
                    .WithRule(this.M.Organisation.CurrentContacts)
                    .Build();
            }
        }

        public List<string> Roles => new List<string>() { "Internal organisation" };

        public bool IsDeletable =>
            !this.IsInternalOrganisation
            && !this.ExistExternalAccountingTransactionsWhereFromParty
            && !this.ExistExternalAccountingTransactionsWhereToParty
            && !this.ExistShipmentsWhereShipFromParty
            && !this.ExistShipmentsWhereShipToParty
            && !this.ExistPaymentsWhereReceiver
            && !this.ExistPaymentsWhereSender
            && !this.ExistEmploymentsWhereEmployer
            && !this.ExistEngagementsWhereBillToParty
            && !this.ExistEngagementsWherePlacingParty
            && !this.ExistPartsWhereManufacturedBy
            && !this.ExistPartsWhereSuppliedBy
            && !this.ExistOrganisationGlAccountsWhereInternalOrganisation
            && !this.ExistOrganisationRollUpsWhereParent
            && !this.ExistPartyFixedAssetAssignmentsWhereParty
            && !this.ExistPickListsWhereShipToParty
            && !this.ExistQuotesWhereIssuer
            && !this.ExistQuotesWhereReceiver
            && !this.ExistPurchaseInvoicesWhereBilledTo
            && !this.ExistPurchaseInvoicesWhereBilledFrom
            && !this.ExistPurchaseInvoicesWhereShipToCustomer
            && !this.ExistPurchaseInvoicesWhereBillToEndCustomer
            && !this.ExistPurchaseInvoicesWhereShipToEndCustomer
            && !this.ExistPurchaseOrdersWhereTakenViaSupplier
            && !this.ExistPurchaseOrdersWhereTakenViaSubcontractor
            && !this.ExistRequestsWhereOriginator
            && !this.ExistRequestsWhereRecipient
            && !this.ExistRequirementsWhereAuthorizer
            && !this.ExistRequirementsWhereNeededFor
            && !this.ExistRequirementsWhereOriginator
            && !this.ExistRequirementsWhereServicedBy
            && !this.ExistSalesInvoicesWhereBilledFrom
            && !this.ExistSalesInvoicesWhereBillToCustomer
            && !this.ExistSalesInvoicesWhereBillToEndCustomer
            && !this.ExistSalesInvoicesWhereShipToCustomer
            && !this.ExistSalesInvoicesWhereShipToEndCustomer
            && !this.ExistSalesOrdersWhereBillToCustomer
            && !this.ExistSalesOrdersWhereBillToEndCustomer
            && !this.ExistSalesOrdersWhereShipToCustomer
            && !this.ExistSalesOrdersWhereShipToEndCustomer
            && !this.ExistSalesOrdersWherePlacingCustomer
            && !this.ExistSalesOrdersWhereTakenBy
            && !this.ExistSalesOrderItemsWhereAssignedShipToParty
            && !this.ExistSerialisedItemsWhereSuppliedBy
            && !this.ExistSerialisedItemsWhereOwnedBy
            && !this.ExistSerialisedItemsWhereRentedBy
            && !this.ExistSerialisedItemsWhereBuyer
            && !this.ExistSerialisedItemsWhereSeller
            && !this.ExistWorkEffortsWhereCustomer
            && !this.ExistWorkEffortsWhereExecutedBy
            && !this.ExistWorkEffortPartyAssignmentsWhereParty;

        public void DeriveRelationships()
        {
            var now = this.Session().Now();

            this.ActiveEmployees = this.EmploymentsWhereEmployer
                .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                .Select(v => v.Employee)
                .ToArray();

            this.ActiveCustomers = this.CustomerRelationshipsWhereInternalOrganisation
                .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                .Select(v => v.Customer)
                .ToArray();

            this.ActiveSuppliers = this.SupplierRelationshipsWhereInternalOrganisation
                .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                .Select(v => v.Supplier)
                .ToArray();

            this.ActiveSubContractors = this.SubContractorRelationshipsWhereContractor
                .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                .Select(v => v.SubContractor)
                .ToArray();

            var allContactRelationships = this.OrganisationContactRelationshipsWhereOrganisation.ToArray();
            var allContacts = allContactRelationships.Select(v => v.Contact);

            this.CurrentOrganisationContactRelationships = allContactRelationships
                .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                .ToArray();

            this.InactiveOrganisationContactRelationships = allContactRelationships
                .Except(this.CurrentOrganisationContactRelationships)
                .ToArray();

            this.CurrentContacts = this.CurrentOrganisationContactRelationships
                .Select(v => v.Contact).ToArray();

            this.InactiveContacts = this.InactiveOrganisationContactRelationships
                .Select(v => v.Contact)
                .ToArray();

            this.ContactsUserGroup.Members = this.CurrentContacts.ToArray();
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (this.IsDeletable)
            {
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

                foreach (OrganisationContactRelationship deletable in this.OrganisationContactRelationshipsWhereOrganisation)
                {
                    deletable.Contact.Delete();
                }

                foreach (PriceComponent deletable in this.PriceComponentsWherePricedBy)
                {
                    deletable.Delete();
                }

                foreach (CommunicationEvent deletable in this.CommunicationEventsWhereInvolvedParty)
                {
                    deletable.Delete();
                }

                foreach (CustomerRelationship deletable in this.CustomerRelationshipsWhereCustomer)
                {
                    deletable.Delete();
                }

                foreach (OrganisationRollUp deletable in this.OrganisationRollUpsWhereChild)
                {
                    deletable.Delete();
                }

                foreach (PartyFixedAssetAssignment deletable in this.PartyFixedAssetAssignmentsWhereParty)
                {
                    deletable.Delete();
                }

                foreach (ProfessionalServicesRelationship deletable in this.ProfessionalServicesRelationshipsWhereProfessionalServicesProvider)
                {
                    deletable.Delete();
                }

                foreach (SubContractorRelationship deletable in this.SubContractorRelationshipsWhereSubContractor)
                {
                    deletable.Delete();
                }

                foreach (SupplierRelationship deletable in this.SupplierRelationshipsWhereSupplier)
                {
                    deletable.Delete();
                }

                foreach (SupplierOffering deletable in this.SupplierOfferingsWhereSupplier)
                {
                    deletable.Delete();
                }
            }
        }
    }
}

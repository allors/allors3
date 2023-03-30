// <copyright file="Organisation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
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
                    .WithRule(this.M.Organisation.ProductQuoteNumberCounter)
                    .WithRule(this.M.Organisation.StatementOfWorkNumberCounter)
                    .WithRule(this.M.Organisation.PurchaseInvoiceNumberCounter)
                    .WithRule(this.M.Organisation.PurchaseOrderNumberCounter)
                    .WithRule(this.M.Organisation.PurchaseShipmentNumberCounter)
                    .WithRule(this.M.Organisation.WorkEffortNumberCounter)
                    .WithRule(this.M.Organisation.InvoiceSequence)
                    .WithRule(this.M.Organisation.ContactsUserGroup)
                    .WithRule(this.M.Organisation.OrganisationContactRelationshipsWhereOrganisation, organisationContactRelationshipPrefetch)
                    .WithRule(this.M.Organisation.PartyContactMechanismsWhereParty, partyContactMechanismePrefetch)
                    .WithRule(this.M.Organisation.CurrentContacts)
                    .Build();
            }
        }

        public List<string> Roles => new List<string>() { "Internal organisation" };

        public bool IsDeletable =>
            !this.ExistAccountingTransactionsWhereInternalOrganisation
            && !this.ExistAccountingTransactionsWhereFromParty
            && !this.ExistAccountingTransactionsWhereToParty
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
            && !this.ExistSerialisedItemsWhereAssignedSuppliedBy
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
            var now = this.Transaction().Now();

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

            var activeSupplierReleationships = this.SupplierRelationshipsWhereInternalOrganisation
                .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                .ToArray();

            this.InactiveSuppliers = this.SupplierRelationshipsWhereInternalOrganisation
                .Except(activeSupplierReleationships)
                .Select(v => v.Supplier)
                .ToArray();
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (this.IsDeletable)
            {
                foreach (var deletable in this.AllVersions)
                {
                    deletable.Strategy.Delete();
                }

                foreach (var deletable in this.StoresWhereInternalOrganisation)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.PartyFinancialRelationshipsWhereFinancialParty)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.PartyFinancialRelationshipsWhereInternalOrganisation)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.PartyContactMechanismsWhereParty)
                {
                    var contactmechanism = deletable.ContactMechanism;

                    deletable.CascadingDelete();

                    if (!contactmechanism.ExistPartyContactMechanismsWhereContactMechanism)
                    {
                        contactmechanism.CascadingDelete();
                    }
                }

                foreach (var deletable in this.OrganisationContactRelationshipsWhereOrganisation)
                {
                    deletable.Contact.CascadingDelete();
                }

                foreach (var deletable in this.PriceComponentsWherePricedBy)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.CommunicationEventsWhereInvolvedParty)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.CustomerRelationshipsWhereCustomer)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.CustomerRelationshipsWhereInternalOrganisation)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.OrganisationRollUpsWhereChild)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.PartyFixedAssetAssignmentsWhereParty)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.ProfessionalServicesRelationshipsWhereProfessionalServicesProvider)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.SubContractorRelationshipsWhereSubContractor)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.SubContractorRelationshipsWhereContractor)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.SupplierRelationshipsWhereSupplier)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.SupplierRelationshipsWhereInternalOrganisation)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.SupplierOfferingsWhereSupplier)
                {
                    deletable.CascadingDelete();
                }
            }
        }
    }
}

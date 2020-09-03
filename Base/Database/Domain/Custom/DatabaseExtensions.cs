// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public static class DatabaseExtensions
    {
        public static void RegisterDerivations(this @IDatabase @this)
        {
            var derivations = new IDomainDerivation[]
            {
                // Core
                new AuditableDerivation(),
                new MediaDerivation(),

                // Base
                new ShipmentDerivation(),
                new SupplierOfferingDerivation(),
                new ServiceDerivation(),
                new SerialisedItemCharacteristicDerivation(),
                new SerialisedInventoryItemDerivation(),
                new ProductCategoryDerivation(),
                new PriceComponentDerivation(),
                new PartCategoryDerivation(),
                new OrderValueDerivation(),
                new OrderQuantityBreakDerivation(),
                new NonUnifiedGoodDerivation(),
                new StatementOfWorkDerivation(),
                new SurchargeComponentDerivation(),
                new SerialisedItemDerivation(),
                new QuoteItemDerivation(),
                new PurchaseOrderApprovalLevel1Derivation(),
                new ProposalDerivation(),
                new ProductQuoteApprovalDerivation(),
                new ProductQuoteDerivation(),
                new OrderAdjustmentDerivation(),
                new EngagementDerivation(),
                new SalesOrderTransferDerivation(),
                new QuoteDerivation(),
                new PurchaseOrderApprovalLevel2Derivation(),
                new PurchaseReturnDerivation(),
                new PurchaseShipmentDerivation(),
                new ShipmentPackageDerivation(),
                new ShipmentValueDerivation(),
                new PickListItemDerivation(),
                new PickListDerivation(),
                new PackagingContentDerivation(),
                new DropShipmentDerivation(),
                new TransferDerivation(),
                new ShipmentReceiptDerivation(),
                new CustomerReturnDerivation(),
                new SalesOrderItemInventoryAssignmentDerivation(),
                new UnifiedGoodDerivation(),
                new NonSerialisedInventoryItemDerivation(),
                new NonUnifiedPartDerivation(),
                new PartDerivation(),
                new InventoryItemTransactionDerivation(),
                new InventoryItemDerivation(),
                new CatalogueDerivation(),
                new SingletonDerivation(),
                new SettingsDerivation(),
                new PhoneCommunicationDerivation(),
                new ProfessionalServicesRelationshipDerivation(),
                new OrganisationContactRelationshipDateDerivation(),
                new OrganisationContactRelationshipPartyDerivation(),
                new InternalOrganisationDerivation(),
                new FaceToFaceCommunicationDerivation(),
                new EmploymentDerivation(),
                new CommunicationTaskDerivation(),
                new CommunicationEventDerivation(),
                new AutomatedAgentDerivation(),
                new AgreementTermDerivation(),
                new EmailCommunicationDerivation(),
                new OrganisationDerivation(),
                new PersonDerivation(),
                new PartyDerivation(),
                new EmailTemplateDerivation(),
                new WebSiteCommunicationsDerivation(),
                new CustomerRelationshipDerivation(),
                new FaxCommunicationDerivation(),
                new LetterCorrespondenceDerivation(),
                new OrganisationRollupDerivation(),
                new PartyContactMechanismDerivation(),
                new SubcontractorRelationshipDerivation(),
                new PassportDerivation(),
                new RequestItemDerivation(),
                new RequestForQuoteDerivation(),
                new RequestForProposalDerivation(),
                new RequestForInformationDerivation(),
                new RequestDerivation(),
                new PurchaseInvoiceDerivation(),
                new PurchaseOrderItemDerivation(),
                new PaymentDerivation(),
                new PurchaseInvoiceApprovalDerivation(),
                new SalesInvoiceItemDerivation(),
                new RepeatingPurchaseInvoiceDerivation(),
                new RepeatingSalesInvoiceDerivation(),
                new SalesInvoiceDerivation(),
                new PartyFinancialRelationshipDerivation(),
                new PaymentApplicationDerivation(),
                new SupplierRelationshipDerivation(),
                new PurchaseOrderDerivation(), //Has Dependency on SupplierRelationship

                new ShipmentItemDerivation(),
                new CustomerShipmentDerivation(),
                new OrderShipmentDerivation(),
                new SalesOrderItemDerivation(), //Has Dependency on OrderShipment
                new SalesOrderDerivation(), //Has Dependency on SalesOrderItem

                new InvoiceItemsTotalIncVatDerivation(),
            };

            foreach (var derivation in derivations)
            {
                @this.DomainDerivationById.Add(derivation.Id, derivation);
            }
        }
    }
}

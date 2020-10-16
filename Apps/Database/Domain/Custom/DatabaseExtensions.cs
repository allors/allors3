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
            var m = @this.State().M;
            var derivations = new IDomainDerivation[]
            {
                // Core
                new AuditableDerivation(m),
                new MediaDerivation(m),

                // Apps
                new ShipmentDerivation(m),
                new SupplierOfferingDerivation(m),
                new ServiceDerivation(m),
                new SerialisedItemCharacteristicDerivation(m),
                new SerialisedInventoryItemDerivation(m),
                new ProductCategoryDerivation(m),
                new PriceComponentDerivation(m),
                new PartCategoryDerivation(m),
                new OrderValueDerivation(m),
                new OrderQuantityBreakDerivation(m),
                new NonUnifiedGoodDerivation(m),
                new StatementOfWorkDerivation(m),
                new SurchargeComponentDerivation(m),
                new SerialisedItemDerivation(m),
                new QuoteItemDerivation(m),
                new PurchaseOrderApprovalLevel1Derivation(m),
                new ProposalDerivation(m),
                new ProductQuoteApprovalDerivation(m),
                new ProductQuoteDerivation(m),
                new OrderAdjustmentDerivation(m),
                new EngagementDerivation(m),
                new SalesOrderTransferDerivation(m),
                new QuoteDerivation(m),
                new PurchaseOrderApprovalLevel2Derivation(m),
                new PurchaseReturnDerivation(m),
                new PurchaseShipmentDerivation(m),
                new ShipmentPackageDerivation(m),
                new ShipmentValueDerivation(m),
                new PickListItemDerivation(m),
                new PickListDerivation(m),
                new PackagingContentDerivation(m),
                new DropShipmentDerivation(m),
                new TransferDerivation(m),
                new ShipmentReceiptDerivation(m),
                new CustomerReturnDerivation(m),
                new SalesOrderItemInventoryAssignmentDerivation(m),
                new UnifiedGoodDerivation(m),
                new NonSerialisedInventoryItemDerivation(m),
                new NonUnifiedPartDerivation(m),
                new PartDerivation(m),
                new InventoryItemTransactionDerivation(m),
                new InventoryItemDerivation(m),
                new CatalogueDerivation(m),
                new SingletonCreationDerivation(m),
                new SingletonLocalesDerivation(m),
                new SettingsDerivation(m),
                new PhoneCommunicationDerivation(m),
                new ProfessionalServicesRelationshipDerivation(m),
                new OrganisationContactRelationshipDateDerivation(m),
                new OrganisationContactRelationshipPartyDerivation(m),
                new InternalOrganisationDerivation(m),
                new FaceToFaceCommunicationDerivation(m),
                new EmploymentDerivation(m),
                new CommunicationTaskDerivation(m),
                new CommunicationEventDerivation(m),
                new AutomatedAgentDerivation(m),
                new AgreementTermDerivation(m),
                new EmailCommunicationDerivation(m),
                new OrganisationDerivation(m),
                new PersonDerivation(m),
                new PartyDerivation(m),
                new EmailTemplateDerivation(m),
                new WebSiteCommunicationsDerivation(m),
                new CustomerRelationshipDerivation(m),
                new FaxCommunicationDerivation(m),
                new LetterCorrespondenceDerivation(m),
                new OrganisationRollupDerivation(m),
                new PartyContactMechanismDerivation(m),
                new SubcontractorRelationshipDerivation(m),
                new PassportDerivation(m),
                new RequestItemDerivation(m),
                new RequestForQuoteDerivation(m),
                new RequestForProposalDerivation(m),
                new RequestForInformationDerivation(m),
                new RequestDerivation(m),
                new PurchaseInvoiceDerivation(m),
                new PurchaseOrderItemDerivation(m),
                new PaymentDerivation(m),
                new PurchaseInvoiceApprovalDerivation(m),
                new SalesInvoiceItemDerivation(m),
                new RepeatingPurchaseInvoiceDerivation(m),
                new RepeatingSalesInvoiceDerivation(m),
                new SalesInvoiceDerivation(m),
                new PartyFinancialRelationshipCreationDerivation(m),
                new PartyFinancialRelationshipAmountDueDerivation(m),
                new PartyFinancialRelationshipOpenOrderAmountDerivation(m),
                new PaymentApplicationDerivation(m),
                new SupplierRelationshipDerivation(m),
                new PurchaseOrderDerivation(m), //Has Dependency on SupplierRelationship

                new ShipmentItemDerivation(m),
                new CustomerShipmentDerivation(m),
                new OrderShipmentDerivation(m),
                new SalesOrderItemDerivation(m), //Has Dependency on OrderShipment
                new SalesOrderDerivation(m), //Has Dependency on SalesOrderItem

                new InvoiceItemTotalIncVatDerivation(m),
            };

            foreach (var derivation in derivations)
            {
                @this.DomainDerivationById.Add(derivation.Id, derivation);
            }
        }
    }
}

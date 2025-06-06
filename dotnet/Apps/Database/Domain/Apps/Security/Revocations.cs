// <copyright file="Roles.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the role type.</summary>

namespace Allors.Database.Domain
{
    using System;

    public partial class Revocations
    {
        public static readonly Guid AccountingTransactionDeleteRevocationId = new Guid("fabbf918-8c77-470f-ac5a-96589c8cf006");
        public static readonly Guid BrandDeleteRevocationId = new Guid("d377ace1-8572-4994-b0d7-0a23b282983d");
        public static readonly Guid FacilityDeleteRevocationId = new Guid("786eef87-99a9-4183-ba4d-414a223e8a1d");
        public static readonly Guid GeneralLedgerAccountDeleteRevocationId = new Guid("75167ef5-ffa4-4541-ba98-aca868270fec");
        public static readonly Guid IrpfRateDeleteRevocationId = new Guid("b677a51d-25dc-4ae5-a257-1db6499e20d5");
        public static readonly Guid IrpfRegimeDeleteRevocationId = new Guid("2aa601f8-4f07-4c6c-afb7-a879cfffbb47");
        public static readonly Guid ModelDeleteRevocationId = new Guid("ba731036-0947-4534-a3c3-087c78059f78");
        public static readonly Guid NonUnifiedGoodDeleteRevocationId = new Guid("093a7d72-c9ad-422b-b04f-827305cd7296");
        public static readonly Guid NonUnifiedPartDeleteRevocationId = new Guid("5241e606-2246-435a-aeed-f819d094c3ae");
        public static readonly Guid OrganisationDeleteRevocationId = new Guid("52819a08-c8ac-4e5e-9048-d1c35ff0711c");
        public static readonly Guid OrganisationGlAccountDeleteRevocationId = new Guid("eb54baeb-19c6-4915-8cb7-89bd26f604b1");
        public static readonly Guid PersonDeleteRevocationId = new Guid("f7ce0f2b-14ef-4a4b-8dc9-5f8f9c4fb3c4");
        public static readonly Guid PersonResetPasswordRevocationId = new Guid("58c26f37-0c9b-4d97-811e-d22a5c557358");
        public static readonly Guid ProductQuoteDeleteRevocationId = new Guid("6e7fabdb-baa1-428b-bc5e-d7935a03e97a");
        public static readonly Guid ProductQuoteSetReadyForProcessingRevocationId = new Guid("30eb2b73-3885-4c11-b329-f49fb3d8c6a4");
        public static readonly Guid ProposalDeleteRevocationId = new Guid("8b267066-0ba9-46e3-bff2-d9ba63373edb");
        public static readonly Guid ProposalSetReadyForProcessingRevocationId = new Guid("eb02916f-33c8-4a2f-9dc2-206618aca4f2");
        public static readonly Guid PurchaseInvoiceCreateSalesInvoiceRevocationId = new Guid("b375129a-7248-4049-8c5a-78165e68d11e");
        public static readonly Guid PurchaseInvoiceDeleteRevocationId = new Guid("43c4f12b-de77-48d5-841f-5bc4249f035f");
        public static readonly Guid PurchaseOrderDeleteRevocationId = new Guid("572139d5-671f-4e33-a7f2-68ade7339295");
        public static readonly Guid PurchaseOrderInvoiceRevocationId = new Guid("caaafa47-b74d-4700-ac23-ee8d4ef44640");
        public static readonly Guid PurchaseOrderQuickReceiveRevocationId = new Guid("eea81a66-3f6f-47d2-a765-3a0a0b3a3633");
        public static readonly Guid PurchaseOrderReturnRevocationId = new Guid("adb5e04d-0059-4d2d-825a-9e122e6416cf");
        public static readonly Guid PurchaseOrderReviseRevocationId = new Guid("acd5a693-1f4e-4cf7-814e-e8c6bbcdc115");
        public static readonly Guid PurchaseOrderReceivedRevocationId = new Guid("4a5f52f7-ba5f-4193-a75a-23e069cc4dbb");
        public static readonly Guid PurchaseOrderReopenRevocationId = new Guid("5bae5f21-9aad-414b-85de-3383fd6cba29");
        public static readonly Guid PurchaseOrderWriteRevocationId = new Guid("99a3a62e-4367-4f13-851e-810fae4801a6");
        public static readonly Guid PurchaseOrderItemDeleteRevocationId = new Guid("c2b44a75-7d80-4399-aad9-aaa2fa3c9dc3");
        public static readonly Guid PurchaseOrderItemReturnRevocationId = new Guid("fe708ae3-54b6-4eea-a36d-5471ff3b42bc");
        public static readonly Guid PurchaseOrderItemQuickReceiveRevocationId = new Guid("eff5ef30-8827-41c9-afb2-9b41cd154101");
        public static readonly Guid PurchaseOrderItemWriteRevocationId = new Guid("23cb74f9-0abf-4efb-bec9-93733b203023");
        public static readonly Guid PurchaseOrderItemExecuteRevocationId = new Guid("388afd2b-08e5-4756-b3f9-de24e129a271");
        public static readonly Guid PurchaseReturnShipRevocationId = new Guid("8f66e782-c175-45e7-b997-028862c2b184");
        public static readonly Guid RequestForProposalDeleteRevocationId = new Guid("2e09cae8-c4fe-4938-88d1-55aad026a515");
        public static readonly Guid QuoteItemDeleteRevocationId = new Guid("c3a15312-bbf2-4b79-8052-cddd4ee52f6b");
        public static readonly Guid RepeatingSalesInvoiceDeleteRevocationId = new Guid("07ebab45-66ef-4b7b-b9b8-5cfb5c404d39");
        public static readonly Guid RequestForInformationDeleteRevocationId = new Guid("cfd6bb96-5acc-4b73-993b-70cb5e076de2");
        public static readonly Guid RequestForQuoteDeleteRevocationId = new Guid("5a5864e2-b452-4da1-9915-093b0250cd61");
        public static readonly Guid RequestForQuoteSubmitRevocationId = new Guid("3de464eb-acad-47e6-be0f-8ceb38a81abc");
        public static readonly Guid RequestItemDeleteRevocationId = new Guid("23aeb662-32f5-4a6d-9b7e-f6078914f7e4");
        public static readonly Guid SalesInvoiceCancelRevocationId = new Guid("e45c0710-4970-448d-8b1f-9bbab940189b");
        public static readonly Guid SalesInvoiceDeleteRevocationId = new Guid("8f07e179-9a61-4244-ad9f-53d1985ec639");
        public static readonly Guid SalesInvoiceReviseRevocationId = new Guid("6aee7ad9-a941-45eb-a081-67d8651f71a4");
        public static readonly Guid SalesInvoiceItemDeleteRevocationId = new Guid("69faf684-6ca3-4106-a9d1-7b9268c0893d");
        public static readonly Guid SalesOrderDeleteRevocationId = new Guid("c7841df0-9b8c-4667-8b21-70ec31453052");
        public static readonly Guid SalesOrderInvoiceRevocationId = new Guid("4c186591-08aa-4f8c-8ee2-e33a1fff3c23");
        public static readonly Guid SalesOrderShipRevocationId = new Guid("0f41b121-75e7-450d-8aaf-da624765ff30");
        public static readonly Guid SalesOrderStateRevocationId = new Guid("375354ff-ddc5-4596-9ce4-4ece3cd1007c");
        public static readonly Guid SalesOrderWriteRevocationId = new Guid("1f6f1ab5-8062-430b-82a7-4adb9c520daf");
        public static readonly Guid SalesOrderItemDeleteRevocationId = new Guid("7d0a1b60-4017-472d-b8f8-e0a9f2675c02");
        public static readonly Guid SalesOrderItemWriteRevocationId = new Guid("04258294-8044-4107-8703-84b3f679179b");
        public static readonly Guid SalesOrderItemExecuteRevocationId = new Guid("c6461cf1-27da-4163-874e-e78aa11dd08d");
        public static readonly Guid SerialisedItemDeleteRevocationId = new Guid("df9751a6-50e5-41d7-bbc1-befaf38b38ec");
        public static readonly Guid StatementOfWorkDeleteRevocationId = new Guid("a6d8c7dc-c849-4850-a37c-696bba468fb5");
        public static readonly Guid UnifiedGoodDeleteRevocationId = new Guid("c742b7f9-2310-4272-ab89-25898a7db3cc");
        public static readonly Guid VatClauseDeleteRevocationId = new Guid("4f9e78af-dfd7-4883-bf59-5bddc434294c");
        public static readonly Guid VatRateDeleteRevocationId = new Guid("44caf036-4880-4865-a163-73c5205c3238");
        public static readonly Guid VatRegimeDeleteRevocationId = new Guid("8affdb44-36cd-401a-a4fb-055b73b11213");
        public static readonly Guid WorkEffortInvoiceItemDeleteRevocationId = new Guid("81128223-025b-48fc-806f-caf3201cb595");
        public static readonly Guid WorkRequirementCancelRevocationId = new Guid("52706d60-64eb-424c-88bd-1e2be382bc84");
        public static readonly Guid WorkRequirementDeleteRevocationId = new Guid("22ab560a-e740-4cdd-b237-50e97fcf6dd2");
        public static readonly Guid WorkRequirementReopenRevocationId = new Guid("db9872f2-0cf7-4a73-a449-2b2c4a436a3f");
        public static readonly Guid WorkRequirementFulfillmentDeleteRevocationId = new Guid("09a087fe-a39d-4e42-a980-079ad348788e");
        public static readonly Guid WorkTaskCompleteRevocationId = new Guid("0ced8924-4efd-4f40-9055-22804ac51b39");
        public static readonly Guid WorkTaskInvoiceRevocationId = new Guid("9d200651-a109-4462-bf99-8a70c3c4afb3");
        public static readonly Guid WorkTaskReviseRevocationId = new Guid("d7367f95-dfe9-467c-a84f-8fad58374d57");

        public Revocation AccountingTransactionDeleteRevocation => this.Cache[AccountingTransactionDeleteRevocationId];

        public Revocation BrandDeleteRevocation => this.Cache[BrandDeleteRevocationId];

        public Revocation FacilityDeleteRevocation => this.Cache[FacilityDeleteRevocationId];

        public Revocation GeneralLedgerAccountDeleteRevocation => this.Cache[GeneralLedgerAccountDeleteRevocationId];

        public Revocation IrpfRateDeleteRevocation => this.Cache[IrpfRateDeleteRevocationId];

        public Revocation IrpfRegimeDeleteRevocation => this.Cache[IrpfRegimeDeleteRevocationId];

        public Revocation ModelDeleteRevocation => this.Cache[ModelDeleteRevocationId];

        public Revocation NonUnifiedGoodDeleteRevocation => this.Cache[NonUnifiedGoodDeleteRevocationId];

        public Revocation NonUnifiedPartDeleteRevocation => this.Cache[NonUnifiedPartDeleteRevocationId];

        public Revocation OrganisationDeleteRevocation => this.Cache[OrganisationDeleteRevocationId];

        public Revocation OrganisationGlAccountDeleteRevocation => this.Cache[OrganisationGlAccountDeleteRevocationId];

        public Revocation PersonDeleteRevocation => this.Cache[PersonDeleteRevocationId];

        public Revocation PersonResetPasswordRevocation => this.Cache[PersonResetPasswordRevocationId];

        public Revocation ProductQuoteDeleteRevocation => this.Cache[ProductQuoteDeleteRevocationId];

        public Revocation ProductQuoteSetReadyForProcessingRevocation => this.Cache[ProductQuoteSetReadyForProcessingRevocationId];

        public Revocation ProposalDeleteRevocation => this.Cache[ProposalDeleteRevocationId];

        public Revocation ProposalSetReadyForProcessingRevocation => this.Cache[ProposalSetReadyForProcessingRevocationId];

        public Revocation PurchaseInvoiceCreateSalesInvoiceRevocation => this.Cache[PurchaseInvoiceCreateSalesInvoiceRevocationId];

        public Revocation PurchaseInvoiceDeleteRevocation => this.Cache[PurchaseInvoiceDeleteRevocationId];

        public Revocation PurchaseOrderDeleteRevocation => this.Cache[PurchaseOrderDeleteRevocationId];

        public Revocation PurchaseOrderInvoiceRevocation => this.Cache[PurchaseOrderInvoiceRevocationId];

        public Revocation PurchaseOrderQuickReceiveRevocation => this.Cache[PurchaseOrderQuickReceiveRevocationId];

        public Revocation PurchaseOrderReturnRevocation => this.Cache[PurchaseOrderReturnRevocationId];

        public Revocation PurchaseOrderReviseRevocation => this.Cache[PurchaseOrderReviseRevocationId];

        public Revocation PurchaseOrderReceivedRevocation => this.Cache[PurchaseOrderReceivedRevocationId];

        public Revocation PurchaseOrderReopenRevocation => this.Cache[PurchaseOrderReopenRevocationId];

        public Revocation PurchaseOrderWriteRevocation => this.Cache[PurchaseOrderWriteRevocationId];

        public Revocation RequestForProposalDeleteRevocation => this.Cache[RequestForProposalDeleteRevocationId];

        public Revocation PurchaseOrderItemDeleteRevocation => this.Cache[PurchaseOrderItemDeleteRevocationId];

        public Revocation PurchaseOrderItemQuickReceiveRevocation => this.Cache[PurchaseOrderItemQuickReceiveRevocationId];

        public Revocation PurchaseOrderItemReturnRevocation => this.Cache[PurchaseOrderItemReturnRevocationId];

        public Revocation PurchaseOrderItemWriteRevocation => this.Cache[PurchaseOrderItemWriteRevocationId];

        public Revocation PurchaseOrderItemExecuteRevocation => this.Cache[PurchaseOrderItemExecuteRevocationId];

        public Revocation PurchaseReturnShipRevocation => this.Cache[PurchaseReturnShipRevocationId];

        public Revocation QuoteItemDeleteRevocation => this.Cache[QuoteItemDeleteRevocationId];

        public Revocation RepeatingSalesInvoiceDeleteRevocation => this.Cache[RepeatingSalesInvoiceDeleteRevocationId];

        public Revocation RequestForInformationDeleteRevocation => this.Cache[RequestForInformationDeleteRevocationId];

        public Revocation RequestForQuoteDeleteRevocation => this.Cache[RequestForQuoteDeleteRevocationId];

        public Revocation RequestForQuoteSubmitRevocation => this.Cache[RequestForQuoteSubmitRevocationId];

        public Revocation RequestItemDeleteRevocation => this.Cache[RequestItemDeleteRevocationId];

        public Revocation SalesInvoiceCancelRevocation => this.Cache[SalesInvoiceCancelRevocationId];

        public Revocation SalesInvoiceDeleteRevocation => this.Cache[SalesInvoiceDeleteRevocationId];

        public Revocation SalesInvoiceReviseRevocation => this.Cache[SalesInvoiceReviseRevocationId];

        public Revocation SalesInvoiceItemDeleteRevocation => this.Cache[SalesInvoiceItemDeleteRevocationId];

        public Revocation SalesOrderDeleteRevocation => this.Cache[SalesOrderDeleteRevocationId];

        public Revocation SalesOrderInvoiceRevocation => this.Cache[SalesOrderInvoiceRevocationId];

        public Revocation SalesOrderShipRevocation => this.Cache[SalesOrderShipRevocationId];

        public Revocation SalesOrderStateRevocation => this.Cache[SalesOrderStateRevocationId];

        public Revocation SalesOrderWriteRevocation => this.Cache[SalesOrderWriteRevocationId];

        public Revocation SalesOrderItemDeleteRevocation => this.Cache[SalesOrderItemDeleteRevocationId];

        public Revocation SalesOrderItemWriteRevocation => this.Cache[SalesOrderItemWriteRevocationId];

        public Revocation SalesOrderItemExecuteRevocation => this.Cache[SalesOrderItemExecuteRevocationId];

        public Revocation SerialisedItemDeleteRevocation => this.Cache[SerialisedItemDeleteRevocationId];

        public Revocation StatementOfWorkDeleteRevocation => this.Cache[StatementOfWorkDeleteRevocationId];

        public Revocation UnifiedGoodDeleteRevocation => this.Cache[UnifiedGoodDeleteRevocationId];

        public Revocation VatClauseDeleteRevocation => this.Cache[VatClauseDeleteRevocationId];

        public Revocation VatRateDeleteRevocation => this.Cache[VatRateDeleteRevocationId];

        public Revocation VatRegimeDeleteRevocation => this.Cache[VatRegimeDeleteRevocationId];

        public Revocation WorkEffortInvoiceItemDeleteRevocation => this.Cache[WorkEffortInvoiceItemDeleteRevocationId];

        public Revocation WorkRequirementCancelRevocation => this.Cache[WorkRequirementCancelRevocationId];

        public Revocation WorkRequirementDeleteRevocation => this.Cache[WorkRequirementDeleteRevocationId];

        public Revocation WorkRequirementReopenRevocation => this.Cache[WorkRequirementReopenRevocationId];

        public Revocation WorkRequirementFulfillmentDeleteRevocation => this.Cache[WorkRequirementFulfillmentDeleteRevocationId];

        public Revocation WorkTaskCompleteRevocation => this.Cache[WorkTaskCompleteRevocationId];

        public Revocation WorkTaskInvoiceRevocation => this.Cache[WorkTaskInvoiceRevocationId];

        public Revocation WorkTaskReviseRevocation => this.Cache[WorkTaskReviseRevocationId];

        protected override void AppsSecure(Security security)
        {
            var merge = this.Cache.Merger().Action();

            merge(AccountingTransactionDeleteRevocationId, _ => { });
            merge(BrandDeleteRevocationId, _ => { });
            merge(FacilityDeleteRevocationId, _ => { });
            merge(GeneralLedgerAccountDeleteRevocationId, _ => { });
            merge(IrpfRateDeleteRevocationId, _ => { });
            merge(IrpfRegimeDeleteRevocationId, _ => { });
            merge(ModelDeleteRevocationId, _ => { });
            merge(NonUnifiedGoodDeleteRevocationId, _ => { });
            merge(NonUnifiedPartDeleteRevocationId, _ => { });
            merge(OrganisationDeleteRevocationId, _ => { });
            merge(OrganisationGlAccountDeleteRevocationId, _ => { });
            merge(PersonDeleteRevocationId, _ => { });
            merge(PersonResetPasswordRevocationId, _ => { });
            merge(ProductQuoteDeleteRevocationId, _ => { });
            merge(ProductQuoteSetReadyForProcessingRevocationId, _ => { });
            merge(ProposalDeleteRevocationId, _ => { });
            merge(ProposalSetReadyForProcessingRevocationId, _ => { });
            merge(PurchaseInvoiceCreateSalesInvoiceRevocationId, _ => { });
            merge(PurchaseInvoiceDeleteRevocationId, _ => { });
            merge(PurchaseOrderDeleteRevocationId, _ => { });
            merge(PurchaseOrderInvoiceRevocationId, _ => { });
            merge(PurchaseOrderQuickReceiveRevocationId, _ => { });
            merge(PurchaseOrderReturnRevocationId, _ => { });
            merge(PurchaseOrderReviseRevocationId, _ => { });
            merge(PurchaseOrderReceivedRevocationId, _ => { });
            merge(PurchaseOrderReopenRevocationId, _ => { });
            merge(PurchaseOrderWriteRevocationId, _ => { });
            merge(RequestForProposalDeleteRevocationId, _ => { });
            merge(PurchaseOrderItemDeleteRevocationId, _ => { });
            merge(PurchaseOrderItemReturnRevocationId, _ => { });
            merge(PurchaseOrderItemQuickReceiveRevocationId, _ => { });
            merge(PurchaseOrderItemWriteRevocationId, _ => { });
            merge(PurchaseOrderItemExecuteRevocationId, _ => { });
            merge(PurchaseReturnShipRevocationId, _ => { });
            merge(QuoteItemDeleteRevocationId, _ => { });
            merge(RepeatingSalesInvoiceDeleteRevocationId, _ => { });
            merge(RequestForInformationDeleteRevocationId, _ => { });
            merge(RequestForQuoteDeleteRevocationId, _ => { });
            merge(RequestForQuoteSubmitRevocationId, _ => { });
            merge(RequestItemDeleteRevocationId, _ => { });
            merge(SalesInvoiceCancelRevocationId, _ => { });
            merge(SalesInvoiceDeleteRevocationId, _ => { });
            merge(SalesInvoiceReviseRevocationId, _ => { });
            merge(SalesInvoiceItemDeleteRevocationId, _ => { });
            merge(SalesOrderDeleteRevocationId, _ => { });
            merge(SalesOrderInvoiceRevocationId, _ => { });
            merge(SalesOrderShipRevocationId, _ => { });
            merge(SalesOrderStateRevocationId, _ => { });
            merge(SalesOrderWriteRevocationId, _ => { });
            merge(SalesOrderItemDeleteRevocationId, _ => { });
            merge(SalesOrderItemWriteRevocationId, _ => { });
            merge(SalesOrderItemExecuteRevocationId, _ => { });
            merge(SerialisedItemDeleteRevocationId, _ => { });
            merge(StatementOfWorkDeleteRevocationId, _ => { });
            merge(UnifiedGoodDeleteRevocationId, _ => { });
            merge(VatClauseDeleteRevocationId, _ => { });
            merge(VatRateDeleteRevocationId, _ => { });
            merge(VatRegimeDeleteRevocationId, _ => { });
            merge(WorkEffortInvoiceItemDeleteRevocationId, _ => { });
            merge(WorkRequirementCancelRevocationId, _ => { });
            merge(WorkRequirementDeleteRevocationId, _ => { });
            merge(WorkRequirementReopenRevocationId, _ => { });
            merge(WorkRequirementFulfillmentDeleteRevocationId, _ => { });
            merge(WorkTaskCompleteRevocationId, _ => { });
            merge(WorkTaskInvoiceRevocationId, _ => { });
            merge(WorkTaskReviseRevocationId, _ => { });
        }
    }
}

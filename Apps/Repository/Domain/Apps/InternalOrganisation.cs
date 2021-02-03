// <copyright file="InternalOrganisation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("c81441c8-9ac9-440e-a926-c96230b2701f")]
    #endregion
    public partial interface InternalOrganisation : Party
    {
        #region inherited properties

        #endregion

        #region Allors
        [Id("25E8BD32-807F-4484-8561-2AA34B425C6F")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool IsInternalOrganisation { get; set; }

        #region Allors
        [Id("5eaab214-d2d4-46e3-806c-5193047067e2")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        public FiscalYearInternalOrganisationSequenceNumbers[] FiscalYearsInternalOrganisationSequenceNumbers { get; set; }

        #region Allors
        [Id("E6BBEC1C-5855-4D22-97D2-BF62B853DC7E")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.OneToMany)]
        PaymentMethod[] PaymentMethods { get; set; }

        #region Allors
        [Id("356044B9-47FA-4EEB-955E-B75C2A21EA2E")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        PaymentMethod DefaultCollectionMethod { get; set; }

        #region Allors
        [Id("1a986cbf-b7db-4850-af06-d96e1339beb7")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        Counter PurchaseInvoiceNumberCounter { get; set; }

        #region Allors
        [Id("23aee857-9cea-481c-a4a3-72dd8b808d71")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        InvoiceSequence InvoiceSequence { get; set; }

        #region Allors
        [Id("63e05248-fb49-46c9-b7f1-98769fbfcc91")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        RequestSequence RequestSequence { get; set; }

        #region Allors
        [Id("5d7ea4f2-9b8b-4715-ada4-7e0489d4dee7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        QuoteSequence QuoteSequence { get; set; }

        #region Allors
        [Id("ccbc587c-f01c-4bd8-805c-1491f8e41f23")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        CustomerShipmentSequence CustomerShipmentSequence { get; set; }

        #region Allors
        [Id("5d758176-8dd7-4578-87a5-0f370801d7df")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        CustomerReturnSequence CustomerReturnSequence { get; set; }

        #region Allors
        [Id("b04166d7-cd76-40cc-984b-edfe25630f02")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        PurchaseShipmentSequence PurchaseShipmentSequence { get; set; }

        #region Allors
        [Id("f757fc1b-66b8-4f40-8f5d-a9388964ecb8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        PurchaseReturnSequence PurchaseReturnSequence { get; set; }

        #region Allors
        [Id("713ebf82-bce2-4c1d-a4cf-c77ae03f5d12")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        DropShipmentSequence DropShipmentSequence { get; set; }

        #region Allors
        [Id("98af108a-dad4-4034-b3c1-fd318d424d9e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        IncomingTransferSequence IncomingTransferSequence { get; set; }

        #region Allors
        [Id("902674e4-7e21-4935-a125-5cada12405d1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        OutgoingTransferSequence OutgoingTransferSequence { get; set; }

        #region Allors
        [Id("23bc3a20-aea2-4e05-b792-fdcf92def95b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        WorkEffortSequence WorkEffortSequence { get; set; }

        #region Allors
        [Id("44f165ed-a6ca-4979-9046-a0f7391bef7d")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        Counter PurchaseOrderNumberCounter { get; set; }

        #region Allors
        [Id("7e210c5e-a68b-4ea0-b019-1dd452d8e407")]
        #endregion
        [Workspace(Default)]
        int FiscalYearStartDay { get; set; }

        #region Allors
        [Id("ad7c8532-59d2-4668-bd9f-6c67ddc4e4bc")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        Counter SubAccountCounter { get; set; }

        #region Allors
        [Id("A49663B5-A432-41FA-BBCA-8368D1B9D53D")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        Counter RequestNumberCounter { get; set; }

        #region Allors
        [Id("5b64cf9d-e990-491e-b009-3481d73db67e")]
        #endregion
        [Workspace(Default)]
        int FiscalYearStartMonth { get; set; }

        #region Allors
        [Id("fe96e14b-9dbd-4497-935f-f605abd2ada7")]
        #endregion
        [Workspace(Default)]
        [Size(256)]
        string PurchaseShipmentNumberPrefix { get; set; }

        #region Allors
        [Id("86dd32e2-74e7-4ced-bfbd-4e1fdc723588")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        Counter PurchaseShipmentNumberCounter { get; set; }

        #region Allors
        [Id("fd8dcd59-cc0a-4b9a-bea1-2e3baff9dd8d")]
        #endregion
        [Workspace(Default)]
        [Size(256)]
        string CustomerReturnNumberPrefix { get; set; }

        #region Allors
        [Id("23c83f3f-1837-4fa9-9393-0a515b6c9daa")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        Counter CustomerReturnNumberCounter { get; set; }

        #region Allors
        [Id("280df890-7f5d-4ba9-9225-f22526e5883c")]
        #endregion
        [Workspace]
        [Size(256)]
        public string IncomingTransferNumberPrefix { get; set; }

        #region Allors
        [Id("799b3992-f189-4030-960f-4c40021430b0")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace]
        public Counter IncomingTransferNumberCounter { get; set; }

        #region Allors
        [Id("ba00c0d2-6067-4584-bdc4-e6c72be77232")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        Counter QuoteNumberCounter { get; set; }

        #region Allors
        [Id("d5645df8-2b10-435d-8e47-57b5d268541a")]
        #endregion
        [Required]
        [Workspace(Default)]
        bool DoAccounting { get; set; }

        #region Allors
        [Id("00bf781c-c874-44fe-ae60-d6609075b1c0")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string PurchaseOrderNumberPrefix { get; set; }

        #region Allors
        [Id("01d4f5d8-da57-4524-b35f-69a1a4adfa1c")]
        #endregion
        [Workspace(Default)]
        [Size(256)]
        string TransactionReferenceNumber { get; set; }

        #region Allors
        [Id("0994b73e-8d4c-4fa4-aca2-287449b22ca7")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Indexed]
        JournalEntryNumber[] JournalEntryNumbers { get; set; }

        #region Allors
        [Id("1a2533cb-9b75-4597-83ab-9bbfc49e0103")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Indexed]
        Country EuListingState { get; set; }

        #region Allors
        [Id("219a1d97-9615-47c5-bc4d-20a7d37313bd")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        AccountingPeriod ActualAccountingPeriod { get; set; }

        #region Allors
        [Id("293758d7-cc0a-4f1c-b122-84f609a828c2")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        PaymentMethod[] AssignedActiveCollectionMethods { get; set; }

        #region Allors
        [Id("55795ded-db46-48ab-ac33-6eac2dfd58c3")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        PaymentMethod[] DerivedActiveCollectionMethods { get; set; }

        #region Allors
        [Id("37b4bf2c-5b09-42b0-84d9-59b57793cf37")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal MaximumAllowedPaymentDifference { get; set; }

        #region Allors
        [Id("3b32c442-9cbc-41d8-8eb2-2ae41beca2c4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        CostCenterSplitMethod CostCenterSplitMethod { get; set; }

        #region Allors
        [Id("4fc741ef-fe95-49a8-8bcd-8ff43092db88")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        GeneralLedgerAccount SalesPaymentDifferencesAccount { get; set; }

        #region Allors
        [Id("5b64038f-5ad9-46a6-9af6-b95819ac9830")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string PurchaseTransactionReferenceNumber { get; set; }

        #region Allors
        [Id("63c9ceb1-d583-41e1-a9a9-0c2576e9adfc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Indexed]
        CostOfGoodsSoldMethod CostOfGoodsSoldMethod { get; set; }

        #region Allors
        [Id("77ae5145-791a-4ef0-94cc-6c9683b02f13")]
        #endregion
        [Workspace(Default)]
        bool VatDeactivated { get; set; }

        #region Allors
        [Id("848f3098-ce8b-400c-9775-85c00ac68f28")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        GeneralLedgerAccount[] GeneralLedgerAccounts { get; set; }

        #region Allors
        [Id("859d95c2-7321-4408-bcd1-405dc0b31efc")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        Counter AccountingTransactionCounter { get; set; }

        #region Allors
        [Id("89f4907d-4a10-428d-9e6b-ef9fb045c019")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        GeneralLedgerAccount RetainedEarningsAccount { get; set; }

        #region Allors
        [Id("9d6aaa81-9f97-427e-9f46-1f1e93748248")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string PurchaseInvoiceNumberPrefix { get; set; }

        #region Allors
        [Id("ab2004c1-fd91-4298-87cd-532a6fe5efb0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        GeneralLedgerAccount SalesPaymentDiscountDifferencesAccount { get; set; }

        #region Allors
        [Id("afbaffe6-b03c-463e-b074-08b32641b482")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        AccountingTransactionNumber[] AccountingTransactionNumbers { get; set; }

        #region Allors
        [Id("b8af8dce-d0e8-4e16-8d72-e56b920a04b4")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string TransactionReferenceNumberPrefix { get; set; }

        #region Allors
        [Id("d0ebaa65-260a-4511-a137-89f25016f12c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        GeneralLedgerAccount PurchasePaymentDifferencesAccount { get; set; }

        #region Allors
        [Id("d2ad57d5-de30-4bc0-90a7-9aea7a9da8c7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        GeneralLedgerAccount SuspenceAccount { get; set; }

        #region Allors
        [Id("d48ef8bb-064b-4360-8162-a138fb601761")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        GeneralLedgerAccount NetIncomeAccount { get; set; }

        #region Allors
        [Id("dd008dfe-a219-42ab-bc08-d091da3f8ea4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        GeneralLedgerAccount PurchasePaymentDiscountDifferencesAccount { get; set; }

        #region Allors
        [Id("e9af1ca5-d24f-4af2-8687-833744941b24")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string QuoteNumberPrefix { get; set; }

        #region Allors
        [Id("ec8e7400-0088-4237-af32-a687e1c45d77")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string PurchaseTransactionReferenceNumberPrefix { get; set; }

        #region Allors
        [Id("f353e7ef-d24d-4a27-8ec9-e930ef936240")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        GeneralLedgerAccount CalculationDifferencesAccount { get; set; }

        #region Allors
        [Id("06FABCD6-EFA4-45DD-B76C-8F791A0E10EF")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string RequestNumberPrefix { get; set; }

        #region Allors
        [Id("224518F0-2014-4BF4-B10A-406821A0FD39")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        Party[] ObsoleteCurrentCustomers { get; set; }

        #region Allors
        [Id("D4B532F9-12F0-4B51-AF8E-0E2160A2488E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        Organisation[] ObsoleteCurrentSuppliers { get; set; }

        #region Allors
        [Id("0ac44c21-6a2c-4162-9d77-fe1b16b60b73")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        GeneralLedgerAccount GlAccount { get; set; }

        #region Allors
        [Id("9a2ab89e-c3bc-4b6b-a82d-417dc21c8f9e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        Party[] ActiveCustomers { get; set; }

        #region Allors
        [Id("cd40057a-5211-4289-a4ef-c30aa4049957")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Derived]
        [Indexed]
        Person[] ActiveEmployees { get; set; }

        #region Allors
        [Id("e09976e8-dc99-4539-9b0b-0bbe98cc5404")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Derived]
        [Indexed]
        Organisation[] ActiveSuppliers { get; set; }

        #region Allors
        [Id("1caf947c-425e-43fa-bcdb-5961bedc0708")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Derived]
        [Indexed]
        Organisation[] ActiveSubContractors { get; set; }

        #region Allors
        [Id("02076C50-183B-4657-BE2F-9CF4872E9989")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        Template ProductQuoteTemplate { get; set; }

        #region Allors
        [Id("7FD61BF5-AC01-405C-A100-5DA3F2861B81")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        Template SalesOrderTemplate { get; set; }

        #region Allors
        [Id("8DF822D1-B7B8-4211-B941-69664FAA3537")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        Template PurchaseOrderTemplate { get; set; }

        #region Allors
        [Id("EE600466-BF26-4155-9FF1-0B86BA136AD1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        Template PurchaseInvoiceTemplate { get; set; }

        #region Allors
        [Id("A8D44A4A-9C82-44A7-BBBA-117A4F7D261B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        Template SalesInvoiceTemplate { get; set; }

        #region Allors
        [Id("9505E487-973D-4556-9C79-3538E7FE1C8B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        Template WorkTaskTemplate { get; set; }

        #region Allors
        [Id("fa9e9c4d-c210-4afd-89b3-4416be8c39a0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        Template WorkTaskWorkerTemplate { get; set; }

        /// <summary>
        /// Gets or sets the WorkEffortCounter to be used to populate the WorkEfforNumber for WorkEffort objects.
        /// </summary>
        #region Allors
        [Id("72BA7C7A-9EA5-4327-86AF-ED77041E19AE")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        Counter WorkEffortNumberCounter { get; set; }

        /// <summary>
        /// Gets or sets the WorkEffortPrefix to be used before the WorkEfforNumber for WorkEffort objects.
        /// </summary>
        #region Allors
        [Id("DFA0E963-7F13-41C0-B1CC-3BEBE1F951F1")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string WorkEffortNumberPrefix { get; set; }

        /// <summary>
        /// Gets or sets a flag to indicate if this InternalOrganisation Requires Existing WorkEffortPartyAssignment
        /// objects to exist before allowing TimeEntry objects to attach to WorkEffort objects.
        /// </summary>
        #region Allors
        [Id("37FEB213-9427-4A41-8050-DBFE9A33D03F")]
        #endregion
        [Workspace(Default)]
        bool RequireExistingWorkEffortPartyAssignment { get; set; }

        #region Allors
        [Id("786a74b0-015a-47db-8d3a-c790b326cc7d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Media LogoImage { get; set; }

        #region Allors
        [Id("0C328E5C-E3A8-44B9-BD4D-0DDABBFC9728")]
        #endregion
        void StartNewFiscalYear();

        #region Allors
        [Id("C5BC0A23-6E54-4EEE-A721-92B3D4F90459")]
        #endregion
        [Required]
        [Workspace(Default)]
        bool PurchaseOrderNeedsApproval { get; set; }

        #region Allors
        [Id("CE7E4BAA-2324-4E5D-A8A7-F05DF34CFF91")]
        #endregion
        [Workspace(Default)]
        decimal PurchaseOrderApprovalThresholdLevel1 { get; set; }

        #region Allors
        [Id("350C37ED-D2DD-4293-A4E6-BAB9C0D95841")]
        #endregion
        [Workspace(Default)]
        decimal PurchaseOrderApprovalThresholdLevel2 { get; set; }

        #region Allors
        [Id("ED2D831F-AF76-4C97-9EAD-D0D6644FCA85")]
        #endregion
        [Required]
        [Workspace(Default)]
        bool IsAutomaticallyReceived { get; set; }

        #region Allors
        [Id("B5A919ED-221C-4EED-AA2A-AE2ACB4B0B24")]
        #endregion
        [Required]
        [Workspace(Default)]
        bool AutoGeneratePurchaseShipment { get; set; }

        #region Allors
        [Id("b263b4a6-567e-4f55-a26d-a3a40a14ad3f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        SerialisedItemSoldOn[] SerialisedItemSoldOns { get; set; }

        #region inherited methods

        #endregion

        #region Allors
        [Id("B6586173-5AC6-457F-8605-070DA9E2631B")]
        #endregion
        [Workspace(Default)]
        void CreateRequest();

        #region Allors
        [Id("281E2CDB-6A14-47CF-9D43-446C4C9F67E1")]
        #endregion
        [Workspace(Default)]
        void CreateQuote();

        #region Allors
        [Id("CF3F43B4-6A32-44A7-911A-39E7293D2877")]
        #endregion
        [Workspace(Default)]
        void CreatePurchaseOrder();

        #region Allors
        [Id("94E92634-F1B0-43B9-B1B0-C36774C2B9F0")]
        #endregion
        [Workspace(Default)]
        void CreatePurchaseInvoice();

        #region Allors
        [Id("8FA98D9E-A991-405A-9B90-8C639BF4701E")]
        #endregion
        [Workspace(Default)]
        void CreateSalesOrder();

        #region Allors
        [Id("AE8895B9-9F31-428E-909C-D1F75741F150")]
        #endregion
        [Workspace(Default)]
        void CreateSalesInvoice();

        #region Allors
        [Id("7edf890e-03c4-4052-b7c9-e143dc50f7aa")]
        #endregion
        [Workspace(Default)]
        void CreateWorkEffortInvoice();
    }
}

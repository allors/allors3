// <copyright file="Organisation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("3a5dcec7-308f-48c7-afee-35d38415aa0b")]
    #endregion
    public partial class Organisation : InternalOrganisation, Deletable, Versioned
    {
        #region inherited properties
        public string PurchaseOrderNumberPrefix { get; set; }

        public string TransactionReferenceNumber { get; set; }

        public FiscalYearInternalOrganisationSequenceNumbers[] FiscalYearsInternalOrganisationSequenceNumbers { get; set; }

        public JournalEntryNumber[] JournalEntryNumbers { get; set; }

        public Country EuListingState { get; set; }

        public bool IsInternalOrganisation { get; set; }

        public PaymentMethod[] PaymentMethods { get; set; }

        public PaymentMethod DefaultCollectionMethod { get; set; }

        public Counter PurchaseInvoiceNumberCounter { get; set; }

        public AccountingPeriod ActualAccountingPeriod { get; set; }

        public InvoiceSequence InvoiceSequence { get; set; }

        public RequestSequence RequestSequence { get; set; }

        public QuoteSequence QuoteSequence { get; set; }

        public CustomerShipmentSequence CustomerShipmentSequence { get; set; }

        public PurchaseShipmentSequence PurchaseShipmentSequence { get; set; }

        public WorkEffortSequence WorkEffortSequence { get; set; }

        public PaymentMethod[] AssignedActiveCollectionMethods { get; set; }

        public PaymentMethod[] DerivedActiveCollectionMethods { get; set; }

        public decimal MaximumAllowedPaymentDifference { get; set; }

        public CostCenterSplitMethod CostCenterSplitMethod { get; set; }

        public Counter PurchaseOrderNumberCounter { get; set; }

        public GeneralLedgerAccount SalesPaymentDifferencesAccount { get; set; }

        public string PurchaseTransactionReferenceNumber { get; set; }

        public int FiscalYearStartMonth { get; set; }

        public CostOfGoodsSoldMethod CostOfGoodsSoldMethod { get; set; }

        public bool VatDeactivated { get; set; }

        public int FiscalYearStartDay { get; set; }

        public GeneralLedgerAccount[] GeneralLedgerAccounts { get; set; }

        public Counter AccountingTransactionCounter { get; set; }

        public Counter IncomingShipmentNumberCounter { get; set; }

        public GeneralLedgerAccount RetainedEarningsAccount { get; set; }

        public string PurchaseInvoiceNumberPrefix { get; set; }

        public GeneralLedgerAccount SalesPaymentDiscountDifferencesAccount { get; set; }

        public Counter SubAccountCounter { get; set; }

        public AccountingTransactionNumber[] AccountingTransactionNumbers { get; set; }

        public string TransactionReferenceNumberPrefix { get; set; }

        public Counter QuoteNumberCounter { get; set; }

        public Counter RequestNumberCounter { get; set; }

        public GeneralLedgerAccount PurchasePaymentDifferencesAccount { get; set; }

        public GeneralLedgerAccount SuspenceAccount { get; set; }

        public GeneralLedgerAccount NetIncomeAccount { get; set; }

        public bool DoAccounting { get; set; }

        public GeneralLedgerAccount PurchasePaymentDiscountDifferencesAccount { get; set; }

        public string QuoteNumberPrefix { get; set; }

        public string PurchaseTransactionReferenceNumberPrefix { get; set; }

        public GeneralLedgerAccount CalculationDifferencesAccount { get; set; }

        public string IncomingShipmentNumberPrefix { get; set; }

        public string RequestNumberPrefix { get; set; }

        public Party[] ObsoleteCurrentCustomers { get; set; }

        public Organisation[] ObsoleteCurrentSuppliers { get; set; }

        public GeneralLedgerAccount GlAccount { get; set; }

        public Party[] ActiveCustomers { get; set; }

        public Person[] ActiveEmployees { get; set; }

        public Organisation[] ActiveSuppliers { get; set; }

        public Organisation[] ActiveSubContractors { get; set; }

        public string PartyName { get; set; }

        public ContactMechanism GeneralCorrespondence { get; set; }

        public TelecommunicationsNumber BillingInquiriesFax { get; set; }

        public Qualification[] Qualifications { get; set; }

        public ContactMechanism HomeAddress { get; set; }

        public ContactMechanism SalesOffice { get; set; }

        public PartyContactMechanism[] InactivePartyContactMechanisms { get; set; }

        public TelecommunicationsNumber OrderInquiriesFax { get; set; }

        public PartyContactMechanism[] PartyContactMechanisms { get; set; }

        public PartyRelationship[] InactivePartyRelationships { get; set; }

        public Person[] CurrentContacts { get; set; }

        public Person[] InactiveContacts { get; set; }

        public TelecommunicationsNumber ShippingInquiriesFax { get; set; }

        public TelecommunicationsNumber ShippingInquiriesPhone { get; set; }

        public BillingAccount[] BillingAccounts { get; set; }

        public TelecommunicationsNumber OrderInquiriesPhone { get; set; }

        public PartySkill[] PartySkills { get; set; }

        public PartyClassification[] PartyClassifications { get; set; }

        public BankAccount[] BankAccounts { get; set; }

        public ContactMechanism BillingAddress { get; set; }

        public EmailAddress GeneralEmail { get; set; }

        public ShipmentMethod DefaultShipmentMethod { get; set; }

        public Resume[] Resumes { get; set; }

        public ContactMechanism HeadQuarter { get; set; }

        public EmailAddress PersonalEmailAddress { get; set; }

        public TelecommunicationsNumber CellPhoneNumber { get; set; }

        public TelecommunicationsNumber BillingInquiriesPhone { get; set; }

        public ContactMechanism OrderAddress { get; set; }

        public ElectronicAddress InternetAddress { get; set; }

        public Media[] Contents { get; set; }

        public CreditCard[] CreditCards { get; set; }

        public PostalAddress ShippingAddress { get; set; }

        public TelecommunicationsNumber GeneralFaxNumber { get; set; }

        public PartyContactMechanism[] CurrentPartyContactMechanisms { get; set; }

        public PartyRelationship[] CurrentPartyRelationships { get; set; }

        public TelecommunicationsNumber GeneralPhoneNumber { get; set; }

        public Currency PreferredCurrency { get; set; }

        public VatRegime VatRegime { get; set; }

        public IrpfRegime IrpfRegime { get; set; }

        public PaymentMethod DefaultPaymentMethod { get; set; }

        public Locale Locale { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Template ProductQuoteTemplate { get; set; }

        public Template SalesOrderTemplate { get; set; }

        public Template PurchaseOrderTemplate { get; set; }

        public Template PurchaseInvoiceTemplate { get; set; }

        public Template SalesInvoiceTemplate { get; set; }

        public Template WorkTaskTemplate { get; set; }

        public Template WorkTaskWorkerTemplate { get; set; }

        public PartyRate[] PartyRates { get; set; }

        public bool CollectiveWorkEffortInvoice { get; set; }

        public Counter WorkEffortNumberCounter { get; set; }

        public string WorkEffortNumberPrefix { get; set; }

        public bool RequireExistingWorkEffortPartyAssignment { get; set; }

        public Media LogoImage { get; set; }

        public void StartNewFiscalYear() { }

        public bool PurchaseOrderNeedsApproval { get; set; }

        public decimal PurchaseOrderApprovalThresholdLevel1 { get; set; }

        public decimal PurchaseOrderApprovalThresholdLevel2 { get; set; }

        public bool IsAutomaticallyReceived { get; set; }

        public bool AutoGeneratePurchaseShipment { get; set; }

        public SerialisedItemSoldOn[] SerialisedItemSoldOns { get; set; }

        public Guid DerivationTrigger { get; set; }

        #endregion

        #region Versioning
        #region Allors
        [Id("275CFF8F-AD72-4237-AEBD-158A72650D25")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public OrganisationVersion CurrentVersion { get; set; }

        #region Allors
        [Id("9BF20468-BF1D-410D-8D83-EBA561A5F066")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public OrganisationVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("8FB7635C-6C06-43E8-9B6C-A760C7205804")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool IsManufacturer { get; set; }

        #region Allors
        [Id("124181AE-3BB2-42E2-A27B-D9B811824282")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        public SecurityToken ContactsSecurityToken { get; set; }

        #region Allors
        [Id("98D13035-810F-4550-8EDE-8514FDFD275D")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        public AccessControl ContactsAccessControl { get; set; }

        #region Allors
        [Id("980631CB-CC72-4264-87E5-B65DC6ABBB4D")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Indexed]
        public UserGroup OwnerUserGroup { get; set; }

        #region Allors
        [Id("1c8bf2e3-6794-47c8-990c-f124d47653fb")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public LegalForm LegalForm { get; set; }

        #region Allors
        [Id("2cc74901-cda5-4185-bcd8-d51c745a8437")]
        #endregion
        [Indexed]
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("4cc8bc02-8305-4bd3-b0c7-e9b3ecaf4bd2")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Indexed]
        public UserGroup ContactsUserGroup { get; set; }

        #region Allors
        [Id("813633df-c6cb-44a6-9fdf-579aa8180ebd")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string TaxNumber { get; set; }

        #region Allors
        [Id("a5318bd4-da7d-48bd-9d41-00c3261caa09")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public IndustryClassification[] IndustryClassifications { get; set; }

        #region Allors
        [Id("d0ac426e-4775-4f2f-8055-08cb84e8e9bd")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public CustomOrganisationClassification[] CustomClassifications { get; set; }

        #region Allors
        [Id("c79070fc-2c7d-440b-80ce-f86796c59a14")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public OrganisationContactRelationship[] CurrentOrganisationContactRelationships { get; set; }

        #region Allors
        [Id("1bf7b758-2b58-4f82-a6a1-a8d5991d3d9d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public OrganisationContactRelationship[] InactiveOrganisationContactRelationships { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }
        #endregion

        public void CreateRequest() { }

        public void CreateQuote() { }

        public void CreatePurchaseOrder() { }

        public void CreatePurchaseInvoice() { }

        public void CreateSalesOrder() { }

        public void CreateSalesInvoice() { }

        public void CreateWorkEffortInvoice() { }
    }
}

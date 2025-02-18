
// <copyright file="PurchaseInvoice.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("7d7e4b6d-eebd-460c-b771-a93cd8d64bce")]
    #endregion
    public partial class PurchaseInvoice : Invoice, Versioned, WorkItem
    {
        #region inherited properties

        public Media[] ElectronicDocuments { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public string InternalComment { get; set; }

        public Currency AssignedCurrency { get; set; }

        public Currency DerivedCurrency { get; set; }

        public string Description { get; set; }

        public OrderAdjustment[] OrderAdjustments { get; set; }

        public string CustomerReference { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal TotalDiscount { get; set; }

        public BillingAccount BillingAccount { get; set; }

        public decimal TotalIncVat { get; set; }

        public decimal GrandTotal { get; set; }

        public decimal TotalSurcharge { get; set; }

        public decimal TotalBasePrice { get; set; }

        public DateTime InvoiceDate { get; set; }

        public DateTime EntryDate { get; set; }

        public decimal TotalShippingAndHandling { get; set; }

        public decimal TotalExVat { get; set; }

        public SalesTerm[] SalesTerms { get; set; }

        public string InvoiceNumber { get; set; }

        public string Message { get; set; }

        public VatRegime AssignedVatRegime { get; set; }

        public VatRegime DerivedVatRegime { get; set; }

        public VatRate DerivedVatRate { get; set; }

        public decimal TotalVat { get; set; }

        public decimal TotalFee { get; set; }

        public decimal TotalExtraCharge { get; set; }

        public InvoiceItem[] ValidInvoiceItems { get; set; }

        public Revocation[] Revocations { get; set; }

        public Revocation[] TransitionalRevocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public PrintDocument PrintDocument { get; set; }

        public string WorkItemDescription { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public IrpfRegime AssignedIrpfRegime { get; set; }

        public IrpfRegime DerivedIrpfRegime { get; set; }

        public IrpfRate DerivedIrpfRate { get; set; }

        public decimal TotalIrpf { get; set; }

        public int SortableInvoiceNumber { get; set; }

        public Guid DerivationTrigger { get; set; }

        public decimal TotalIrpfInPreferredCurrency { get; set; }

        public decimal TotalExVatInPreferredCurrency { get; set; }

        public decimal TotalVatInPreferredCurrency { get; set; }

        public decimal TotalIncVatInPreferredCurrency { get; set; }

        public decimal GrandTotalInPreferredCurrency { get; set; }

        public decimal TotalSurchargeInPreferredCurrency { get; set; }

        public decimal TotalDiscountInPreferredCurrency { get; set; }

        public decimal TotalShippingAndHandlingInPreferredCurrency { get; set; }

        public decimal TotalFeeInPreferredCurrency { get; set; }

        public decimal TotalExtraChargeInPreferredCurrency { get; set; }

        public decimal TotalBasePriceInPreferredCurrency { get; set; }

        public decimal TotalListPriceInPreferredCurrency { get; set; }

        public string SearchString { get; set; }

        #endregion

        #region ObjectStates
        #region PurchaseInvoiceState
        #region Allors
        [Id("EDC12BA8-41F6-4E3A-8430-9592201A821E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseInvoiceState PreviousPurchaseInvoiceState { get; set; }

        #region Allors
        [Id("96B88C50-E18C-4776-86CF-D3126A4E5C1B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseInvoiceState LastPurchaseInvoiceState { get; set; }

        #region Allors
        [Id("AAB01767-7EA3-48E4-85ED-153DED6CB873")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public PurchaseInvoiceState PurchaseInvoiceState { get; set; }
        #endregion
        #endregion

        #region Versioning
        #region Allors
        [Id("E1F38604-4DB9-4D34-A34E-9B64649ABDE9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public PurchaseInvoiceVersion CurrentVersion { get; set; }

        #region Allors
        [Id("AC26A490-1260-4E2D-B621-E827C12FAA39")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public PurchaseInvoiceVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("8b52455a-bcd1-4743-91d3-1bf9d2226c99")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Organisation BilledFrom { get; set; }

        #region Allors
        [Id("0CE57597-A6E0-4F9D-B619-A8688E02A045")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public ContactMechanism AssignedBilledFromContactMechanism { get; set; }

        #region Allors
        [Id("868472ef-a005-44a0-98ab-fd33fc5b1d3a")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public ContactMechanism DerivedBilledFromContactMechanism { get; set; }

        #region Allors
        [Id("9254C511-081D-4E05-98EB-5F4E52A700E3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person BilledFromContactPerson { get; set; }

        #region Allors
        [Id("045918FA-CC14-4616-A2B0-519E2ACEBA31")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public InternalOrganisation BilledTo { get; set; }

        #region Allors
        [Id("B75B10EE-6AA5-4C48-BDC0-61FA814B3E19")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person BilledToContactPerson { get; set; }

        #region Allors
        [Id("F37F0194-EDF4-49DA-BDB2-AA9C2A601809")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party ShipToCustomer { get; set; }

        #region Allors
        [Id("3E8E3FAF-2FFE-483A-8F7C-3ABF4BC29BD6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public PostalAddress AssignedShipToCustomerAddress { get; set; }

        #region Allors
        [Id("996d7b2e-5085-41cf-8c7b-ce3d790fdf65")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public PostalAddress DerivedShipToCustomerAddress { get; set; }

        #region Allors
        [Id("5EF823D6-7EED-40C6-9E4A-9106A655B9E1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person ShipToCustomerContactPerson { get; set; }

        #region Allors
        [Id("ABB96E7A-F5D1-4173-AEDB-62B217A22495")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party BillToEndCustomer { get; set; }

        #region Allors
        [Id("B9ED27E2-9429-40AF-8B1E-4C4210023F5F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public ContactMechanism AssignedBillToEndCustomerContactMechanism { get; set; }

        #region Allors
        [Id("5004df78-2aac-4e58-9e6b-b83ac59253ca")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public ContactMechanism DerivedBillToEndCustomerContactMechanism { get; set; }

        #region Allors
        [Id("91CF5142-0240-47A6-9423-3EA8F2EA43F4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person BillToEndCustomerContactPerson { get; set; }

        #region Allors
        [Id("0E1BB984-6D42-4473-9BA1-A3EBDEF84A54")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party ShipToEndCustomer { get; set; }

        #region Allors
        [Id("B4EC8C7B-E4BA-4428-9898-6FB9B6C048A4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress AssignedShipToEndCustomerAddress { get; set; }

        #region Allors
        [Id("3af66b6f-0f5d-4b3c-b0a3-7276b687403b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public PostalAddress DerivedShipToEndCustomerAddress { get; set; }

        #region Allors
        [Id("02C6EEC2-0DBD-4F63-9FFC-6C107E90E303")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person ShipToEndCustomerContactPerson { get; set; }

        #region Allors
        [Id("F3CF7DEC-452C-4A68-9653-E7BB8987F8A1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public PaymentMethod AssignedBillToCustomerPaymentMethod { get; set; }

        #region Allors
        [Id("5a2ab60a-00a0-4302-81a6-49d8211eaea6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public PaymentMethod DerivedBillToCustomerPaymentMethod { get; set; }

        #region Allors
        [Id("4cf09eb7-820f-4677-bfc0-92a48d0a938b")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public PurchaseInvoiceItem[] PurchaseInvoiceItems { get; set; }

        #region Allors
        [Id("e444b5e7-0128-49fc-86cb-a6fe39c280ae")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public PurchaseInvoiceType PurchaseInvoiceType { get; set; }

        #region Allors
        [Id("147372B8-ADC3-442E-BF60-968A0B13FBDD")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Workspace(Default)]
        public PurchaseOrder[] PurchaseOrders { get; set; }

        #region Allors
        [Id("B1C82298-2ABF-4FF7-BCC1-EF6B77AB6B50")]
        #endregion
        [Workspace(Default)]
        public DateTime DueDate { get; set; }

        #region Allors
        [Id("b1a0d63f-a0bc-424e-9294-e3b7b37e9c6e")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal ActualInvoiceAmount { get; set; }

        #region Allors
        [Id("797A9C2C-A2CF-4AE3-8395-B2F25D0F40C1")]
        #endregion
        [Workspace(Default)]
        public void Confirm() { }

        #region Allors
        [Id("B188B7B5-BA61-4FF5-9D9A-812E22F8A289")]
        #endregion
        [Workspace(Default)]
        public void Approve() { }

        #region Allors
        [Id("55DA112F-F7BF-4400-B1FD-7A87A7B3C67B")]
        #endregion
        [Workspace(Default)]
        public void Reject() { }

        #region Allors
        [Id("07A2BE5F-5686-4B0A-8B05-8875FA277622")]
        #endregion
        [Workspace(Default)]
        public void Cancel() { }

        #region Allors
        [Id("2D4FDE1F-FE36-4880-9B95-ACFE1B20C085")]
        #endregion
        [Workspace(Default)]
        public void Reopen() { }

        #region Allors
        [Id("422DD593-DECC-40FD-9216-D5A25458B59F")]
        #endregion
        [Workspace(Default)]
        public void CreateSalesInvoice() { }

        #region Allors
        [Id("4BF977FA-75AF-4D6D-8CD7-7250D527EF61")]
        #endregion
        [Workspace(Default)]
        public void SetPaid() { }

        #region Allors
        [Id("3bd0368b-78dc-4872-8437-62645b16ee2b")]
        #endregion
        [Workspace(Default)]
        public void FinishRevising() { }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        public void Delete() { }

        public void Print() { }

        public void Create() { }

        public void Revise() { }

        #endregion
    }
}

// <copyright file="SalesInvoice.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("6173fc23-115f-4356-a0ce-867872c151ac")]
    #endregion
    public partial class SalesInvoice : Invoice, Versioned, Localised
    {
        #region inherited properties

        public Media[] ElectronicDocuments { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public string InternalComment { get; set; }

        public Currency Currency { get; set; }

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

        public VatRegime VatRegime { get; set; }

        public decimal TotalVat { get; set; }

        public decimal TotalFee { get; set; }

        public decimal TotalExtraCharge { get; set; }

        public InvoiceItem[] ValidInvoiceItems { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public Permission[] TransitionalDeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Locale Locale { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public PrintDocument PrintDocument { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public IrpfRegime IrpfRegime { get; set; }

        public decimal TotalIrpf { get; set; }

        public int SortableInvoiceNumber { get; set; }

        public Locale DefaultLocale { get; set; }

        public Currency DefaultCurrency { get; set; }

        public Guid DerivationTrigger { get; set; }

        #endregion

        #region ObjectStates
        #region SalesInvoiceState
        #region Allors
        [Id("0617CB28-67B8-4BEF-A9A3-6A06C292A7F0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesInvoiceState PreviousSalesInvoiceState { get; set; }

        #region Allors
        [Id("E706A4A4-BB19-431A-8949-1B22B0F8AA68")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesInvoiceState LastSalesInvoiceState { get; set; }

        #region Allors
        [Id("931B8FC4-A0EC-4450-A469-EB585839B05A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SalesInvoiceState SalesInvoiceState { get; set; }
        #endregion
        #endregion

        #region Versioning
        #region Allors
        [Id("FD3391B6-1B75-43F6-ADDB-A97F5E8F3BC6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public SalesInvoiceVersion CurrentVersion { get; set; }

        #region Allors
        [Id("EF051A68-7FB9-4461-B16D-34F1B99F34C4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public SalesInvoiceVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("D18CE961-CC03-47E2-A179-A5D93E7C65CD")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public InternalOrganisation BilledFrom { get; set; }

        #region Allors
        [Id("ddd9b372-4687-4a6e-b62b-4e0521f8c4b7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanism BilledFromContactMechanism { get; set; }

        #region Allors
        [Id("B7797410-75C3-4BE2-AD1A-5CF6D7438A72")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person BilledFromContactPerson { get; set; }

        #region Allors
        [Id("816d66a7-7cab-4ce3-9912-c7cc9d6c294c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Party BillToCustomer { get; set; }

        #region Allors
        [Id("2d0e924b-ff24-4630-9151-ac9bfc844c0c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        public Party PreviousBillToCustomer { get; set; }

        #region Allors
        [Id("8FCCF6B4-69EE-4087-8F42-9BDAC9971FA3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public ContactMechanism BillToContactMechanism { get; set; }

        #region Allors
        [Id("F03DD1C5-A2BA-4231-8274-7594E2AF5507")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person BillToContactPerson { get; set; }

        #region Allors
        [Id("B51570D0-05F5-45D1-B820-6FA693A60EB9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party BillToEndCustomer { get; set; }

        #region Allors
        [Id("27faaa2c-d4db-4cab-aa04-8ec4997d73d2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanism BillToEndCustomerContactMechanism { get; set; }

        #region Allors
        [Id("B1DA9A4E-F969-44DC-BFD8-409FE5E33AA9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person BillToEndCustomerContactPerson { get; set; }

        #region Allors
        [Id("6AFCE702-9659-480B-A0C0-CC7FBC92E05E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party ShipToEndCustomer { get; set; }

        #region Allors
        [Id("3A27066B-1EC4-4B72-9920-95E31C3539D9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress ShipToEndCustomerAddress { get; set; }

        #region Allors
        [Id("08A314AE-A3DF-4993-AE09-11E6E5A0B316")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person ShipToEndCustomerContactPerson { get; set; }

        #region Allors
        [Id("af0a72c8-003c-44a6-8c6f-086f26542e3d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party ShipToCustomer { get; set; }

        #region Allors
        [Id("7f833ad2-3146-4660-a9d4-8a70d3ce01db")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        public Party PreviousShipToCustomer { get; set; }

        #region Allors
        [Id("f808aafb-3c7d-4a26-af5c-44b76ee45e86")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress ShipToAddress { get; set; }

        #region Allors
        [Id("D8127DD5-BD5C-4608-9D69-B2F585A45066")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person ShipToContactPerson { get; set; }

        #region Allors
        [Id("3eb16102-21cc-4b71-a8e2-4f016da4cfb0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public SalesInvoiceType SalesInvoiceType { get; set; }

        #region Allors
        [Id("09064adb-7094-48e9-992c-2eab319d640f")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal TotalListPrice { get; set; }

        #region Allors
        [Id("4a7695a8-c649-4122-9336-8a1e2e5665ea")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PaymentMethod PaymentMethod { get; set; }

        #region Allors
        [Id("C6F95A7F-C812-42A3-B215-7A110D9D6862")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public PurchaseInvoice PurchaseInvoice { get; set; }

        #region Allors
        [Id("8F2706C3-445B-4B75-941E-A5E07CBEBF02")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Workspace(Default)]
        public SalesOrder[] SalesOrders { get; set; }

        #region Allors
        [Id("91D368B2-D4BE-47F4-A5D7-DE122AB518C5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Workspace(Default)]
        public WorkEffort[] WorkEfforts { get; set; }

        #region Allors
        [Id("89557826-c9d1-4aa1-8789-79fb425cdb87")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public SalesInvoiceItem[] SalesInvoiceItems { get; set; }

        #region Allors
        [Id("ed091c3c-1f38-498a-8ca5-ca8b8ddfc5c4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public SalesChannel SalesChannel { get; set; }

        #region Allors
        [Id("f2f85b74-b28f-4627-9dca-94142789c0bc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public Party[] Customers { get; set; }

        #region Allors
        [Id("fd12507e-96b7-4b15-a43d-ab418d4795d6")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Store Store { get; set; }

        #region Allors
        [Id("CE515051-9555-4810-8175-17B152919C2A")]
        [Indexed]
        #endregion
        [Required]
        [Workspace(Default)]
        public decimal AdvancePayment { get; set; }

        #region Allors
        [Id("17ED2D23-0D51-4F04-BBD4-572318E91D82")]
        [Indexed]
        #endregion
        [Derived]
        [Required]
        [Workspace(Default)]
        public int PaymentDays { get; set; }

        #region Allors
        [Id("DB18BDE8-D70F-4E24-8866-D1D46CB0D82B")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public DateTime DueDate { get; set; }

        #region Allors
        [Id("96D069CB-E463-4C94-A294-17A57D6CD418")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public VatClause AssignedVatClause { get; set; }

        #region Allors
        [Id("4B8F6EB0-DCDC-4F6D-BFD8-7DE8CE159377")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public VatClause DerivedVatClause { get; set; }

        #region Allors
        [Id("968409E5-9FA6-4085-9707-6ECF38A08272")]
        [Indexed]
        #endregion
        [Required]
        [Derived]
        [Workspace(Default)]
        public bool IsRepeatingInvoice { get; set; }

        #region Allors
        [Id("55A60B80-2052-47E6-BD41-2AF414ABB885")]
        #endregion
        [Workspace(Default)]
        public void Send() { }

        #region Allors
        [Id("96AF8F69-F1A4-420A-8D9D-AF61EB061620")]
        #endregion
        [Workspace(Default)]
        public void CancelInvoice() { }

        #region Allors
        [Id("F6EC229C-288C-4830-9DE5-8D5236DE4781")]
        #endregion
        [Workspace(Default)]
        public void WriteOff() { }

        #region Allors
        [Id("99F56814-6F90-4A6A-996B-84DDE7956544")]
        #endregion
        [Workspace(Default)]
        public void Copy() { }

        #region Allors
        [Id("1D9B28F2-1439-41F8-A556-59BDEFB4683E")]
        #endregion
        [Workspace(Default)]
        public void Credit() { }

        #region Allors
        [Id("033FF876-BBC5-47B3-B2C9-CEDE9869C231")]
        #endregion
        [Workspace(Default)]
        public void Reopen() { }

        #region Allors
        [Id("A2E784E3-B0D0-42FE-8E3C-7217E8948D95")]
        #endregion
        [Workspace(Default)]
        public void SetPaid() { }

        #region inherited methods
        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Print() { }

        public void Delete() { }

        public void Create() { }

        public void Revise() { }

        #endregion
    }
}

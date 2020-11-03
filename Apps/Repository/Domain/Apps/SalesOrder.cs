// <copyright file="SalesOrder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("716647bf-7589-4146-a45c-a6a3b1cee507")]
    #endregion
    public partial class SalesOrder : Order, Versioned
    {
        #region inherited properties

        public Media[] ElectronicDocuments { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public string InternalComment { get; set; }

        public Currency Currency { get; set; }

        public string CustomerReference { get; set; }

        public OrderAdjustment[] OrderAdjustments { get; set; }

        public decimal TotalExVat { get; set; }

        public SalesTerm[] SalesTerms { get; set; }

        public decimal TotalVat { get; set; }

        public decimal TotalSurcharge { get; set; }

        public OrderItem[] ValidOrderItems { get; set; }

        public string OrderNumber { get; set; }

        public decimal TotalDiscount { get; set; }

        public string Message { get; set; }

        public string Description { get; set; }

        public DateTime EntryDate { get; set; }

        public OrderKind OrderKind { get; set; }

        public decimal TotalIncVat { get; set; }

        public decimal GrandTotal { get; set; }

        public VatRegime VatRegime { get; set; }

        public decimal TotalShippingAndHandling { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime DeliveryDate { get; set; }

        public decimal TotalBasePrice { get; set; }

        public decimal TotalFee { get; set; }

        public decimal TotalExtraCharge { get; set; }

        public IrpfRegime IrpfRegime { get; set; }

        public decimal TotalIrpf { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public Permission[] TransitionalDeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public PrintDocument PrintDocument { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Locale Locale { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public int SortableOrderNumber { get; set; }
        #endregion

        #region ObjectStates
        #region SalesOrderState
        #region Allors
        [Id("13476417-0F9F-48AD-A197-D8FE897345E6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderState PreviousSalesOrderState { get; set; }

        #region Allors
        [Id("ABD5310C-D5CF-4584-B0E5-D76CB2A7174E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderState LastSalesOrderState { get; set; }

        #region Allors
        [Id("822493E4-B80B-4506-BBF6-C3CD73DE0C4A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SalesOrderState SalesOrderState { get; set; }
        #endregion

        #region SalesOrderPaymentState
        #region Allors
        [Id("5548C20D-6F31-4D7C-8A80-7D9CE6187B76")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderPaymentState PreviousSalesOrderPaymentState { get; set; }

        #region Allors
        [Id("6D34483F-9A30-4C37-8B5E-22AA9EA03381")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderPaymentState LastSalesOrderPaymentState { get; set; }

        #region Allors
        [Id("5B79CBD9-D450-4AF3-9338-A66025345011")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Derived]
        public SalesOrderPaymentState SalesOrderPaymentState { get; set; }
        #endregion

        #region SalesOrderInvoiceState
        #region Allors
        [Id("B1B53EB4-1FB6-476B-899F-FFAF5AE8ED28")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderInvoiceState PreviousSalesOrderInvoiceState { get; set; }

        #region Allors
        [Id("82EF3FDC-41CA-462F-B50E-35FEE72866BE")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderInvoiceState LastSalesOrderInvoiceState { get; set; }

        #region Allors
        [Id("FE413572-DFEB-4EB5-BDB0-003A2600946B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public SalesOrderInvoiceState SalesOrderInvoiceState { get; set; }
        #endregion

        #region SalesOrderShipmentState
        #region Allors
        [Id("0C064509-FBCE-466B-A67E-C4EDD465E926")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderShipmentState PreviousSalesOrderShipmentState { get; set; }

        #region Allors
        [Id("3B64B544-C2EB-48AE-8DBC-32F5B31C21D2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderShipmentState LastSalesOrderShipmentState { get; set; }

        #region Allors
        [Id("23F8B966-A5E2-42B3-BD84-029D31FC073C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public SalesOrderShipmentState SalesOrderShipmentState { get; set; }
        #endregion
        #endregion

        #region Versioning
        #region Allors
        [Id("3CE4119B-6653-482D-9EEC-27BBDC56472F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public SalesOrderVersion CurrentVersion { get; set; }

        #region Allors
        [Id("9C6B7601-4B01-4A2D-8B43-E08703B66A0C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public SalesOrderVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("9E20E5E5-1838-48F6-A9E6-B56ED4F44BA3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public InternalOrganisation TakenBy { get; set; }

        #region Allors
        [Id("108a1136-feaa-45b8-a899-d455718090d1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        [Indexed]
        public ContactMechanism TakenByContactMechanism { get; set; }

        #region Allors
        [Id("F43697A3-157B-487C-86D1-42B9A469AE88")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person TakenByContactPerson { get; set; }

        #region Allors
        [Id("28359bf8-506e-41db-a86b-a1eee3d50198")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Indexed]
        [Workspace(Default)]
        public Party ShipToCustomer { get; set; }

        #region Allors
        [Id("3d01b9c9-5f37-40a8-9305-8ee9e98cc192")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        public Party PreviousShipToCustomer { get; set; }

        #region Allors
        [Id("848B1B7B-C91E-460E-B702-342CDCC58238")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress ShipFromAddress { get; set; }

        #region Allors
        [Id("3a2be2f2-2608-46e0-b1f1-1da7e372b8f8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress ShipToAddress { get; set; }

        #region Allors
        [Id("09469930-2854-4B18-99F4-65A02F4332A1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person ShipToContactPerson { get; set; }

        #region Allors
        [Id("2bd27b4c-37fd-4f82-bd43-4301ac704749")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        [Indexed]
        public Party BillToCustomer { get; set; }

        #region Allors
        [Id("c90e107b-6b47-4337-9937-391eacd1b1c5")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        public Party PreviousBillToCustomer { get; set; }

        #region Allors
        [Id("2171BB3B-A09C-499E-A654-E1A6A615FF0B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public ContactMechanism BillToContactMechanism { get; set; }

        #region Allors
        [Id("FB1749C7-FB4E-4E85-9440-01941885EE31")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person BillToContactPerson { get; set; }

        #region Allors
        [Id("501673A2-E15F-44BB-9CA2-BCCC1F0E2E66")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party BillToEndCustomer { get; set; }

        #region Allors
        [Id("469084d5-acc5-4fc9-910b-ead4d8d4d021")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanism BillToEndCustomerContactMechanism { get; set; }

        #region Allors
        [Id("F49326D6-77B3-4F46-BC93-5DC0340796FB")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person BillToEndCustomerContactPerson { get; set; }

        #region Allors
        [Id("D5B46F18-77AB-4535-A3DA-027489CBA9D1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party ShipToEndCustomer { get; set; }

        #region Allors
        [Id("D6216599-925F-4BEA-B6EB-C0B6CCE05617")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress ShipToEndCustomerAddress { get; set; }

        #region Allors
        [Id("D1E8AAE3-FBA0-4693-8954-CC29AD2D042C")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person ShipToEndCustomerContactPerson { get; set; }

        #region Allors
        [Id("d6714c09-dce1-4182-aa2f-bbc887edc89a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party PlacingCustomer { get; set; }

        #region Allors
        [Id("ba592fc9-78bb-4102-b9b5-fa692210dc38")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanism PlacingCustomerContactMechanism { get; set; }

        #region Allors
        [Id("3963B50C-5EEA-4068-955F-85914C57F938")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person PlacingCustomerContactPerson { get; set; }

        #region Allors
        [Id("81560BAA-D8E1-4688-A191-3081C0CE3B01")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public Facility OriginFacility { get; set; }

        #region Allors
        [Id("2ee793c8-512e-4358-b28a-f364280db93f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ShipmentMethod ShipmentMethod { get; set; }

        #region Allors
        [Id("7c5206f5-391d-485d-a030-513450f4dd2f")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal TotalListPrice { get; set; }

        #region Allors
        [Id("8f27c21b-ac66-4851-90d8-e955ef31bbec")]
        #endregion
        [Required]
        public bool PartiallyShip { get; set; }

        #region Allors
        [Id("a1d8e768-0a81-409d-ac13-7c7b8f5081f0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        public Party[] Customers { get; set; }

        #region Allors
        [Id("a54ff0dc-5adb-4314-8081-66522431b11d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Store Store { get; set; }

        #region Allors
        [Id("b9f315a5-22dc-4cba-a19f-fe71fe56ca49")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Indexed]
        [Workspace(Default)]
        public PaymentMethod PaymentMethod { get; set; }

        #region Allors
        [Id("ce771472-d789-4077-80bb-25622624e1df")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Indexed]
        public SalesChannel SalesChannel { get; set; }

        #region Allors
        [Id("da5a63d2-33bb-4da3-a1bf-064280cac0fa")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Indexed]
        public SalesInvoice ProformaInvoice { get; set; }

        #region Allors
        [Id("eb5a3564-996d-4bbe-b592-6205adad93b8")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public SalesOrderItem[] SalesOrderItems { get; set; }

        #region Allors
        [Id("7788542E-5095-4D18-8F52-0732CBB599EA")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public ProductQuote Quote { get; set; }

        #region Allors
        [Id("5308B55C-A248-44C1-9438-951B1BCDB48C")]
        #endregion
        [Derived]
        [Required]
        [Workspace(Default)]
        public bool CanShip { get; set; }

        #region Allors
        [Id("5B00CA2E-3F97-445E-813E-AA315C588AAC")]
        #endregion
        [Derived]
        [Required]
        [Workspace(Default)]
        public bool CanInvoice { get; set; }

        #region Allors
        [Id("1FC9526F-D1C5-41E3-93D9-FDA9308CD7B4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public VatClause AssignedVatClause { get; set; }

        #region Allors
        [Id("5E059289-F4F8-4FFB-B642-9F2A0B597713")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public VatClause DerivedVatClause { get; set; }

        public Guid DerivationTrigger { get; set; }
        #region Allors

        [Id("393446ab-59ea-4f23-b1bc-f8fd4e3dfb1b")]

        #endregion
        [Workspace(Default)]
        public void SetReadyForPosting() { }

        #region Allors

        [Id("4B8004A4-4E4E-4E52-913E-AB25AE24D240")]

        #endregion
        [Workspace(Default)]
        public void Post() { }

        #region Allors

        [Id("d7c82d6b-b1e0-4496-b1e7-28052f86e496")]

        #endregion
        [Workspace(Default)]
        public void Accept() { }

        #region Allors
        [Id("E822B75C-3A37-480A-A469-B18A060EC560")]
        #endregion
        [Workspace(Default)]
        public void Ship() { }

        #region Allors
        [Id("35c5cfac-f8bc-4640-9aaa-50989fd9f765")]
        #endregion
        [Workspace(Default)]
        public void DoTransfer() { }

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

        public void Create() { }

        public void Approve() { }

        public void Revise() { }

        public void Reject() { }

        public void Hold() { }

        public void Continue() { }

        public void Cancel() { }

        public void Complete() { }

        public void Invoice() { }

        public void Reopen() { }

        public void Print() { }

        #endregion
    }
}

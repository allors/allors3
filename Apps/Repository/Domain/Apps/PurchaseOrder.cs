// <copyright file="PurchaseOrder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("062bd939-9902-4747-a631-99ea10002156")]
    #endregion
    public partial class PurchaseOrder : Order, Versioned, WorkItem
    {
        #region inherited properties

        public Media[] ElectronicDocuments { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public string InternalComment { get; set; }

        public Currency AssignedCurrency { get; set; }

        public Currency DerivedCurrency { get; set; }

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

        public VatRegime AssignedVatRegime { get; set; }

        public VatRegime DerivedVatRegime { get; set; }

        public VatRate DerivedVatRate { get; set; }

        public decimal TotalShippingAndHandling { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime DeliveryDate { get; set; }

        public decimal TotalBasePrice { get; set; }

        public decimal TotalFee { get; set; }

        public decimal TotalExtraCharge { get; set; }

        public IrpfRegime AssignedIrpfRegime { get; set; }

        public IrpfRegime DerivedIrpfRegime { get; set; }

        public IrpfRate DerivedIrpfRate { get; set; }

        public decimal TotalIrpf { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public Permission[] TransitionalDeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public PrintDocument PrintDocument { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Locale Locale { get; set; }

        public Locale DerivedLocale { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string WorkItemDescription { get; set; }

        public int SortableOrderNumber { get; set; }

        public Guid DerivationTrigger { get; set; }

        #endregion

        #region ObjectStates
        #region PurchaseOrderState
        #region Allors
        [Id("6E96C786-EC92-4A1A-BE23-51D3DA29048F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseOrderState PreviousPurchaseOrderState { get; set; }

        #region Allors
        [Id("249741FA-C0AB-4259-B647-32144DEEEA33")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseOrderState LastPurchaseOrderState { get; set; }

        #region Allors
        [Id("C57CDFF9-C921-49D0-9781-A1B4D03F4C85")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public PurchaseOrderState PurchaseOrderState { get; set; }
        #endregion

        #region PurchaseOrderPaymentState
        #region Allors
        [Id("7F7CA63A-57D9-47DA-BC75-1FA9AE5FEC96")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseOrderPaymentState PreviousPurchaseOrderPaymentState { get; set; }

        #region Allors
        [Id("A0C5FC70-EDE7-4571-A6F3-9EC2E9A8731F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseOrderPaymentState LastPurchaseOrderPaymentState { get; set; }

        #region Allors
        [Id("65182F74-EF4E-4E35-9E3E-3AC6E42CEFE1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public PurchaseOrderPaymentState PurchaseOrderPaymentState { get; set; }
        #endregion

        #region PurchaseOrderShipmentState
        #region Allors
        [Id("03BF41DE-6170-424C-9D56-A6999144992F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseOrderShipmentState PreviousPurchaseOrderShipmentState { get; set; }

        #region Allors
        [Id("2E65C1AD-7581-4AF0-ADF2-EEDA88422B1B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseOrderShipmentState LastPurchaseOrderShipmentState { get; set; }

        #region Allors
        [Id("D7DBE9EE-544F-463A-80E9-522AF0E00325")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public PurchaseOrderShipmentState PurchaseOrderShipmentState { get; set; }
        #endregion

        #endregion

        #region Versioning
        #region Allors
        [Id("25006AFF-3E2E-44D5-9F42-38A34868EA87")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public PurchaseOrderVersion CurrentVersion { get; set; }

        #region Allors
        [Id("101C600D-775C-44A1-B065-F464D68CF14A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public PurchaseOrderVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("1982BFC9-9B79-4C1A-984D-8784EE02895F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public InternalOrganisation OrderedBy { get; set; }

        #region Allors
        [Id("15ea478f-b71d-412f-8ee4-abe554b9a7d8")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public PurchaseOrderItem[] PurchaseOrderItems { get; set; }

        #region Allors
        [Id("1638a432-3a4f-4cca-906e-660b9164838b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        public Organisation PreviousTakenViaSupplier { get; set; }

        #region Allors
        [Id("36607a9e-d411-4726-a63c-7622b928bfe8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Organisation TakenViaSupplier { get; set; }

        #region Allors
        [Id("6ef15b20-da12-47ed-aa2a-fdf06b17fdac")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        public Organisation PreviousTakenViaSubcontractor { get; set; }

        #region Allors
        [Id("483f1661-9c50-4eb2-82b3-8c060920a90e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Organisation TakenViaSubcontractor { get; set; }

        #region Allors
        [Id("4830cfc5-0375-4996-8cd8-27e36c102b65")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanism AssignedTakenViaContactMechanism { get; set; }

        #region Allors
        [Id("bc312efb-7e4a-45d9-aa0a-4628164503f7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public ContactMechanism DerivedTakenViaContactMechanism { get; set; }

        #region Allors
        [Id("73A7B96E-5DA1-465D-9754-CDB3184DC20E")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person TakenViaContactPerson { get; set; }

        #region Allors
        [Id("7eceb1b6-1395-4655-a558-6d72ad4b380e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanism AssignedBillToContactMechanism { get; set; }

        #region Allors
        [Id("42a2bc0a-5849-4781-9c8b-4793ccc6ff2a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Derived]
        [Workspace(Default)]
        public ContactMechanism DerivedBillToContactMechanism { get; set; }

        #region Allors
        [Id("63AE6DC7-F484-49D8-87D1-6F137232D385")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person BillToContactPerson { get; set; }

        #region Allors
        [Id("ccf88515-6441-4d0f-a2e7-8f5ed7c0533e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Indexed]
        [Workspace(Default)]
        public Facility StoredInFacility { get; set; }

        #region Allors
        [Id("d74bd1fd-f243-4b5d-8061-1eafe7c25beb")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress AssignedShipToAddress { get; set; }

        #region Allors
        [Id("cd5001f2-dee2-4b94-baa1-32fb9b957163")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public PostalAddress DerivedShipToAddress { get; set; }

        #region Allors
        [Id("8C27B92F-FB4C-4FAA-8F1C-3CF80AD746E8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person ShipToContactPerson { get; set; }

        #region Allors
        [Id("fdab0bcc-518d-419c-a446-499381158c2d")]
        #endregion
        [Required]
        [Derived]
        public bool OverDue { get; set; }

        #region Allors
        [Id("db6de178-8836-406a-b02a-e58500c9d617")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Derived]
        [Indexed]
        public PurchaseOrderItemByProduct[] PurchaseOrderItemsByProduct { get; set; }

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

        #region Allors
        [Id("c0d775b3-6a12-47ff-b404-0598b11acd50")]
        #endregion
        [Workspace(Default)]
        public void SetReadyForProcessing() { }

        #region Allors
        [Id("2CED78A3-0A7D-475B-82EE-5374D6E65944")]
        #endregion
        [Workspace(Default)]
        public void Send() { }

        #region Allors
        [Id("08E9783F-4DEE-428B-ADDD-785775AFAA46")]
        #endregion
        [Workspace(Default)]
        public void QuickReceive() { }
    }
}

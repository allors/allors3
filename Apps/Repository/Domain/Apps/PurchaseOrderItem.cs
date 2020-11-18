// <copyright file="PurchaseOrderItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("ab648bd0-6e31-4ab0-a9ee-cf4a6f02033d")]
    #endregion
    public partial class PurchaseOrderItem : OrderItem, Versioned
    {
        #region inherited properties
        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public string InternalComment { get; set; }

        public BudgetItem BudgetItem { get; set; }

        public decimal PreviousQuantity { get; set; }

        public decimal QuantityOrdered { get; set; }

        public string Description { get; set; }

        public PurchaseOrder CorrespondingPurchaseOrder { get; set; }

        public DiscountAdjustment[] DiscountAdjustments { get; set; }

        public SurchargeAdjustment[] SurchargeAdjustments { get; set; }

        public decimal TotalOrderAdjustment { get; set; }

        public QuoteItem QuoteItem { get; set; }

        public DateTime AssignedDeliveryDate { get; set; }

        public DateTime DeliveryDate { get; set; }

        public SalesTerm[] SalesTerms { get; set; }

        public string ShippingInstruction { get; set; }

        public OrderItem[] Associations { get; set; }

        public IrpfRegime DerivedIrpfRegime { get; set; }

        public IrpfRegime AssignedIrpfRegime { get; set; }

        public IrpfRate IrpfRate { get; set; }

        public decimal UnitIrpf { get; set; }

        public decimal TotalIrpf { get; set; }

        public decimal GrandTotal { get; set; }

        public string Message { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public Permission[] TransitionalDeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public decimal TotalDiscountAsPercentage { get; set; }

        public decimal UnitVat { get; set; }

        public VatRegime DerivedVatRegime { get; set; }

        public decimal TotalVat { get; set; }

        public decimal UnitSurcharge { get; set; }

        public decimal UnitDiscount { get; set; }

        public VatRate VatRate { get; set; }

        public decimal AssignedUnitPrice { get; set; }

        public decimal UnitBasePrice { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalIncVat { get; set; }

        public decimal TotalSurchargeAsPercentage { get; set; }

        public decimal TotalDiscount { get; set; }

        public decimal TotalSurcharge { get; set; }

        public VatRegime AssignedVatRegime { get; set; }

        public decimal TotalBasePrice { get; set; }

        public decimal TotalExVat { get; set; }

        public Order SyncedOrder { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public Guid DerivationTrigger { get; set; }

        #endregion

        #region ObjectStates
        #region PurchaseOrderItemState
        #region Allors
        [Id("830C4CBE-1621-4CC3-BC8D-CFC853B2C70A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseOrderItemState PreviousPurchaseOrderItemState { get; set; }

        #region Allors
        [Id("D6EE10B5-A4D6-4B7E-BCE3-8A338A7B8CB2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseOrderItemState LastPurchaseOrderItemState { get; set; }

        #region Allors
        [Id("C2A16E15-2AED-405D-8C0C-FF14DA14AF69")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Derived]
        public PurchaseOrderItemState PurchaseOrderItemState { get; set; }
        #endregion

        #region PurchaseOrderItemShipmentState
        #region Allors
        [Id("4E1A2881-B08E-4BC0-9B20-F08869DC4D45")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseOrderItemShipmentState PreviousPurchaseOrderItemShipmentState { get; set; }

        #region Allors
        [Id("3E1D4FC9-0364-45CF-AFB3-6583A27673FA")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseOrderItemShipmentState LastPurchaseOrderItemShipmentState { get; set; }

        #region Allors
        [Id("5F0A45CF-7EDD-4DFC-AA79-DC40E3470F7F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public PurchaseOrderItemShipmentState PurchaseOrderItemShipmentState { get; set; }
        #endregion

        #region PurchaseOrderItemPaymentState
        #region Allors
        [Id("E0331172-5CCE-44ED-9FFF-6CA1AD03EA0E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseOrderItemPaymentState PreviousPurchaseOrderItemPaymentState { get; set; }

        #region Allors
        [Id("FB765D9B-1A7A-4723-AB6A-FC99D38D302B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseOrderItemPaymentState LastPurchaseOrderItemPaymentState { get; set; }

        #region Allors
        [Id("37881CFB-C845-400A-A634-3811F190F401")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public PurchaseOrderItemPaymentState PurchaseOrderItemPaymentState { get; set; }
        #endregion

        #endregion

        #region Versioning
        #region Allors
        [Id("93C91DE0-2083-410F-A373-90C2C4AE999D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public PurchaseOrderItemVersion CurrentVersion { get; set; }

        #region Allors
        [Id("F9961466-5C49-4497-AD2A-26FEED74BE66")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public PurchaseOrderItemVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("b77555e9-b850-459b-9682-68859e7eeca4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Indexed]
        [Workspace(Default)]
        public InvoiceItemType InvoiceItemType { get; set; }

        #region Allors
        [Id("64e30c56-a77d-4ecf-b21e-e480dd5a25d8")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal QuantityReceived { get; set; }

        #region Allors
        [Id("e2dc0027-220b-4935-bc5a-cb2e2b6be248")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Part Part { get; set; }

        #region Allors
        [Id("318ba225-3a35-4f8a-8c05-51c97b21ebc7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Facility StoredInFacility { get; set; }

        #region Allors
        [Id("E0FC1C78-EE7A-499E-8D48-BFD846CCA47C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SerialisedItem SerialisedItem { get; set; }

        #region Allors
        [Id("5FE65A7F-F7EC-4910-AED7-35C88DED80C7")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string SerialNumber { get; set; }

        #region Allors
        [Id("56E8D200-1D63-4619-B617-CFA95B9CE07A")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool CanInvoice { get; set; }

        #region Allors
        [Id("ca16e968-9fa1-4e95-9405-f44271a2f724")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool IsReceivable { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Cancel() { }

        public void Reject() { }

        public void Approve() { }

        public void Delete() { }

        public void Reopen() { }

        public void DelegateAccess() { }

        #endregion

        #region Allors
        [Id("10FCCE86-96CC-440F-903A-2BB909373DC0")]
        [Workspace(Default)]
        #endregion
        public void QuickReceive() { }
    }
}

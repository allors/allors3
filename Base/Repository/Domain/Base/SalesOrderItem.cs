// <copyright file="SalesOrderItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("80de925c-04cc-412c-83a5-60405b0e63e6")]
    #endregion
    public partial class SalesOrderItem : OrderItem, Versioned
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

        public IrpfRegime IrpfRegime { get; set; }

        public IrpfRegime AssignedIrpfRegime { get; set; }

        public IrpfRate IrpfRate { get; set; }

        public decimal UnitIrpf { get; set; }

        public decimal TotalIrpf { get; set; }

        public string Message { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public decimal TotalDiscountAsPercentage { get; set; }

        public decimal UnitVat { get; set; }

        public VatRegime VatRegime { get; set; }

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

        public decimal GrandTotal{ get; set; }

        public Order SyncedOrder { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public Guid DerivationTrigger { get; set; }

        #endregion

        #region ObjectStates
        #region SalesOrderItemState
        #region Allors
        [Id("552F50AC-5ACD-4F8F-A6CD-68E0C3426F3B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderItemState PreviousSalesOrderItemState { get; set; }

        #region Allors
        [Id("FEBA7CC1-E449-4542-8159-71840DDE093B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderItemState LastSalesOrderItemState { get; set; }

        #region Allors
        [Id("0A44ABC2-41C2-444C-A20D-D2B5E387A611")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public SalesOrderItemState SalesOrderItemState { get; set; }
        #endregion

        #region SalesOrderItemPaymentState
        #region Allors
        [Id("C4C0E26C-C324-4391-B660-9DE9545C41DF")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderItemPaymentState PreviousSalesOrderItemPaymentState { get; set; }

        #region Allors
        [Id("5B59E71C-C568-4433-A398-0EAD573FAF92")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderItemPaymentState LastSalesOrderItemPaymentState { get; set; }

        #region Allors
        [Id("4DB38F8C-0903-4E05-9B4F-28EF7C3D9C01")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace]
        public SalesOrderItemPaymentState SalesOrderItemPaymentState { get; set; }
        #endregion

        #region SalesOrderItemInvoiceState

        #region Allors
        [Id("53D4E265-78FA-47E0-A452-03B55F4B1620")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderItemInvoiceState PreviousSalesOrderItemInvoiceState { get; set; }

        #region Allors
        [Id("8F1D474D-3807-4591-8016-21796AF89184")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderItemInvoiceState LastSalesOrderItemInvoiceState { get; set; }

        #region Allors
        [Id("219A398F-DDE3-4A50-A76D-34F7A1C086F7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace]
        public SalesOrderItemInvoiceState SalesOrderItemInvoiceState { get; set; }
        #endregion

        #region SalesOrderItemShipmentState
        #region Allors
        [Id("16139169-9E1C-4B3A-9635-660CA07F3190")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderItemShipmentState PreviousSalesOrderItemShipmentState { get; set; }

        #region Allors
        [Id("5F81EA61-A2C2-4E88-99C1-FE7DE0EBAB43")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesOrderItemShipmentState LastSalesOrderItemShipmentState { get; set; }

        #region Allors
        [Id("6F794926-3A3F-4701-ACA6-3D622BADAED6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace]
        public SalesOrderItemShipmentState SalesOrderItemShipmentState { get; set; }
        #endregion
        #endregion

        #region Versioning
        #region Allors
        [Id("6672E7FD-B5B7-41D6-8AFF-799045EBFC26")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace]
        public SalesOrderItemVersion CurrentVersion { get; set; }

        #region Allors
        [Id("97271467-852D-44E9-8D6E-6C5CC8EAABF0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace]
        public SalesOrderItemVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("C1F15DA3-9609-48C0-B319-C0A36418379B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Synced]
        [Workspace]
        public SalesOrderItemInventoryAssignment[] SalesOrderItemInventoryAssignments { get; set; }

        #region Allors
        [Id("1ea02a2c-280a-4a48-9ffb-1517789c56f1")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        public OrderItem[] OrderedWithFeatures { get; set; }

        #region Allors
        [Id("C0E36C78-95CD-4842-AFAA-137882E65214")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace]
        public SerialisedInventoryItem ReservedFromSerialisedInventoryItem { get; set; }

        #region Allors
        [Id("d7c25b48-d82f-4250-b09d-1e935eab665b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public NonSerialisedInventoryItem ReservedFromNonSerialisedInventoryItem { get; set; }

        #region Allors
        [Id("3e798309-d5d5-4860-87ec-ba3766e96c9e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        public NonSerialisedInventoryItem PreviousReservedFromNonSerialisedInventoryItem { get; set; }

        #region Allors
        [Id("8D14514A-190F-4B9F-ABE4-3E65C4337BB1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public SerialisedItemAvailability NextSerialisedItemAvailability { get; set; }

        #region Allors
        [Id("B9E742C3-F497-4663-9874-EB49DCB45BC0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public PostalAddress ShipFromAddress { get; set; }

        #region Allors
        [Id("5cc50f26-361b-46d7-a8e6-a9f53f7d2722")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace]
        public PostalAddress ShipToAddress { get; set; }

        #region Allors
        [Id("6826e05e-eb9a-4dc4-a653-0230dec934a9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        public Product PreviousProduct { get; set; }

        #region Allors
        [Id("7a8255f5-4283-4803-9f96-60a9adc2743b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace]
        public Party ShipToParty { get; set; }

        #region Allors
        [Id("7ae1b939-b387-4e6e-9da2-bc0364e04f7b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public PostalAddress AssignedShipToAddress { get; set; }

        #region Allors
        [Id("28104b69-ef65-47f7-96fe-e800c8803384")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal CostOfGoodsSold { get; set; }

        #region Allors
        [Id("545eb094-63d8-4d25-a069-7c3e91f26eb7")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal QuantityShipped { get; set; }

        #region Allors
        [Id("8145fbd3-a30f-44a0-9520-6b72ac20a82d")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal QuantityReturned { get; set; }

        #region Allors
        [Id("85d11891-5ffe-488f-9f23-5b2c7bc1c480")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal QuantityReserved { get; set; }

        #region Allors
        [Id("1e1ed439-ae25-4446-83e6-295d8627a7b5")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal QuantityShortFalled { get; set; }

        #region Allors
        [Id("b2f7dabb-8b87-41bc-8903-166d77bba1c5")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal QuantityPendingShipment { get; set; }

        #region Allors
        [Id("DA03C8C6-84F1-44B1-8007-2011D092D2C2")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal QuantityCommittedOut { get; set; }

        #region Allors
        [Id("b2d2645e-0d3f-473e-b277-6f890b9b911e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Party AssignedShipToParty { get; set; }

        #region Allors
        [Id("e8980105-2c4d-41de-bd67-802a8c0720f1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Product Product { get; set; }

        #region Allors
        [Id("E65F951A-2719-4010-A622-D781E26BFAD8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public SerialisedItem SerialisedItem { get; set; }

        #region Allors
        [Id("ed586b2f-c687-4d97-9416-52f7156b7b11")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public ProductFeature ProductFeature { get; set; }

        #region Allors
        [Id("f148e660-1e09-4e76-97fb-de62a7ee7482")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal QuantityRequestsShipping { get; set; }

        #region Allors
        [Id("9400A92D-DCA3-4F60-BC5C-05D4F32C337D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace]
        public InvoiceItemType InvoiceItemType { get; set; }

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
    }
}

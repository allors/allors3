// <copyright file="SalesOrderItemVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("CD97F8F9-C0E8-4E5F-8516-3F9FE6A4F0FC")]
    #endregion
    public partial class SalesOrderItemVersion : OrderItemVersion
    {
        #region inherited properties
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

        public DateTime DerivedDeliveryDate { get; set; }

        public SalesTerm[] SalesTerms { get; set; }

        public string ShippingInstruction { get; set; }

        public OrderItem[] Associations { get; set; }

        public string Message { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public User LastModifiedBy { get; set; }

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

        public decimal GrandTotal { get; set; }

        public decimal TotalSurchargeAsPercentage { get; set; }

        public decimal TotalDiscount { get; set; }

        public decimal TotalSurcharge { get; set; }

        public VatRegime AssignedVatRegime { get; set; }

        public decimal TotalBasePrice { get; set; }

        public decimal TotalExVat { get; set; }

        public IrpfRegime DerivedIrpfRegime { get; set; }

        public IrpfRegime AssignedIrpfRegime { get; set; }

        public IrpfRate IrpfRate { get; set; }

        public decimal UnitIrpf { get; set; }

        public decimal TotalIrpf { get; set; }

        #endregion

        #region Allors
        [Id("E9D53F26-6B36-4278-A3CE-CA9730458109")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public SalesOrderItemState SalesOrderItemState { get; set; }

        #region Allors
        [Id("054A3EB3-CC49-4377-B5FE-8361165F219B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SalesOrderItemShipmentState SalesOrderItemShipmentState { get; set; }

        #region Allors
        [Id("5B1EB657-791B-43F6-9106-2484B863082A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SalesOrderItemInvoiceState SalesOrderItemInvoiceState { get; set; }

        #region Allors
        [Id("0BA7F287-96FD-488C-9AED-493F90574CA5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SalesOrderItemPaymentState SalesOrderItemPaymentState { get; set; }

        #region Allors
        [Id("519B172B-C966-411A-852F-7486667975CB")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        public decimal QuantityShortFalled { get; set; }

        #region Allors
        [Id("AB660021-A95B-47D7-9627-D83DC4A053A4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public OrderItem[] OrderedWithFeatures { get; set; }

        #region Allors
        [Id("f7afe0e0-2460-41cd-8c1e-4146243370fb")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public SerialisedItem SerialisedItem { get; set; }

        #region Allors
        [Id("A442312C-71F5-4C96-ABF7-7C303A6E17F4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SerialisedInventoryItem ReservedFromSerialisedInventoryItem { get; set; }

        #region Allors
        [Id("219FE50D-5CEE-4266-93F7-496668668A00")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public NonSerialisedInventoryItem ReservedFromNonSerialisedInventoryItem { get; set; }

        #region Allors
        [Id("C90C49BE-C67A-4B66-B110-A7CF09D8235A")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public NonSerialisedInventoryItem PreviousReservedFromNonSerialisedInventoryItem { get; set; }

        #region Allors
        [Id("BBD58CD0-83BD-47B4-955F-95CC4A66E880")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SerialisedItemAvailability NextSerialisedItemAvailability { get; set; }

        #region Allors
        [Id("17BB3EF6-6368-436D-A7C4-5AD55889E2B7")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal QuantityShipped { get; set; }

        #region Allors
        [Id("6F3996FE-E7E2-44F0-90B5-BD6E59B108A7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress AssignedShipFromAddress { get; set; }

        #region Allors
        [Id("378fd06a-4651-4a62-84a5-1ff0bbf1dfbc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress DerivedShipFromAddress { get; set; }

        #region Allors
        [Id("0FF4C90C-EBD7-491D-B265-EF5083E4BB95")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress AssignedShipToAddress { get; set; }

        #region Allors
        [Id("38F38BF8-B735-4578-A2B1-2FD997A1FE3C")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public PostalAddress DerivedShipToAddress { get; set; }

        #region Allors
        [Id("1837DB18-F0D5-4A84-88B9-09EF35D98A24")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party AssignedShipToParty { get; set; }

        #region Allors
        [Id("5589BC3C-DD00-429A-92F5-7981228964DE")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party DerivedShipToParty { get; set; }

        #region Allors
        [Id("2BE18BDC-27ED-4E1B-8F7C-58CBF8E58ED3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public Product PreviousProduct { get; set; }

        #region Allors
        [Id("7A3ED514-0136-45BB-835B-05AA446052B7")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal QuantityReturned { get; set; }

        #region Allors
        [Id("4C235BA5-958D-4EA0-B08E-DD98000C2433")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal QuantityReserved { get; set; }

        #region Allors
        [Id("72C8B608-F504-4A0B-BBFF-144DED827625")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal QuantityPendingShipment { get; set; }

        #region Allors
        [Id("F79B3A47-46AE-4098-A4C4-21D098621F52")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Product Product { get; set; }

        #region Allors
        [Id("303E6727-E0E8-4D7E-B3F1-FF751B6BF285")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ProductFeature ProductFeature { get; set; }

        #region Allors
        [Id("ED8039CE-8212-43FA-8FD8-9CBD90970E49")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal QuantityRequestsShipping { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}

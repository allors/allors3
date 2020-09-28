// <copyright file="SalesInvoiceItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("a98f8aca-d711-47e8-ac9c-25b607cbaef1")]
    #endregion
    public partial class SalesInvoiceItem : InvoiceItem, Versioned
    {
        #region inherited properties

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public string InternalComment { get; set; }

        public SalesTerm[] SalesTerms { get; set; }

        public DiscountAdjustment[] DiscountAdjustments { get; set; }

        public SurchargeAdjustment[] SurchargeAdjustments { get; set; }

        public decimal TotalInvoiceAdjustment { get; set; }

        public InvoiceVatRateItem[] InvoiceVatRateItems { get; set; }

        public InvoiceItem AdjustmentFor { get; set; }

        public string Message { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal Quantity { get; set; }

        public string Description { get; set; }

        public Invoice SyncedInvoice { get; set; }

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

        public decimal GrandTotal { get; set; }

        public IrpfRegime IrpfRegime { get; set; }

        public IrpfRegime AssignedIrpfRegime { get; set; }

        public IrpfRate IrpfRate { get; set; }

        public decimal UnitIrpf { get; set; }

        public decimal TotalIrpf { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public Guid DerivationTrigger { get; set; }

        #endregion

        #region ObjectStates
        #region SalesInvoiceItemState
        #region Allors
        [Id("6033F4A9-9ABA-457C-9A44-218415E01B79")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesInvoiceItemState PreviousSalesInvoiceItemState { get; set; }

        #region Allors
        [Id("7623707D-C0F7-47D8-9B39-E34A55FC087B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SalesInvoiceItemState LastSalesInvoiceItemState { get; set; }

        #region Allors
        [Id("AC0B80C8-84C6-4A2D-8CE1-B94994537998")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SalesInvoiceItemState SalesInvoiceItemState { get; set; }
        #endregion
        #endregion

        #region Versioning
        #region Allors
        [Id("61008480-5266-42B1-BD09-477C514F5FC5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public SalesInvoiceItemVersion CurrentVersion { get; set; }

        #region Allors
        [Id("3409316D-FD54-408C-BC1C-9468CCE4B72E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public SalesInvoiceItemVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("D3D47236-8B21-420B-883A-C035EB0DBAE0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Product Product { get; set; }

        #region Allors
        [Id("44103C15-D438-433A-B157-3ACAA4544D29")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Part Part { get; set; }

        #region Allors
        [Id("6df95cf4-115f-4f43-aaea-52313c47d824")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public SerialisedItem SerialisedItem { get; set; }

        #region Allors
        [Id("f3516e3b-4bb6-45a2-b7e8-089f20d35648")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public SerialisedItemVersion SerialisedItemVersionBeforeSale { get; set; }

        #region Allors
        [Id("d1931e89-bbe7-4c90-8d22-564c4a8604d0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SerialisedItemAvailability NextSerialisedItemAvailability { get; set; }

        #region Allors
        [Id("0854aece-6ca1-4b8d-99a9-6d424de8dfd4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public ProductFeature[] ProductFeatures { get; set; }

        #region Allors
        [Id("6dd4e8ee-48ed-400d-a129-99a3a651586a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public InvoiceItemType InvoiceItemType { get; set; }

        #region Allors
        [Id("BB115D9A-53F8-4A3C-95F0-403A883C84FE")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Facility Facility { get; set; }

        #region Allors
        [Id("5e268182-f7f2-44aa-bb7c-7da8702bf5d2")]
        #endregion
        [Required]
        [Workspace(Default)]
        public decimal CostOfGoodsSold { get; set; }

        #region Allors

        [Id("5EFBB240-3B6B-47C4-8696-C7063ACBE074")]

        #endregion
        public void IsSubTotalItem()
        {
        }

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

        public void DelegateAccess() { }

        #endregion
    }
}


// <copyright file="PurchaseInvoiceItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("1ee19062-e36d-4836-b0e6-928a3957bd57")]
    #endregion
    public partial class PurchaseInvoiceItem : InvoiceItem, Versioned
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

        public User CreatedBy { get; set; }

        public IrpfRegime IrpfRegime { get; set; }

        public IrpfRegime AssignedIrpfRegime { get; set; }

        public IrpfRate IrpfRate { get; set; }

        public decimal UnitIrpf { get; set; }

        public decimal TotalIrpf { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public Guid DerivationTrigger { get; set; }

        #endregion

        #region ObjectStates
        #region PurchaseInvoiceItemState
        #region Allors
        [Id("7DDF09F9-1F6F-4863-9A6D-83172DDB20FE")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseInvoiceItemState PreviousPurchaseInvoiceItemState { get; set; }

        #region Allors
        [Id("4422FA5F-8DE7-4329-B0C0-5938CBB559A8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PurchaseInvoiceItemState LastPurchaseInvoiceItemState { get; set; }

        #region Allors
        [Id("C0467550-8CB5-4A65-BCBF-7A51D048529C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public PurchaseInvoiceItemState PurchaseInvoiceItemState { get; set; }
        #endregion
        #endregion

        #region Versioning
        #region Allors
        [Id("F41279D9-A0F9-44EB-857D-3C76D9CBE634")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public PurchaseInvoiceItemVersion CurrentVersion { get; set; }

        #region Allors
        [Id("E17BF428-BA56-451B-90D9-371CDA61E0E6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public PurchaseInvoiceItemVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("3DC3728F-748D-454F-8D5C-0F1BD5AE3855")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public PurchaseOrderItem PurchaseOrderItem { get; set; }

        #region Allors
        [Id("56e47122-faaa-4211-806c-1c19695fe434")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public InvoiceItemType InvoiceItemType { get; set; }

        #region Allors
        [Id("65eebcc4-d5ef-4933-8640-973b67c65127")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Part Part { get; set; }

        #region Allors
        [Id("2E3887BF-A895-4D25-9519-9B0ADA1DE6C4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Product ObsoleteProduct { get; set; }

        #region Allors
        [Id("B036079F-0B4A-4F7A-87A2-F3854A01F73A")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public SerialisedItem SerialisedItem { get; set; }

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

        #region Allors
        [Id("EE82F184-2FFD-4B9C-A0C5-556A76737591")]
        #endregion
        [Workspace(Default)]
        public void Reject() { }

        #region Allors
        [Id("f75e58d0-af6e-41d5-a606-d28ae53f63c8")]
        #endregion
        [Workspace(Default)]
        public void Revise() { }

        #region Allors
        [Id("eedc34ce-d944-435d-bc67-c57d695a6102")]
        #endregion
        [Workspace(Default)]
        public void FinishRevising() { }
    }
}

// <copyright file="PurchaseInvoiceItemVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("E431E1B1-9CF0-4BA2-AF57-E2EF8B8AE711")]
    #endregion
    public partial class PurchaseInvoiceItemVersion : InvoiceItemVersion
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

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
        [Id("F9AA8529-25F3-4374-9394-7B1A4868490A")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public PurchaseInvoiceItemState PurchaseInvoiceItemState { get; set; }

        #region Allors
        [Id("C534F912-F733-46A8-B54B-A4001DBB76FE")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public InvoiceItemType InvoiceItemType { get; set; }

        #region Allors
        [Id("D88804CF-E48A-4C7F-8916-0FC182BF43CA")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public PurchaseOrderItem PurchaseOrderItem { get; set; }

        #region Allors
        [Id("BBE7B96D-9B68-46E6-9158-743797C6AA43")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public Part Part { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}

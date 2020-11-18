// <copyright file="InvoiceItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("d79f734d-4434-4710-a7ea-7d6306f3064f")]
    #endregion
    public partial interface InvoiceItem : DelegatedAccessControlledObject, Priceable, Deletable
    {
        #region Allors
        [Id("39CB3BE2-2E0D-4124-8241-866860C2BDC0")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Size(-1)]
        [Workspace(Default)]
        string InternalComment { get; set; }

        #region Allors
        [Id("067674d0-6d9b-4a7e-b0c6-62c24f3a4815")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        SalesTerm[] SalesTerms { get; set; }

        #region Allors
        [Id("1f92aed8-8a8f-4eb6-8102-83a6395788d6")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalInvoiceAdjustment { get; set; }

        #region Allors
        [Id("33caab05-ec61-4cf9-b903-b5d5a8d7eef9")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        InvoiceVatRateItem[] InvoiceVatRateItems { get; set; }

        #region Allors
        [Id("475d7a79-27a1-4d5a-90c1-3896fa2e892e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        InvoiceItem AdjustmentFor { get; set; }

        #region Allors
        [Id("7eed800d-c2b5-4837-a288-150803578b27")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Message { get; set; }

        #region Allors
        [Id("8fd19791-85ed-44c9-8580-a6768578ca3a")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal AmountPaid { get; set; }

        #region Allors
        [Id("ba90acfe-0d55-4854-a046-35279f872e0b")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal Quantity { get; set; }

        #region Allors
        [Id("fb202916-1a87-439e-b2d8-b3f3ed4f681a")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("4B19B32A-1B6F-478A-8376-779A32AB6386")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Synced]
        Invoice SyncedInvoice { get; set; }

        #region Allors
        [Id("124e6101-f1df-427e-bd19-9c8fe69ff3ee")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime DerivedIrpfRegime { get; set; }

        #region Allors
        [Id("92e1308f-53d2-449d-aa26-6d1fd0daa7d3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime AssignedIrpfRegime { get; set; }

        #region Allors
        [Id("3a068974-bf77-448b-9f60-3bf4f27a85ad")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        IrpfRate IrpfRate { get; set; }

        #region Allors
        [Id("5210f7b3-0902-4e40-9178-c61ecd634726")]
        #endregion
        [Required]
        [Derived]
        [Precision(19)]
        [Scale(5)]
        [Workspace(Default)]
        decimal UnitIrpf { get; set; }

        #region Allors
        [Id("e7ef0a88-bfaf-420b-9979-6647ea1aa022")]
        #endregion
        [Required]
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalIrpf { get; set; }

        #region Allors
        [Id("54d0ec1c-b826-41e3-946b-06adc27c4b8e")]
        #endregion
        [Required]
        [Workspace(Default)]
        public Guid DerivationTrigger { get; set; }
    }
}

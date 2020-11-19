// <copyright file="InvoiceItemVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("D8B0BB9D-BD15-426C-A4A4-6AEEFF2FBDB2")]
    #endregion
    public partial interface InvoiceItemVersion : PriceableVersion
    {
        #region Allors
        [Id("9D47BAEB-ED16-4314-84EA-4A462E554823")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Size(-1)]
        [Workspace(Default)]
        string InternalComment { get; set; }

        #region Allors
        [Id("83241B88-9F30-4732-9755-03B27A6DC1F8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        SalesTerm[] SalesTerms { get; set; }

        #region Allors
        [Id("5C0780BC-5580-41AF-BA01-8643B93FF4F7")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalInvoiceAdjustment { get; set; }

        #region Allors
        [Id("DA6D40BB-C45E-46A5-802A-94DFFB535870")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        InvoiceVatRateItem[] InvoiceVatRateItems { get; set; }

        #region Allors
        [Id("14C11E41-00AA-4406-9D55-887B2DF66C6C")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        InvoiceItem AdjustmentFor { get; set; }

        #region Allors
        [Id("8738D82C-99E0-4EAE-BC4F-77B2F8D04E8D")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Message { get; set; }

        #region Allors
        [Id("AD28ED60-187C-4722-A41F-2372B274B193")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal AmountPaid { get; set; }

        #region Allors
        [Id("A6FBC8B3-2A1B-47B5-813C-B58C9C84FBDD")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal Quantity { get; set; }

        #region Allors
        [Id("676378E4-F8AB-41AE-82F0-9F143F8A9A36")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("697df771-9dcd-4cfb-a4cb-fbe023aa9515")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime DerivedIrpfRegime { get; set; }

        #region Allors
        [Id("caece89f-9811-4790-911d-54e5fda82265")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime AssignedIrpfRegime { get; set; }

        #region Allors
        [Id("f48e4902-aabd-4e2c-ad5f-4a45ea05426b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        IrpfRate IrpfRate { get; set; }

        #region Allors
        [Id("1942c162-2af7-48f9-95ec-362697c5bb62")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(5)]
        [Workspace(Default)]
        decimal UnitIrpf { get; set; }

        #region Allors
        [Id("d9463742-8414-441f-88ef-90938293296e")]
        #endregion
        [Required]
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalIrpf { get; set; }
    }
}

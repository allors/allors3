// <copyright file="InvoiceVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("683DF770-70BE-4E31-830A-B9DD9031030D")]
    #endregion
    public partial interface InvoiceVersion : Version
    {
        #region Allors
        [Id("3C21D07A-3723-4D0C-BCDF-2A699DB6794F")]
        [Indexed]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Comment { get; set; }

        #region Allors
        [Id("85356434-312C-4147-8D32-D27BE1B103B6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        User CreatedBy { get; set; }

        #region Allors
        [Id("1E73052D-5404-4D10-B353-493906CED780")]
        #endregion
        [Workspace(Default)]
        DateTime CreationDate { get; set; }

        #region Allors
        [Id("93C86CD1-0329-4303-8EC4-DAAB6BF451C1")]
        #endregion
        [Workspace(Default)]
        DateTime LastModifiedDate { get; set; }

        #region Allors
        [Id("3A694B6A-A891-418C-9A7C-3709978B7761")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string InternalComment { get; set; }

        #region Allors
        [Id("2B43A67D-2E2A-4D10-B4A7-024C85B5FC74")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Derived]
        [Indexed]
        Currency AssignedCurrency { get; set; }

        #region Allors
        [Id("6F65890E-B409-4A70-887A-778C6703AEFB")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("56151707-E3E4-4CE0-AB6A-63C64DD42DC6")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string CustomerReference { get; set; }

        #region Allors
        [Id("6C96867C-D07F-4AB4-BD7D-5E622A5B55BE")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal AmountPaid { get; set; }

        #region Allors
        [Id("CCFACA15-C8BC-4260-AA9E-0B4739281590")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalDiscount { get; set; }

        #region Allors
        [Id("49FA267E-4801-4347-852C-BFA556FDB4B5")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        BillingAccount BillingAccount { get; set; }

        #region Allors
        [Id("5913C137-F2C7-4C77-B005-2935853CE2A8")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalIncVat { get; set; }

        #region Allors
        [Id("D17019EC-A2AB-4063-B862-EF95A867CE61")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalSurcharge { get; set; }

        #region Allors
        [Id("3D03D4F2-E82C-4802-BABC-ADD692925C44")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalBasePrice { get; set; }

        #region Allors
        [Id("d60855bf-af6c-4871-927f-9e1ac689979a")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal GrandTotal { get; set; }

        #region Allors
        [Id("4F0BDB92-1D82-4489-AF4B-35000DC8EA6A")]
        #endregion
        [Workspace(Default)]
        DateTime InvoiceDate { get; set; }

        #region Allors
        [Id("F4B96308-2791-4A0B-A83D-462D22141968")]
        #endregion
        [Derived]
        [Workspace(Default)]
        DateTime EntryDate { get; set; }

        #region Allors
        [Id("D5A20009-50CC-4605-AAA1-554C6B478931")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalShippingAndHandling { get; set; }

        #region Allors
        [Id("2B3F165E-27C4-461B-9A0B-6107CEC37200")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalExVat { get; set; }

        #region Allors
        [Id("1a9136c4-1d51-4db7-a434-97e4d87bdba1")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Indexed]
        OrderAdjustment[] OrderAdjustments { get; set; }

        #region Allors
        [Id("E6F77579-FB47-42B1-8A0C-D699A491AA18")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        SalesTerm[] SalesTerms { get; set; }

        #region Allors
        [Id("05D98EF9-5464-4C37-A833-47B76DA57F24")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string InvoiceNumber { get; set; }

        #region Allors
        [Id("CCCAFE62-A111-4000-852C-1621B5B009EA")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Message { get; set; }

        #region Allors
        [Id("141D8BBF-5EE1-4521-BFBC-34D9F69BEADA")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        VatRegime AssignedVatRegime { get; set; }

        #region Allors
        [Id("850af264-a601-4070-ae75-1c608aacb0dc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        VatRegime DerivedVatRegime { get; set; }

        #region Allors
        [Id("613afd54-e378-4998-ba2d-777f691c0cf7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime AssignedIrpfRegime { get; set; }

        #region Allors
        [Id("c7f5396d-026c-4036-916c-c3b91b7fa288")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime DerivedIrpfRegime { get; set; }

        #region Allors
        [Id("070DDB3E-F6F6-42C1-B723-4D8C406BD2E8")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalVat { get; set; }

        #region Allors
        [Id("7E572C4E-C2B5-4008-93CC-D9F909EAF0C6")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalFee { get; set; }

        #region Allors
        [Id("e1528797-7ff6-41cc-aa0e-b043d7a1f1c4")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalExtraCharge { get; set; }
    }
}

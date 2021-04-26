// <copyright file="QuoteVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("4C239D5F-85BF-4B8B-A2E6-609A2FE672B8")]
    #endregion
    public partial interface QuoteVersion : Version
    {
        #region Allors
        [Id("84c2fda0-76c0-4366-a198-49379431fb0f")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        InternalOrganisation Issuer { get; set; }

        #region Allors
        [Id("EC36228F-7688-4C5F-9BF1-A05EE82E1B64")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        QuoteState QuoteState { get; set; }

        #region Allors
        [Id("26E0C5B6-05F4-4D18-9B2F-FAB56E057C76")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string InternalComment { get; set; }

        #region Allors
        [Id("93DD5B53-08C6-40A9-AD40-FE547C34FE7A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        DateTime RequiredResponseDate { get; set; }

        #region Allors
        [Id("1D8BE58D-45CE-446D-8508-3A95F500EB78")]
        #endregion
        [Workspace(Default)]
        DateTime ValidFromDate { get; set; }

        #region Allors
        [Id("62A02B3A-03DE-415F-8083-DEDD9D4BDCFE")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        QuoteTerm[] QuoteTerms { get; set; }

        #region Allors
        [Id("BFB459E5-E28A-4EE7-ABAD-06A06919C4C4")]
        #endregion
        [Workspace(Default)]
        DateTime ValidThroughDate { get; set; }

        #region Allors
        [Id("ACF13AE8-CD1A-48BD-BABF-EA4EB67671D2")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("89535B62-F3B5-488B-BE3E-B8F200B6F5C9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Party Receiver { get; set; }

        #region Allors
        [Id("3654A64D-43C6-4703-850D-2DE1B4A1127C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        ContactMechanism FullfillContactMechanism { get; set; }

        #region Allors
        [Id("1927FAF4-B595-4F53-8BD4-97DD1865187A")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        VatRegime AssignedVatRegime { get; set; }

        #region Allors
        [Id("74fc94e4-35cd-44ed-87c5-af24de72f53e")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        VatRegime DerivedVatRegime { get; set; }

        #region Allors
        [Id("15421231-909B-4374-8EFB-E8C717F242D7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        VatClause AssignedVatClause { get; set; }

        #region Allors
        [Id("21D1EA49-C7FB-4771-A669-E2DE3F533AE2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        VatClause DerivedVatClause { get; set; }

        #region Allors
        [Id("1306f14d-ad14-4792-ac9a-c458a749181b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime DerivedIrpfRegime { get; set; }

        #region Allors
        [Id("57b6f2e5-2c3e-46f8-b47f-8e0b27fffc2e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime AssignedIrpfRegime { get; set; }

        #region Allors
        [Id("67c226ab-5526-4101-af56-bd216a50555a")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalIrpf { get; set; }

        #region Allors
        [Id("EC2A3D0E-6733-402C-AD44-3010A960064D")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalExVat { get; set; }

        #region Allors
        [Id("2DD9990F-214C-4810-8F97-506FEC845F3C")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalVat { get; set; }

        #region Allors
        [Id("6DC1543C-6F15-41A3-8906-2967DE811497")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalIncVat { get; set; }

        #region Allors
        [Id("84E463DA-DCCE-4469-A4E2-A66BCAE9CB8E")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalSurcharge { get; set; }

        #region Allors
        [Id("EA5BE89C-3CB8-4077-8D7E-29A51DED5225")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalDiscount { get; set; }

        #region Allors
        [Id("D1AFA863-7983-459D-828A-4CE90722583A")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalShippingAndHandling { get; set; }

        #region Allors
        [Id("BA3800D3-E37B-4479-AB9A-2DD49FFBF131")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalFee { get; set; }

        #region Allors
        [Id("dae828b3-da1e-4858-8353-6d5b69f2380c")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalExtraCharge { get; set; }

        #region Allors
        [Id("2F081F44-9627-4A61-8045-BDA8A9754B2C")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalBasePrice { get; set; }

        #region Allors
        [Id("4E9D5109-04B6-4057-B35A-9E1BFA149CEE")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalListPrice { get; set; }

        #region Allors
        [Id("4f4fef59-a335-45e3-9aff-b2111aeba9e5")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Indexed]
        OrderAdjustment[] OrderAdjustments { get; set; }

        #region Allors
        [Id("543adf0a-7a2d-4a6f-b220-4db487062e62")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal GrandTotal { get; set; }

        #region Allors
        [Id("7C3916BA-3406-4DF3-88C3-FC8DABC9E888")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Currency AssignedCurrency { get; set; }

        #region Allors
        [Id("798B7FF6-2E3A-4ACD-8FF3-98E110FBC1DC")]
        #endregion
        [Workspace(Default)]
        DateTime IssueDate { get; set; }

        #region Allors
        [Id("A0BA1842-CC3D-49C3-A9A2-52DA6AA21CB8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        QuoteItem[] QuoteItems { get; set; }

        #region Allors
        [Id("5A8F66EF-95AB-45AA-8D0F-2FA441D01937")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string QuoteNumber { get; set; }

        #region Allors
        [Id("92C78C7A-3844-4277-AD94-8A67D8E6C267")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Request Request { get; set; }

        #region Allors
        [Id("0e029726-8764-4459-bc86-13086ea0e0fb")]
        #endregion
        [Required]
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        decimal TotalIrpfInPreferredCurrency { get; set; }

        #region Allors
        [Id("2110f733-70d9-490c-835e-af87b6aeabf3")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalExVatInPreferredCurrency { get; set; }

        #region Allors
        [Id("c41cae73-1f1d-4949-81f8-e85f3081f450")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalVatInPreferredCurrency { get; set; }

        #region Allors
        [Id("395e4323-e34c-4a6b-ad37-804a3bc2cb73")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalIncVatInPreferredCurrency { get; set; }

        #region Allors
        [Id("0e380790-8342-4002-8705-a88c1088b9ed")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalSurchargeInPreferredCurrency { get; set; }

        #region Allors
        [Id("6b6eb5cc-ecc0-440c-a982-a35a08107753")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalDiscountInPreferredCurrency { get; set; }

        #region Allors
        [Id("71f26581-c272-4572-9873-9bc14eea7da2")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalShippingAndHandlingInPreferredCurrency { get; set; }

        #region Allors
        [Id("95136d43-4e00-4e96-87de-ea7f38f0c8ec")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalFeeInPreferredCurrency { get; set; }

        #region Allors
        [Id("cf6acd36-ced8-474c-ba60-38150ff288df")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        decimal TotalExtraChargeInPreferredCurrency { get; set; }

        #region Allors
        [Id("54082bce-96ee-4d1e-add0-c613b07d022c")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalBasePriceInPreferredCurrency { get; set; }

        #region Allors
        [Id("11582625-fa3b-49f1-814e-c280f3aaa3fa")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        decimal TotalListPriceInPreferredCurrency { get; set; }

        #region Allors
        [Id("e85e3b3b-47bd-4a4e-a652-b7017714409f")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        decimal GrandTotalInPreferredCurrency { get; set; }
    }
}

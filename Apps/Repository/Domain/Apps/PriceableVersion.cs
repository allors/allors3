// <copyright file="PriceableVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("7CFD2B40-4C45-489B-9F92-83E7E1641F19")]
    #endregion
    public partial interface PriceableVersion : Version
    {
        #region Allors
        [Id("A2B92C8C-3263-47AE-9345-16A04A4A5AEC")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        decimal TotalDiscountAsPercentage { get; set; }

        #region Allors
        [Id("19eaba19-7179-4511-87b8-7a8f48e4a7a3")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        DiscountAdjustment[] DiscountAdjustments { get; set; }

        #region Allors
        [Id("9db9535d-9dc9-4bbf-8a4f-aae8839383fe")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        SurchargeAdjustment[] SurchargeAdjustments { get; set; }

        #region Allors
        [Id("DBB03B6B-93EA-41FD-838D-D89C38D192FA")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal UnitVat { get; set; }

        #region Allors
        [Id("C087BCE3-6075-48FC-9D71-5EC4B8956E21")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        VatRegime VatRegime { get; set; }

        #region Allors
        [Id("D05C5D37-1915-445C-AC89-0CF1E0D1636E")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalVat { get; set; }

        #region Allors
        [Id("949C917B-718B-44E9-8E00-C620ECDDB003")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal UnitSurcharge { get; set; }

        #region Allors
        [Id("EDFC927F-DFD4-4CAF-B7AE-7349A94BBF20")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal UnitDiscount { get; set; }

        #region Allors
        [Id("35034B55-9903-4561-B195-19A7865D09BA")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        VatRate VatRate { get; set; }

        #region Allors
        [Id("AE5ECB96-912C-49FC-9F99-4EC3D5B86406")]
        #endregion
        [Precision(19)]
        [Scale(4)]
        decimal AssignedUnitPrice { get; set; }

        #region Allors
        [Id("BBA7B69A-7D79-4E6E-B39C-891A8DF36148")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(4)]
        decimal UnitBasePrice { get; set; }

        #region Allors
        [Id("5AABEDB7-9690-402A-9188-1929BAA75D89")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(4)]
        decimal UnitPrice { get; set; }

        #region Allors
        [Id("C75F67AF-3875-46F7-9B4D-C569799821E2")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalIncVat { get; set; }

        #region Allors
        [Id("D2212BA5-C309-4909-B584-6B118D51DAB7")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        decimal TotalSurchargeAsPercentage { get; set; }

        #region Allors
        [Id("9BA0BE40-56FB-4F50-AA08-B5FDE6D8B36B")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalDiscount { get; set; }

        #region Allors
        [Id("785DD408-CE58-44FC-8F16-66FFBEA6DF0E")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalSurcharge { get; set; }

        #region Allors
        [Id("6D73797C-EC9F-4C59-9234-4E67756AE365")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        VatRegime AssignedVatRegime { get; set; }

        #region Allors
        [Id("5AE60592-1B86-4C71-860D-A3C0CFC1C647")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalBasePrice { get; set; }

        #region Allors
        [Id("D9E651C1-F508-402E-B599-6EE3206B5924")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalExVat { get; set; }

        #region Allors
        [Id("38c29c2f-94a7-4e71-9282-b91b67ef3368")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal GrandTotal { get; set; }
    }
}

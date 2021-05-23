// <copyright file="OrderVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("2D3761C6-CF53-4CA8-9392-99ED6B01EDE1")]
    #endregion
    public partial interface OrderVersion : Version
    {
        #region Allors
        [Id("C1A57736-7027-4944-BF06-7CB5E513CCBE")]
        [Indexed]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Comment { get; set; }

        #region Allors
        [Id("CECB258A-5B25-4AB5-9F27-924CB9CD8080")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        User CreatedBy { get; set; }

        #region Allors
        [Id("95FAE69F-0298-425A-AB74-7D553056624A")]
        #endregion
        [Workspace(Default)]
        DateTime CreationDate { get; set; }

        #region Allors
        [Id("16365C4A-C5F8-4004-9384-77E4E42E6151")]
        #endregion
        [Workspace(Default)]
        DateTime LastModifiedDate { get; set; }

        #region Allors
        [Id("812FD6D2-13B6-42A8-AE84-355E6DE25D86")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string InternalComment { get; set; }

        #region Allors
        [Id("60017934-BF46-485D-8844-E2FC521B6604")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Currency AssignedCurrency { get; set; }

        #region Allors
        [Id("B3F0CB65-DEDF-4E81-998E-7766E8AC63C9")]
        #endregion
        [Workspace(Default)]
        [Size(256)]
        string CustomerReference { get; set; }

        #region Allors
        [Id("4A6AC333-A672-459C-929D-A999A8DBBF08")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalExVat { get; set; }

        #region Allors
        [Id("2d710be9-39be-4a8b-a88d-ceb960691acb")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Indexed]
        OrderAdjustment[] OrderAdjustments { get; set; }

        #region Allors
        [Id("83929DB1-5DFC-4D9F-A10E-C46CBD55ACA1")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToMany)]
        SalesTerm[] SalesTerms { get; set; }

        #region Allors
        [Id("057978E2-F3A3-4BD6-BF1C-A6D27EC6C154")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalVat { get; set; }

        #region Allors
        [Id("24c94389-7016-433c-81b0-d63eea705d20")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalIrpf { get; set; }

        #region Allors
        [Id("12DE1B8C-8488-4C18-B048-753640329AA6")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalSurcharge { get; set; }

        #region Allors
        [Id("AD239348-CADB-4784-888A-68DBD082B7AA")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        OrderItem[] ValidOrderItems { get; set; }

        #region Allors
        [Id("EC48639A-EB57-465D-8FE7-68BF15586F6B")]
        #endregion
        [Workspace(Default)]
        [Size(256)]
        string OrderNumber { get; set; }

        #region Allors
        [Id("23891E65-CF51-4B93-893A-BA2D17346F67")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalDiscount { get; set; }

        #region Allors
        [Id("654AB214-8214-4F86-9AE0-3DDCF8C10031")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string Message { get; set; }

        #region Allors
        [Id("6D2AE22D-39BC-4A92-BE31-F3C30326498B")]
        #endregion
        [Workspace(Default)]
        DateTime EntryDate { get; set; }

        #region Allors
        [Id("B6C314CD-6BBE-4D6A-9FDC-F19A18C128D8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        OrderKind OrderKind { get; set; }

        #region Allors
        [Id("5963F652-CA7A-4871-AD3C-DD31B8AD3385")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalIncVat { get; set; }

        #region Allors
        [Id("7166CBA0-82B6-4332-9D52-3874CDAF72CD")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        VatRegime AssignedVatRegime { get; set; }

        #region Allors
        [Id("3606221e-2a60-406f-9829-4c9e6efad01d")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        VatRegime DerivedVatRegime { get; set; }

        #region Allors
        [Id("360268c9-501e-4f13-8ea0-01cce00197f5")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime AssignedIrpfRegime { get; set; }

        #region Allors
        [Id("edde2f2b-d3b8-4ff9-ab70-5103fd052977")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime DerivedIrpfRegime { get; set; }

        #region Allors
        [Id("1ABA537A-2AC5-48BE-A4CA-D39B8811F10E")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalShippingAndHandling { get; set; }

        #region Allors
        [Id("eb5fad3d-e047-4efc-bd2e-b9b874dc6cc8")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal GrandTotal { get; set; }

        #region Allors
        [Id("D2B76765-60EC-4D08-9AE5-26398DC0A7A2")]
        #endregion
        [Workspace(Default)]
        DateTime OrderDate { get; set; }

        #region Allors
        [Id("393069B8-9180-41B7-9D5D-40DC0EF16928")]
        #endregion
        [Workspace(Default)]
        DateTime DeliveryDate { get; set; }

        #region Allors
        [Id("49289FBF-480E-49BC-AE23-555F95D4D868")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalBasePrice { get; set; }

        #region Allors
        [Id("20833465-873E-4DA6-B769-E8CD56A4D809")]
        #endregion
        [Workspace(Default)]
        [Precision(19)]
        [Scale(2)]
        decimal TotalFee { get; set; }

        #region Allors
        [Id("c53a554d-eb77-4247-8eb7-ae5b353da13f")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalExtraCharge { get; set; }

        #region Allors
        [Id("2be7e4c6-7c6f-40c9-908d-7e8998f498e7")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        decimal TotalIrpfInPreferredCurrency { get; set; }

        #region Allors
        [Id("947906e5-e45d-4827-b866-787274b6c91c")]
        #endregion
        [Workspace]
        [Precision(19)]
        [Scale(2)]
        decimal TotalExVatInPreferredCurrency { get; set; }

        #region Allors
        [Id("67a766ac-d3e7-43b4-9af3-9a0f6b1378de")]
        #endregion
        [Workspace]
        [Precision(19)]
        [Scale(2)]
        decimal TotalVatInPreferredCurrency { get; set; }

        #region Allors
        [Id("9b19b27a-b05b-4624-86b9-638fa2516d6d")]
        #endregion
        [Workspace]
        [Precision(19)]
        [Scale(2)]
        decimal TotalIncVatInPreferredCurrency { get; set; }

        #region Allors
        [Id("deb8b328-30c9-44dc-be73-dbe1c9821a43")]
        #endregion
        [Workspace]
        [Precision(19)]
        [Scale(2)]
        decimal TotalSurchargeInPreferredCurrency { get; set; }

        #region Allors
        [Id("8162085b-dd62-4346-9ab3-4219a761003f")]
        #endregion
        [Workspace]
        [Precision(19)]
        [Scale(2)]
        decimal TotalDiscountInPreferredCurrency { get; set; }

        #region Allors
        [Id("ec64d948-8571-4c03-90fa-3ae6ae8c62bf")]
        #endregion
        [Workspace]
        [Precision(19)]
        [Scale(2)]
        decimal TotalShippingAndHandlingInPreferredCurrency { get; set; }

        #region Allors
        [Id("157c7464-5acc-444b-9c16-33076d8b4ea4")]
        #endregion
        [Workspace]
        [Precision(19)]
        [Scale(2)]
        decimal TotalFeeInPreferredCurrency { get; set; }

        #region Allors
        [Id("a5a198bf-b07b-4d71-a166-cd14c7cc120e")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        decimal TotalExtraChargeInPreferredCurrency { get; set; }

        #region Allors
        [Id("033b8379-6f93-4915-b4fb-a9d27b205f3f")]
        #endregion
        [Workspace]
        [Precision(19)]
        [Scale(2)]
        decimal TotalBasePriceInPreferredCurrency { get; set; }

        #region Allors
        [Id("a6bf479b-d092-4a99-af26-91fe8c1b03d3")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        decimal TotalListPriceInPreferredCurrency { get; set; }

        #region Allors
        [Id("01340397-9f93-4137-8f2a-93fe063c96db")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        decimal GrandTotalInPreferredCurrency { get; set; }
    }
}

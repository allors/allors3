// <copyright file="Quote.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("066bf242-2710-4a68-8ff6-ce4d7d88a04a")]
    #endregion
    public partial interface Quote : Transitional, WorkItem, Printable, Auditable, Commentable, Localised, Deletable
    {
        #region ObjectStates
        #region QuoteState
        #region Allors
        [Id("B1792FCE-33EF-4A03-BCB7-92E839A55B2C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        QuoteState PreviousQuoteState { get; set; }

        #region Allors
        [Id("C1B9AD76-9773-4A52-AADB-ED3E7222C89B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        QuoteState LastQuoteState { get; set; }

        #region Allors
        [Id("2A4AADE6-B3F0-436B-BA9E-5D0ECB958077")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        QuoteState QuoteState { get; set; }
        #endregion
        #endregion

        #region Allors
        [Id("AFB30FBE-9E93-4EBD-B8D3-5C5B231D70E1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        InternalOrganisation Issuer { get; set; }

        #region Allors
        [Id("BA16DE57-19A1-40BC-AF3C-99690EB5ECAB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Currency AssignedCurrency { get; set; }

        #region Allors
        [Id("0e251edc-35aa-499e-89f3-acc28ab82e6e")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        Currency DerivedCurrency { get; set; }

        #region Allors
        [Id("3B913CC6-C627-4F16-ACF5-98EC97CE5FDA")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string InternalComment { get; set; }

        #region Allors
        [Id("D566DD5B-BF58-45A6-A68F-FD7D2652FB4D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        DateTime RequiredResponseDate { get; set; }

        #region Allors
        [Id("b5ecffab-0f27-4311-9f66-197f0cdc147f")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Derived]
        [Indexed]
        QuoteItem[] ValidQuoteItems { get; set; }

        #region Allors
        [Id("033df6dd-fdf7-44e4-84ca-5c7e100cb3f5")]
        #endregion
        [Workspace(Default)]
        DateTime ValidFromDate { get; set; }

        #region Allors
        [Id("05e3454a-0a7a-488d-b4b1-f0fd41392ddf")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        QuoteTerm[] QuoteTerms { get; set; }

        #region Allors
        [Id("2140e106-2ef3-427a-be94-458c2b8e154d")]
        #endregion
        [Workspace(Default)]
        DateTime ValidThroughDate { get; set; }

        #region Allors
        [Id("3da51ccc-24b9-4b03-9218-7da06492224d")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("9119c598-cd98-43da-bfdf-1e6573112c9e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        Party Receiver { get; set; }

        #region Allors
        [Id("A1C248DF-7F2A-4622-9052-9106C67B1D71")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        ContactMechanism FullfillContactMechanism { get; set; }

        #region Allors
        [Id("37D046B8-3804-4912-9B53-C98D66A67BC0")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        VatRegime AssignedVatRegime { get; set; }

        #region Allors
        [Id("164735dd-c7a4-4e8a-8e17-9be8bd9a29e7")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        VatRegime DerivedVatRegime { get; set; }

        #region Allors
        [Id("9810d19b-6728-4e8b-91c0-050a9d363b1e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Workspace]
        VatRate DerivedVatRate { get; set; }

        #region Allors
        [Id("0B00B80D-1A5C-4CB0-A50A-B6E552A1AF6F")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        VatClause AssignedVatClause { get; set; }

        #region Allors
        [Id("3B47127B-7C65-4891-8D24-4622D7573EC5")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        VatClause DerivedVatClause { get; set; }

        #region Allors
        [Id("d1366534-36fa-4b64-9488-c5da5d083dfb")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Workspace(Default)]
        IrpfRegime DerivedIrpfRegime { get; set; }

        #region Allors
        [Id("07efb261-438e-4f03-831f-fbc11ab944f2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        IrpfRegime AssignedIrpfRegime { get; set; }

        #region Allors
        [Id("f0fe835b-bf4a-4e17-b492-c7691ee26405")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Workspace]
        IrpfRate DerivedIrpfRate { get; set; }

        #region Allors
        [Id("37726ca4-936c-4dcc-b07a-a20c483f5890")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public Locale DerivedLocale { get; set; }

        #region Allors
        [Id("7f8b987f-85fd-44f9-8218-1f4d136e4d1d")]
        #endregion
        [Required]
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalIrpf { get; set; }

        #region Allors
        [Id("E38FDC05-A2BB-4E37-9B92-7976AEB5AD4E")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalExVat { get; set; }

        #region Allors
        [Id("2BB55996-6992-4960-ADD3-7B2AB846DBC3")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalVat { get; set; }

        #region Allors
        [Id("2163DB4B-684D-45BF-B56A-99D5311DDC52")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalIncVat { get; set; }

        #region Allors
        [Id("DB9E318C-FE6E-4A84-89DA-AD02EE9C3266")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalSurcharge { get; set; }

        #region Allors
        [Id("81140E8D-B94B-47AD-B478-E3AD91C1E66E")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalDiscount { get; set; }

        #region Allors
        [Id("3FD4A223-4EDB-44B7-8AE4-CC9B0AA9FEEE")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalShippingAndHandling { get; set; }

        #region Allors
        [Id("7B8FB84F-02FD-4616-BD09-2D7E421FBB5B")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalFee { get; set; }

        #region Allors
        [Id("0441b70f-597c-4e71-8480-bcbe6b520152")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalExtraCharge { get; set; }

        #region Allors
        [Id("B02CBBD4-0D39-4851-80FA-E0727CF38353")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalBasePrice { get; set; }

        #region Allors
        [Id("706BEBDC-C2E3-4534-B94C-B6A8D3222BA1")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalListPrice { get; set; }

        #region Allors
        [Id("a59e8de9-9caa-41ea-942a-229d18339e7c")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal GrandTotal { get; set; }

        #region Allors
        [Id("1d41aa3d-3db8-444b-923e-e1d3057c4d31")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Indexed]
        OrderAdjustment[] OrderAdjustments { get; set; }

        #region Allors
        [Id("d7dc81e8-76e7-4c68-9843-a2aaf8293510")]
        #endregion
        [Workspace(Default)]
        [Required]
        DateTime IssueDate { get; set; }

        #region Allors
        [Id("e250154a-77c5-4a0b-ae3d-28668a9037d1")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        QuoteItem[] QuoteItems { get; set; }

        #region Allors
        [Id("e76cbd73-78b7-4ef8-a24c-9ac0db152f7f")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        string QuoteNumber { get; set; }

        #region Allors
        [Id("94DE208B-5FF9-45F5-BD35-5BB7D7B33FB7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        Request Request { get; set; }

        #region Allors
        [Id("2D6804B9-A745-497A-9F43-FADE6B1B76AB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Person ContactPerson { get; set; }

        #region Allors
        [Id("c44822c3-beb9-4eab-bfe0-f8f270460f26")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        int SortableQuoteNumber { get; set; }

        #region Allors
        [Id("bd1de829-0619-4e3c-a104-89bcb5ab1f4d")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public Media[] ElectronicDocuments { get; set; }

        #region Allors
        [Id("24343b2e-6b55-421a-b5b4-ac9b8fb76a8f")]
        #endregion
        [Required]
        [Workspace(Default)]
        public Guid DerivationTrigger { get; set; }

        #region Allors
        [Id("2e44c8f9-5da2-4027-b896-ae155b17be09")]
        #endregion
        [Required]
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        decimal TotalIrpfInPreferredCurrency { get; set; }

        #region Allors
        [Id("7aa1987c-cce6-47cb-b5e4-c72970afe038")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalExVatInPreferredCurrency { get; set; }

        #region Allors
        [Id("5c712db9-55ca-4bb7-8734-421db98ac642")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalVatInPreferredCurrency { get; set; }

        #region Allors
        [Id("1aa160a0-c5b6-48eb-88f3-a5ed92022329")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalIncVatInPreferredCurrency { get; set; }

        #region Allors
        [Id("9f1e78d8-2fd0-46a2-8ccb-5b87e4f4b459")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalSurchargeInPreferredCurrency { get; set; }

        #region Allors
        [Id("3796228a-9816-4dd2-a330-ce6e9a62b16b")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalDiscountInPreferredCurrency { get; set; }

        #region Allors
        [Id("984fb92c-be38-492d-866f-d2fa7ae2e366")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalShippingAndHandlingInPreferredCurrency { get; set; }

        #region Allors
        [Id("16a1a43e-0c42-45e8-94a6-ee8aa4d3dee4")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalFeeInPreferredCurrency { get; set; }

        #region Allors
        [Id("bb5aabb9-80c4-478e-ac66-a60626e3c5a6")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        decimal TotalExtraChargeInPreferredCurrency { get; set; }

        #region Allors
        [Id("a971dca8-e852-403d-b901-dea36a7898b5")]
        #endregion
        [Workspace]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal TotalBasePriceInPreferredCurrency { get; set; }

        #region Allors
        [Id("d972f3b8-c11a-4213-a0b9-fc91d9ddd6f1")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        decimal TotalListPriceInPreferredCurrency { get; set; }

        #region Allors
        [Id("36d27068-bc8a-49f7-ac97-a934e3de4617")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        decimal GrandTotalInPreferredCurrency { get; set; }

        #region Allors

        [Id("8C858157-B9BC-4E2C-97BC-646066532854")]

        #endregion
        [Workspace(Default)]
        void Create();

        #region Allors

        [Id("3df1ddb1-cb93-4da7-a5d1-fa22a164c2e2")]

        #endregion

        [Workspace(Default)]
        public void SetReadyForProcessing();

        #region Allors

        [Id("70F1138B-1383-4AA1-A08E-6C99F71F3F07")]

        #endregion
        [Workspace(Default)]
        void Reopen();

        #region Allors
        [Id("519F70DC-0C4C-43E7-8929-378D8871CD84")]
        #endregion
        [Workspace(Default)]
        void Approve();

        #region Allors
        [Id("506ED1BA-5F88-487E-B126-470FE1FD7791")]
        #endregion
        [Workspace(Default)]
        void Send();

        #region Allors
        [Id("90c9c005-3842-44f5-877c-f601523a888f")]
        #endregion
        [Workspace(Default)]
        void Accept();

        #region Allors
        [Id("6b5e540d-96a8-48cf-a888-7e7f6b844d28")]
        #endregion
        [Workspace(Default)]
        void Revise();

        #region Allors
        [Id("39694549-7173-4904-8AE0-DA7390F595A5")]
        #endregion
        [Workspace(Default)]
        void Reject();

        #region Allors
        [Id("712D9F73-0D39-4F25-9CD3-6D8BE6F8AEC8")]
        #endregion
        [Workspace(Default)]
        void Cancel();
    }
}

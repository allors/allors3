// <copyright file="QuoteItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("01fc58a0-89b8-4dc0-97f9-5f628b9c9577")]
    #endregion
    public partial class QuoteItem : Priceable, Versioned, Deletable
    {
        #region inherited properties

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public decimal AssignedUnitPrice { get; set; }

        public decimal UnitBasePrice { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal UnitSurcharge { get; set; }

        public decimal UnitDiscount { get; set; }

        public decimal UnitVat { get; set; }

        public VatRegime AssignedVatRegime { get; set; }

        public VatRegime DerivedVatRegime { get; set; }

        public VatRate VatRate { get; set; }

        public decimal TotalBasePrice { get; set; }

        public decimal TotalVat { get; set; }

        public decimal TotalIncVat { get; set; }

        public decimal TotalExVat { get; set; }

        public decimal TotalDiscount { get; set; }

        public decimal TotalDiscountAsPercentage { get; set; }

        public decimal TotalSurcharge { get; set; }

        public decimal TotalSurchargeAsPercentage { get; set; }

        public decimal GrandTotal { get; set; }
        public DiscountAdjustment[] DiscountAdjustments { get; set; }

        public SurchargeAdjustment[] SurchargeAdjustments { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public Permission[] TransitionalDeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }
        #endregion

        #region ObjectStates
        #region QuoteItemState
        #region Allors
        [Id("72B768E0-1F06-4409-A3A0-9F2AE622CB0E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public QuoteItemState PreviousQuoteItemState { get; set; }

        #region Allors
        [Id("9BA3DB20-4F1C-472F-958F-D1D506ECB019")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public QuoteItemState LastQuoteItemState { get; set; }

        #region Allors
        [Id("D4272795-F320-4DAA-9009-E1150197F890")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public QuoteItemState QuoteItemState { get; set; }
        #endregion
        #endregion

        #region Versioning
        #region Allors
        [Id("AD9C5BEA-6D7D-4417-8859-18D7D46DF8CC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public QuoteItemVersion CurrentVersion { get; set; }

        #region Allors
        [Id("DA5C696C-3496-49F4-B380-3D78851AC064")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public QuoteItemVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("77096A96-3445-441B-86EE-A2B60E9EAD91")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public InvoiceItemType InvoiceItemType { get; set; }

        #region Allors
        [Id("E534ABCB-FB3B-4722-B740-7EE4C2DE7EF7")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public QuoteItem[] QuotedWithFeatures { get; set; }

        #region Allors
        [Id("F8F42AF0-193A-4427-96AC-B20FAC637ADD")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        public string InternalComment { get; set; }

        #region Allors
        [Id("05c69ae6-e671-4520-87c7-5fa24a92c44d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party Authorizer { get; set; }

        #region Allors
        [Id("1214acee-1b91-4c16-b6d0-84f865b6a43a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Deliverable Deliverable { get; set; }

        #region Allors
        [Id("20a5f3d3-8b12-4717-874f-eb62ad0a1654")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Product Product { get; set; }

        #region Allors
        [Id("28f5767e-16fa-40aa-89d9-c23ee29572d1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ProductFeature ProductFeature { get; set; }

        #region Allors
        [Id("7A5BFCE5-D7FB-483C-AEE0-E05427EAAF2E")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public SerialisedItem SerialisedItem { get; set; }

        #region Allors
        [Id("262a458d-0b38-4123-b210-576633297f44")]
        #endregion
        [Workspace(Default)]
        public DateTime EstimatedDeliveryDate { get; set; }

        #region Allors
        [Id("D071BBFA-8960-4F02-8F55-702112A0F608")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Workspace(Default)]
        public DateTime RequiredByDate { get; set; }

        #region Allors
        [Id("28c0e280-16ce-48fc-8bc4-734e1ea0cd36")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Indexed]
        [Workspace(Default)]
        public UnitOfMeasure UnitOfMeasure { get; set; }

        #region Allors
        [Id("8b1280eb-0fef-450e-afc8-dbdc6fc65abb")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Skill Skill { get; set; }

        #region Allors
        [Id("8be8dc07-a358-4b8d-a84c-01bd3efea6fb")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public WorkEffort WorkEffort { get; set; }

        #region Allors
        [Id("d1f7f2cb-cbc8-42b4-a3f0-198ff35957de")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public QuoteTerm[] QuoteTerms { get; set; }

        #region Allors
        [Id("d7805656-dd9c-4144-a11f-efbb32e6ecb3")]
        #endregion
        [Required]
        [Workspace(Default)]
        public decimal Quantity { get; set; }

        #region Allors
        [Id("F68BA38D-5F7B-49F8-B019-522E2E5463EC")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal TotalPriceAdjustment { get; set; }

        #region Allors
        [Id("dc00905b-bb4f-4a47-88d6-1ae6ce0855f7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public RequestItem RequestItem { get; set; }

        #region Allors
        [Id("06C27EDA-0DF1-4318-BC57-D62F8BF32B0C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Synced]
        public Quote SyncedQuote { get; set; }

        #region Allors
        [Id("F8746889-097A-4C4E-BB55-511F0A8E3B41")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string Details { get; set; }

        #region Allors
        [Id("5ccce98c-eeec-439f-adf5-d472aa00eecd")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public IrpfRegime DerivedIrpfRegime { get; set; }

        #region Allors
        [Id("ecd72f14-a7f3-43d9-bc95-46f0339ab920")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public IrpfRegime AssignedIrpfRegime { get; set; }

        #region Allors
        [Id("da79fdb4-2488-4b7c-926f-209d62d901fc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public IrpfRate IrpfRate { get; set; }

        #region Allors
        [Id("e646b243-674a-42c5-804a-596bfcc80d33")]
        #endregion
        [Required]
        [Derived]
        [Precision(19)]
        [Scale(5)]
        [Workspace(Default)]
        public decimal UnitIrpf { get; set; }

        #region Allors
        [Id("135abd04-f017-435b-b7db-b0a3dc730674")]
        #endregion
        [Required]
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal TotalIrpf { get; set; }

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

        #endregion

        #region Allors
        [Id("57B07865-B4CA-4443-8877-0DDAC1EA106B")]
        #endregion
        [Workspace(Default)]
        public void Cancel() { }

        #region Allors
        [Id("BE5F50FE-4524-486C-8D3D-66AB32B27C7B")]
        #endregion
        [Workspace(Default)]
        public void Reject() { }

        #region Allors
        [Id("4388A6A1-43C7-4B15-AB36-C587D2997A34")]
        #endregion
        [Workspace(Default)]
        public void Order() { }

        #region Allors
        [Id("C6494A74-92B0-4C9F-9931-8D5C97647DCA")]
        #endregion
        [Workspace(Default)]
        public void Submit() { }

        #region Allors
        [Id("DC016E6B-24FE-415C-8C50-2B9E643734BE")]
        #endregion
        [Workspace(Default)]
        public void Send() { }

        #region Allors
        [Id("81f4a9c9-d170-4590-8b1e-c42900093645")]
        #endregion
        [Workspace(Default)]
        public void DeriveDetails() { }

    }
}

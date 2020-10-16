// <copyright file="SerialisedItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("5E594A00-15A4-4871-84E9-B8010A78FD21")]
    #endregion
    public partial class SerialisedItem : Deletable, FixedAsset, Versioned
    {
        #region InheritedProperties

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Name { get; set; }

        public LocalisedText[] LocalisedNames { get; set; }

        public DateTime LastServiceDate { get; set; }

        public DateTime AcquiredDate { get; set; }

        public string Description { get; set; }

        public LocalisedText[] LocalisedDescriptions { get; set; }

        public decimal ProductionCapacity { get; set; }

        public DateTime NextServiceDate { get; set; }

        public string Keywords { get; set; }

        public string SearchString { get; set; }

        public LocalisedText[] LocalisedKeywords { get; set; }

        public Media[] PublicElectronicDocuments { get; set; }

        public LocalisedMedia[] PublicLocalisedElectronicDocuments { get; set; }

        public Media[] PrivateElectronicDocuments { get; set; }

        public LocalisedMedia[] PrivateLocalisedElectronicDocuments { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        #endregion InheritedProperties

        #region Versioning
        #region Allors
        [Id("414BDA46-B49A-4AB4-A9E2-02842414D572")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public SerialisedItemVersion CurrentVersion { get; set; }

        #region Allors
        [Id("0318F8DE-D3D1-497D-870D-34E3A8F55ACC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public SerialisedItemVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("9C9A7694-4E41-46D7-B33C-14A703370A5B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SerialisedItemState SerialisedItemState { get; set; }

        #region Allors
        [Id("330381e1-f1de-4f44-9c08-0417c2df3c0d")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SerialisedItemAvailability SerialisedItemAvailability { get; set; }

        #region Allors
        [Id("B6DD4F80-EE97-446E-9779-610FF07F13B2")]
        #endregion
        [Derived]
        [Size(256)]
        [Workspace(Default)]
        public string ItemNumber { get; set; }

        #region Allors
        [Id("de9caf09-6ae7-412e-b9bc-19ece66724da")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string SerialNumber { get; set; }

        #region Allors
        [Id("91D1A28D-AE04-4445-B4AC-2053559DCFB7")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public SerialisedItemCharacteristic[] SerialisedItemCharacteristics { get; set; }

        #region Allors
        [Id("D9D4FF13-6D54-4F35-9A81-902E0BB86545")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        public Ownership Ownership { get; set; }

        #region Allors
        [Id("E511EE11-FA2E-4F84-8010-EE1453C609F3")]
        #endregion
        [Workspace(Default)]
        public int AcquisitionYear { get; set; }

        #region Allors
        [Id("CCDD8203-F635-4821-876D-A83A925C145D")]
        #endregion
        [Workspace(Default)]
        public int ManufacturingYear { get; set; }

        #region Allors
        [Id("ECE5838C-6E0B-4889-91DA-4F9277760E9D")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public decimal PurchasePrice { get; set; }

        #region Allors
        [Id("D7BA117D-6C14-4A26-BAD2-F418E472A1A1")]
        #endregion
        [Workspace(Default)]
        public decimal AssignedPurchasePrice { get; set; }

        #region Allors
        [Id("53E31ACE-5F48-4CBF-9D35-003534E1A1F1")]
        #endregion
        [Workspace(Default)]
        public decimal ExpectedSalesPrice { get; set; }

        #region Allors
        [Id("A616AE10-EA83-4878-BCBA-377396B4357A")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public Media PrimaryPhoto { get; set; }

        #region Allors
        [Id("2F5FF954-C9E2-463F-8DD6-BBC0701DD3EA")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToMany)]
        public Media[] SecondaryPhotos { get; set; }

        #region Allors
        [Id("65BBB01F-66A1-47E2-B206-2F1BE6C91398")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToMany)]
        public Media[] AdditionalPhotos { get; set; }

        #region Allors
        [Id("2A6D6DA0-A106-400E-9F2F-BA19D3F9EC77")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToMany)]
        public Media[] PrivatePhotos { get; set; }

        #region Allors
        [Id("18A320F1-2F65-4E49-A615-D88EDD15AC5C")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        [MediaType("text/markdown")]
        public string InternalComment { get; set; }

        #region Allors
        [Id("7A2A878B-1428-4C75-9A52-8725606FAA41")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public Party SuppliedBy { get; set; }

        #region Allors
        [Id("C16A8A73-84D3-4889-8B95-B8B05CB561DE")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party AssignedSuppliedBy { get; set; }

        #region Allors
        [Id("66CEB3A4-C1AD-4CAD-BBB9-F29FB12669DA")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public InternalOrganisation Buyer { get; set; }

        #region Allors
        [Id("3a7c5038-bd54-4caa-8f61-7d8a5336f24b")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public InternalOrganisation Seller { get; set; }

        #region Allors
        [Id("E9ACD0EE-693C-4459-9F40-D478F538659F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party OwnedBy { get; set; }

        #region Allors
        [Id("18F5FCB0-E48B-4DD2-8871-45540E040B80")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party RentedBy { get; set; }

        #region Allors
        [Id("5E13E62A-FD8F-49D9-9BFA-6701892FC243")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public PurchaseOrder PurchaseOrder { get; set; }

        #region Allors
        [Id("b2188137-dfd4-4f0a-a76d-a2266f87e352")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public PurchaseInvoice PurchaseInvoice { get; set; }

        #region Allors
        [Id("56FBFE00-2480-476C-86C0-140D419C33DE")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool AvailableForSale { get; set; }

        #region Allors
        [Id("D5E98D57-6DAC-46E6-A30A-E70044EC5C40")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ShowOnFrontPage { get; set; }

        #region Allors
        [Id("BB954677-BEB7-4092-96C6-44D36503174D")]
        #endregion
        [Workspace(Default)]
        public string CustomerReferenceNumber { get; set; }

        #region Allors
        [Id("15179D87-D6D8-438A-AB36-E30418DAE2AE")]
        #endregion
        [Workspace(Default)]
        public DateTime RentalFromDate { get; set; }

        #region Allors
        [Id("83220BB7-AB7D-4CE4-A3FA-1EF13720E167")]
        #endregion
        [Workspace(Default)]
        public DateTime RentalThroughDate { get; set; }

        #region Allors
        [Id("D5ABF25F-31BB-4406-AC4A-4171E42EF0D7")]
        #endregion
        [Workspace(Default)]
        public DateTime ExpectedReturnDate { get; set; }

        // TODO: Don't use WHERE in role name
        #region Allors
        [Id("E927291E-21A1-4289-B5AF-4A2CA2996DA2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Part PartWhereItem { get; set; }

        #region Allors
        [Id("2b8a24f3-ce26-4b53-94b2-d7d0cef3f6b1")]
        #endregion
        [Required]
        public Guid DerivationTrigger { get; set; }

        #region Allors
        [Id("50db0036-a15c-418d-b354-ad3b5b1c4bd6")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        public string DisplayProductCategories { get; set; }

        #region Allors
        [Id("23572f9e-9423-49ac-baf7-c0ecb039c823")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string SerialisedItemAvailabilityName { get; set; }

        #region Allors
        [Id("70442e8b-9965-4c5a-a2a7-10b11ee8620a")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string SuppliedByPartyName { get; set; }

        #region Allors
        [Id("ea0a8474-490b-4ae3-8c0d-546b9167b552")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string OwnedByPartyName { get; set; }

        #region Allors
        [Id("0e2a38d0-c550-4cd7-8fc1-e2f93c546b5d")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string RentedByPartyName { get; set; }

        #region Allors
        [Id("148487bd-4561-400a-8540-ae1e57fa2268")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string OwnershipByOwnershipName { get; set; }

        #region Allors
        [Id("80c6e34f-aadd-4ef6-b8cf-da532833ac03")]
        #endregion
        [Required]
        [Derived]
        [Workspace(Default)]
        public bool OnQuote { get; set; }

        #region Allors
        [Id("85daec66-1768-40ce-a91b-f987256ee0ed")]
        #endregion
        [Required]
        [Derived]
        [Workspace(Default)]
        public bool OnSalesOrder { get; set; }

        #region Allors
        [Id("7885e0a2-514d-4eb9-b654-f047eda00574")]
        #endregion
        [Required]
        [Derived]
        [Workspace(Default)]
        public bool OnWorkEffort { get; set; }

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
        [Id("3a008524-c74c-48e7-8aa8-a8f9743bd32f")]
        #endregion
        [Workspace(Default)]
        public void DeriveDisplayProductCategories() { }
    }
}

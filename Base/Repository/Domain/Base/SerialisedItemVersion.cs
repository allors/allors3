// <copyright file="SerialisedItemVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("485C8073-22B6-402B-B0F0-479764CFB67A")]
    #endregion
    public partial class SerialisedItemVersion : Version, Deletable
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public User LastModifiedBy { get; set; }

        #endregion

        #region Allors
        [Id("4EE6B72D-B1EC-4586-8666-1FE8006F147A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SerialisedItemState SerialisedItemState { get; set; }

        #region Allors
        [Id("60cf90a6-7049-4692-ac73-1394478b0fb6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SerialisedItemAvailability SerialisedItemAvailability { get; set; }

        #region Allors
        [Id("76B16EB6-4526-4024-B29A-F51AAB49F20E")]
        #endregion
        [Workspace(Default)]
        public string SerialNumber { get; set; }

        #region Allors
        [Id("94F10411-FDDA-4A7D-8617-AF7BFE36BE9F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public SerialisedItemCharacteristic[] SerialisedItemCharacteristics { get; set; }

        #region Allors
        [Id("7E46E5D7-FBFB-4D7A-9EC6-522FBE37826D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Ownership Ownership { get; set; }

        #region Allors
        [Id("25178972-F921-47CA-B32D-D63CCF9A4AC8")]
        #endregion
        [Workspace(Default)]
        public int AcquisitionYear { get; set; }

        #region Allors
        [Id("59266D15-C7B2-4BFD-8470-0517B634AA50")]
        #endregion
        [Workspace(Default)]
        public int ManufacturingYear { get; set; }

        #region Allors
        [Id("03D549E9-0DCD-4674-A789-8D9CB6CF0377")]
        #endregion
        [Workspace(Default)]
        public decimal PurchasePrice { get; set; }

        #region Allors
        [Id("D7B6361C-2387-4838-BBB1-B6F001D9E2B4")]
        #endregion
        [Workspace(Default)]
        public decimal ExpectedSalesPrice { get; set; }

        #region Allors
        [Id("8AA2ED2E-BB4D-489A-81BD-9B5075AFC7CA")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        public string InternalComment { get; set; }

        #region Allors
        [Id("53857cc0-5fcb-43ee-960d-a9d0c2189b18")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool AvailableForSale { get; set; }

        #region Allors
        [Id("c8217953-ad82-4db3-b70f-231bae89c298")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public InternalOrganisation Seller { get; set; }

        #region Allors
        [Id("34F61A40-3794-4195-A269-749C68CBC8A4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party SuppliedBy { get; set; }

        #region Allors
        [Id("92A371AC-A079-403F-9219-829F217B3EB6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party OwnedBy { get; set; }

        #region Allors
        [Id("46F8C336-584F-4B18-AA4C-71A576EE2136")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party RentedBy { get; set; }

        #region Allors
        [Id("b128557f-63b3-4626-b6a4-e53dce6ddf67")]
        #endregion
        [Workspace(Default)]
        public bool OnQuote { get; set; }

        #region Allors
        [Id("c470b360-d7aa-4ce6-bc69-43d8829e5405")]
        #endregion
        [Workspace(Default)]
        public bool OnSalesOrder { get; set; }

        #region Allors
        [Id("98ff5b83-6cae-4c5e-9137-f8ef9545b189")]
        #endregion
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
    }
}

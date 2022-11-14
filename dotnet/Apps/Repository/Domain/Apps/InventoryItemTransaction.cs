// <copyright file="InventoryItemTransaction.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("b00e2650-283f-4326-bdd3-46a2890e2037")]
    #endregion
    public partial class InventoryItemTransaction : Commentable
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        #endregion

        /// <summary>
        /// Gets or Sets the Part to which this InventoryItemTransaction applies.
        /// </summary>
        #region Allors
        [Id("F851D977-7D58-4105-AB4A-74CFD5298D2D")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Part Part { get; set; }

        #region Allors
        [Id("0b285a95-a479-4052-a75c-2b4a3e074188")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string PartNumber { get; set; }

        #region Allors
        [Id("aedd91bd-b401-4261-aa27-32bb82197257")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string PartDisplayName { get; set; }

        #region Allors
        [Id("e422efc4-4d17-46d8-bba4-6e78e7761f93")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public InventoryTransactionReason Reason { get; set; }

        /// <summary>
        /// Gets or sets the Serial Number for this InventoryItemTransaction (required if Part.InventoryItemKind.IsSerialised).
        /// </summary>
        #region Allors
        [Id("AFC2C5F2-4E00-4FB8-836F-C2B6A5A292A0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SerialisedItem SerialisedItem { get; set; }

        #region Allors
        [Id("c5b12893-741e-4bcf-b027-1fb000ee370c")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string SerialisedItemItemNumber { get; set; }

        #region Allors
        [Id("57bdf1d7-84b8-4c7c-a470-396f6facd3bd")]
        #endregion
        [Required]
        [Workspace(Default)]
        public decimal Quantity { get; set; }

        #region Allors
        [Id("364853b7-13eb-4952-ac71-36541518b48a")]
        #endregion
        [Required]
        [Workspace(Default)]
        public decimal CostInApplicationCurrency { get; set; }

        /// <summary>
        /// The TransactionDate and Time when this InventoryItemTransaction occurred.
        /// </summary>
        #region Allors
        [Id("af9fa5bc-a392-473d-b077-7f06ee24390b")]
        #endregion
        [Required]
        [Indexed]
        [Workspace(Default)]
        public DateTime TransactionDate { get; set; }

        #region Allors
        [Id("9ADBF0A8-5676-430A-8242-97660692A1F6")]
        #endregion
        [Indexed]
        [Derived]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public InventoryItem InventoryItem { get; set; }

        /// <summary>
        /// Gets or Sets the ShipmentItem where this InventoryItemTransaction applies.
        /// Used in updating transaction cost when invoice is approved.
        /// </summary>
        #region Allors
        [Id("474d6ddc-6f07-4e74-948a-ec3ffa4640cb")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public ShipmentItem ShipmentItem { get; set; }

        /// <summary>
        /// Gets or Sets the Facility where this InventoryItemTransaction applies.
        /// If not provided, the DefaultFacility from the Part will be used for this InventoryItemTransaction.
        /// </summary>
        #region Allors
        [Id("D22BB11E-8E99-4B81-9B72-20858AF33A11")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Facility Facility { get; set; }

        #region Allors
        [Id("627d4c37-3f17-49c3-8b8d-4d7019daebb0")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string FacilityName { get; set; }

        /// <summary>
        /// Gets or Sets the Lot where this InventoryItemTransaction applies (if any).
        /// </summary>
        #region Allors
        [Id("7EC5EF43-3031-4519-9C0C-14828E123C7D")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Lot Lot { get; set; }

        /// <summary>
        /// Gets or Sets the UnitOfMeasure for this InventoryItemTransaction.
        /// If not provided, the UnitOfMeasure from the Part will be used for this InventoryItemTransaction.
        /// </summary>
        #region Allors
        [Id("639C6EF1-1D76-42B4-A59B-184DAD622D6F")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public UnitOfMeasure UnitOfMeasure { get; set; }

        /// <summary>
        /// Gets or Sets the NonSerialisedInventoryItemState for this InventoryItemTransaction.
        /// If not provided, the DefaultNonSerialisedInventoryItemState from the InventoryStrategy will be used for this InventoryItemTransaction.
        /// </summary>
        #region Allors
        [Id("CA486EA2-D3CA-47CD-B09B-87CDA73DFC42")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public NonSerialisedInventoryItemState NonSerialisedInventoryItemState { get; set; }

        /// <summary>
        /// Gets or Sets the SerialisedInventoryItemState for this InventoryItemTransaction.
        /// /// If not provided, the DefaultSerialisedInventoryItemState from the InventoryStrategy will be used for this InventoryItemTransaction.
        /// </summary>
        #region Allors
        [Id("07166685-B405-41F7-9581-10CFB937ACF9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public SerialisedInventoryItemState SerialisedInventoryItemState { get; set; }

        #region Allors
        [Id("69d9d589-a462-44a0-994b-3eed90f4924b")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string InventoryItemStateName { get; set; }

        #region Allors
        [Id("bd32bfb7-12cb-409e-bdef-7e748b2b7f86")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string WorkEffortNumber { get; set; }

        #region Allors
        [Id("0a5bd58d-dfab-4bed-918d-f224dfb6a9c3")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string SalesOrderNumber { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}

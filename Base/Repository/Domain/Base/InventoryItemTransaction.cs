// <copyright file="InventoryItemTransaction.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("b00e2650-283f-4326-bdd3-46a2890e2037")]
    #endregion
    public partial class InventoryItemTransaction : Commentable, Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

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
        [Workspace]
        public Part Part { get; set; }

        #region Allors
        [Id("e422efc4-4d17-46d8-bba4-6e78e7761f93")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace]
        public InventoryTransactionReason Reason { get; set; }

        /// <summary>
        /// Gets or sets the Serial Number for this InventoryItemTransaction (required if Part.InventoryItemKind.IsSerialised).
        /// </summary>
        #region Allors
        [Id("AFC2C5F2-4E00-4FB8-836F-C2B6A5A292A0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public SerialisedItem SerialisedItem { get; set; }

        #region Allors
        [Id("57bdf1d7-84b8-4c7c-a470-396f6facd3bd")]
        #endregion
        [Required]
        [Workspace]
        public decimal Quantity { get; set; }

        #region Allors
        [Id("364853b7-13eb-4952-ac71-36541518b48a")]
        #endregion
        [Required]
        [Workspace]
        public decimal Cost { get; set; }

        /// <summary>
        /// The TransactionDate and Time when this InventoryItemTransaction occurred.
        /// </summary>
        #region Allors
        [Id("af9fa5bc-a392-473d-b077-7f06ee24390b")]
        #endregion
        [Required]
        [Indexed]
        [Workspace]
        public DateTime TransactionDate { get; set; }

        #region Allors
        [Id("9ADBF0A8-5676-430A-8242-97660692A1F6")]
        #endregion
        [Indexed]
        [Derived]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
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
        [Workspace]
        public ShipmentItem ShipmentItem { get; set; }

        #region Allors
        [Id("58ead8d2-c9c3-4092-b5d1-79af4811f43c")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        public ItemVarianceAccountingTransaction ItemVarianceAccountingTransaction { get; set; }

        /// <summary>
        /// Gets or Sets the Facility where this InventoryItemTransaction applies.
        /// If not provided, the DefaultFacility from the Part will be used for this InventoryItemTransaction.
        /// </summary>
        #region Allors
        [Id("D22BB11E-8E99-4B81-9B72-20858AF33A11")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Facility Facility { get; set; }

        /// <summary>
        /// Gets or Sets the Lot where this InventoryItemTransaction applies (if any).
        /// </summary>
        #region Allors
        [Id("7EC5EF43-3031-4519-9C0C-14828E123C7D")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
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
        [Workspace]
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
        [Workspace]
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
        [Workspace]
        public SerialisedInventoryItemState SerialisedInventoryItemState { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion

    }
}

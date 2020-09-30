// <copyright file="SerialisedInventoryItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("4a70cbb3-6e23-4118-a07d-d611de9297de")]
    #endregion
    public partial class SerialisedInventoryItem : InventoryItem
    {
        #region inherited properties

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public Part Part { get; set; }

        public string Name { get; set; }

        public string PartDisplayName { get; set; }

        public Lot Lot { get; set; }

        public UnitOfMeasure UnitOfMeasure { get; set; }

        public Facility Facility { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string SearchString { get; set; }

        #endregion

        #region SerialisedInventoryItemState
        #region Allors
        [Id("CCB71B4F-1A3F-4D08-B3E4-380FB2D513FF")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SerialisedInventoryItemState PreviousSerialisedInventoryItemState { get; set; }

        #region Allors
        [Id("72A268C1-4A32-48C1-BB2D-837AC1DF361E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public SerialisedInventoryItemState LastSerialisedInventoryItemState { get; set; }

        #region Allors
        [Id("7E757767-61AC-49E9-97CF-DE929C015D5B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SerialisedInventoryItemState SerialisedInventoryItemState { get; set; }
        #endregion

        #region Versioning
        #region Allors
        [Id("235F117A-3288-4729-8348-92BCEBCDB3B6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public SerialisedInventoryItemVersion CurrentVersion { get; set; }

        #region Allors
        [Id("14266ECC-B4FF-4365-9087-0F67946246D2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public SerialisedInventoryItemVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("B0753263-46F0-409E-8445-2B7E261DD7F6")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public SerialisedItem SerialisedItem { get; set; }

        #region Allors
        [Id("C3C73F0D-2B82-460E-9C58-272AB7CFE8E4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SerialisedInventoryItemState InventoryItemState { get; set; }

        #region Allors
        [Id("2BE471C7-7C0C-436C-B55A-14930D2A8A5C")]
        #endregion
        [Derived]
        [Required]
        [Workspace(Default)]
        public int Quantity { get; set; }

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

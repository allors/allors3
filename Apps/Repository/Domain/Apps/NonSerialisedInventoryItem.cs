// <copyright file="NonSerialisedInventoryItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("5b294591-e20a-4bad-940a-27ae7b2f8770")]
    #endregion
    public partial class NonSerialisedInventoryItem : InventoryItem
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

        #region ObjectStates
        #region NonSerialisedInventoryItemState
        #region Allors
        [Id("35D3FF5B-AA47-41F9-A44F-7809EC2D7955")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public NonSerialisedInventoryItemState PreviousNonSerialisedInventoryItemState { get; set; }

        #region Allors
        [Id("4524D9FF-A484-49BD-B8BC-74C4D488FDC3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public NonSerialisedInventoryItemState LastNonSerialisedInventoryItemState { get; set; }

        #region Allors
        [Id("B31DEEC8-709E-4049-989A-D4BD3028A166")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public NonSerialisedInventoryItemState NonSerialisedInventoryItemState { get; set; }
        #endregion
        #endregion

        #region Versioning
        #region Allors
        [Id("4E2486A2-3CF9-4EB6-B675-6565A64116A6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public NonSerialisedInventoryItemVersion CurrentVersion { get; set; }

        #region Allors
        [Id("53B35269-EF6C-45EE-BE20-FCDC732CE06E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public NonSerialisedInventoryItemVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("4a0dc5cb-8d4a-479a-8413-4df6d9e884fe")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal QuantityOnHand { get; set; }

        #region Allors
        [Id("c02a2b4a-5ae4-4050-a975-4e675fa56589")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal AvailableToPromise { get; set; }

        #region Allors
        [Id("2d25feaf-3468-41d5-8107-ce46b78a82b4")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal QuantityCommittedOut { get; set; }

        #region Allors
        [Id("7d402c70-da15-4f28-917b-e75e3fd22560")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal QuantityExpectedIn { get; set; }

        #region Allors
        [Id("ba5e2476-abdd-4d61-8a14-5d99a36c4544")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal PreviousQuantityOnHand { get; set; }

        #region Allors
        [Id("F1986E5F-3A8F-478A-B4C4-C6913C78AFE2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public NonSerialisedInventoryItemState InventoryItemState { get; set; }

        #region Allors
        [Id("9865c082-839f-406b-8973-8bc57ca6da5f")]
        [Indexed]
        [Size(256)]
        #endregion
        [Workspace(Default)]
        public string PartLocation { get; set; }

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

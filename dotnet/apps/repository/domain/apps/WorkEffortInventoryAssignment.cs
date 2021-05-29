// <copyright file="WorkEffortInventoryAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("f67e7755-5848-4601-ba70-4d1a39abfe4b")]
    #endregion
    public partial class WorkEffortInventoryAssignment : Versioned, Deletable, DelegatedAccessControlledObject
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the WorkEffort under which this Assignment exists.
        /// </summary>
        #region Allors
        [Id("0bf425d4-7468-4e28-8fda-0b04278cb2cd")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public WorkEffort Assignment { get; set; }

        /// <summary>
        /// Gets or sets the Part which describes this WorkEffortInventoryAssignment.
        /// </summary>
        #region Allors
        [Id("3704B202-A216-4943-A98A-EB0A78477EFD")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public InventoryItem InventoryItem { get; set; }

        /// <summary>
        /// Gets or sets the Quantity of the Part for this WorkEffortInventoryAssignment.
        /// </summary>
        #region Allors
        [Id("70121570-c02d-4977-80e4-23e14cbc3fc9")]
        #endregion
        [Required]
        [Workspace(Default)]
        public decimal Quantity { get; set; }

        #region Allors
        [Id("495645bf-0ef7-49be-9d4d-59125221ca06")]
        #endregion
        [Required]
        [Workspace(Default)]
        public decimal CostOfGoodsSold { get; set; }

        #region Allors
        [Id("E13BAD88-7B44-4B92-89D0-86D182404880")]
        #endregion
        [Workspace(Default)]
        public decimal AssignedBillableQuantity { get; set; }

        #region Allors
        [Id("be2b92ca-53d0-46c7-954d-6432590e1994")]
        #endregion
        [Required]
        [Derived]
        [Workspace(Default)]
        public decimal DerivedBillableQuantity { get; set; }

        /// <summary>
        /// Gets or sets the InventoryItemTransactions create by this WorkEffortInventoryAssignment (derived).
        /// </summary>
        #region Allors
        [Id("5fcdb553-4b8f-419b-9f12-b9cefa68d39f")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public InventoryItemTransaction[] InventoryItemTransactions { get; set; }

        #region Allors
        [Id("ED3CE72C-C980-43DC-95FC-E8111E87F018")]
        #endregion
        [Precision(19)]
        [Scale(4)]
        [Workspace(Default)]
        public decimal AssignedUnitSellingPrice { get; set; }

        #region Allors
        [Id("48C27B12-EAE9-48F6-B803-DC6C568D3816")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal UnitSellingPrice { get; set; }

        #region Versioning
        #region Allors
        [Id("07AAB5A6-19C0-4812-B957-B051C3998BCD")]
        #endregion
        [Indexed]
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public WorkEffortInventoryAssignmentVersion CurrentVersion { get; set; }

        #region Allors
        [Id("A5E9EAB4-D939-4F03-A771-712EAC2674A5")]
        #endregion
        [Indexed]
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public WorkEffortInventoryAssignmentVersion[] AllVersions { get; set; }
        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        public void DelegateAccess() { }

        #endregion
    }
}

// <copyright file="SalesOrderItemInventoryAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("7975E28B-4707-44E2-8B56-0EAA8D7F9EFD")]
    #endregion
    public partial class SalesOrderItemInventoryAssignment : Versioned
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        #endregion

        #region Allors
        [Id("BDE5FF54-505B-4241-88A8-334999E43C0B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public InventoryItem InventoryItem { get; set; }

        #region Allors
        [Id("B5997790-CAC7-464A-91AD-AC01E6F1D57F")]
        #endregion
        [Required]
        [Workspace(Default)]
        public decimal Quantity { get; set; }

        #region Allors
        [Id("063A2F35-2CC0-4ACA-935D-31BBEDC0A2C6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Derived]
        [Workspace(Default)]
        public InventoryItemTransaction[] InventoryItemTransactions { get; set; }

        #region Versioning
        #region Allors
        [Id("E3E0F9C1-7B16-4C9C-B1A0-57F7A7CA50AF")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public SalesOrderItemInventoryAssignmentVersion CurrentVersion { get; set; }

        #region Allors
        [Id("FE0F0C2F-1198-46E8-9423-09A736BDF24A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public SalesOrderItemInventoryAssignmentVersion[] AllVersions { get; set; }
        #endregion

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

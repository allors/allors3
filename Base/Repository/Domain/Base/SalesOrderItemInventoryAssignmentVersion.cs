// <copyright file="SalesOrderItemInventoryAssignmentVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("7CF1B3F8-46B1-4DDE-92F9-455D6E30D37F")]
    #endregion
    public partial class SalesOrderItemInventoryAssignmentVersion : Version
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public User LastModifiedBy { get; set; }

        #endregion

        #region Allors
        [Id("5CE3E9F4-BBD1-4134-A187-6570D1D7E52A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public InventoryItem InventoryItem { get; set; }

        #region Allors
        [Id("47239001-6F4D-4EDA-9DE4-3805629017F1")]
        #endregion
        [Required]
        [Workspace(Default)]
        public decimal Quantity { get; set; }

        #region Allors
        [Id("B05C8F82-ABA9-464A-852B-885F6C89AEEB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Derived]
        [Workspace(Default)]
        public InventoryItemTransaction[] InventoryItemTransactions { get; set; }

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

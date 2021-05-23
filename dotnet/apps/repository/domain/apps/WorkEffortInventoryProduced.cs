// <copyright file="WorkEffortInventoryProduced.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("0E74304B-1B18-41E7-A20B-3DC1E46A8504")]
    #endregion
    public partial class WorkEffortInventoryProduced : Versioned, Object
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
        [Id("B978B71D-4FC4-4820-B535-2CE8E7A1E60D")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public WorkEffort Assignment { get; set; }

        /// <summary>
        /// Gets or sets the Part which describes this WorkEffortInventoryProduced.
        /// </summary>
        #region Allors
        [Id("7A5B793B-866E-40F2-BBD1-A8FB65AF9EDA")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Part Part { get; set; }

        /// <summary>
        /// Gets or sets the Quantity of the Part for this WorkEffortInventoryProduced.
        /// </summary>
        #region Allors
        [Id("1B1F154E-5C33-4E9C-9318-4BACC7827A9E")]
        #endregion
        [Required]
        [Workspace(Default)]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the InventoryItemTransactions create by this WorkEffortInventoryProduced (derived).
        /// </summary>
        #region Allors
        [Id("FE7B73C6-CF83-4A89-8087-E469A1C8819A")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public InventoryItemTransaction[] InventoryItemTransactions { get; set; }

        #region Versioning
        #region Allors
        [Id("9D6E6FCF-AE5D-42EB-942E-E1B5AC9AE8C8")]
        #endregion
        [Indexed]
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public WorkEffortInventoryProducedVersion CurrentVersion { get; set; }

        #region Allors
        [Id("8CAE04DC-EC29-4366-A322-64602710A287")]
        #endregion
        [Indexed]
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public WorkEffortInventoryProducedVersion[] AllVersions { get; set; }
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

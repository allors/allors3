// <copyright file="WorkEffortInventoryAssignmentVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;

    #region Allors
    [Id("D4BF9E21-4EB5-4ED0-A9BF-3FE01E585AC7")]
    #endregion
    public partial class WorkEffortInventoryAssignmentVersion : Version
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public User LastModifiedBy { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the WorkEffort under which this Assignment exists.
        /// </summary>
        #region Allors
        [Id("1B695715-456E-4BE6-8B42-0386928BBE07")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public WorkEffort Assignment { get; set; }

        /// <summary>
        /// Gets or sets the Part which describes this WorkEffortInventoryAssignment.
        /// </summary>
        #region Allors
        [Id("F1811C2A-9A4E-4949-9008-E3519EA4AB51")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public InventoryItem InventoryItem { get; set; }

        /// <summary>
        /// Gets or sets the Quantity of the Part for this WorkEffortInventoryAssignment.
        /// </summary>
        #region Allors
        [Id("F5E0881D-E239-4B21-8E7A-C380E96E2A26")]
        #endregion
        [Required]
        [Workspace]
        public decimal Quantity { get; set; }

        #region Allors
        [Id("A1A024D4-B20F-44E1-84F0-1EBFEB962DE2")]
        #endregion
        [Workspace]
        public decimal AssignedBillableQuantity { get; set; }

        #region Allors
        [Id("5e6511de-2845-4306-8c68-57e1cabbe092")]
        #endregion
        [Workspace]
        public decimal DerivedBillableQuantity { get; set; }

        /// <summary>
        /// Gets or sets the InventoryItemTransactions create by this WorkEffortInventoryAssignment (derived).
        /// </summary>
        #region Allors
        [Id("30EF280B-A7EA-400D-BA36-2DCD242C96F2")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
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

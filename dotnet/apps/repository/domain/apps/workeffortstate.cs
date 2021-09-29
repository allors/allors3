// <copyright file="WorkEffortState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("f7d24734-88d3-47e7-b718-8815914c9ad4")]
    #endregion
    public partial class WorkEffortState : ObjectState
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Revocation ObjectRevocation { get; set; }

        public string Name { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        /// <summary>
        /// Gets or Sets the InventoryTransactionReasons to Create (if they do not exist) for this WorkEffortState.
        /// </summary>
        #region Allors
        [Id("3DBEB0A7-DB3B-4B37-9BCC-5D46842AAF44")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public InventoryTransactionReason[] InventoryTransactionReasonsToCreate { get; set; }

        /// <summary>
        /// Gets or Sets the InventoryTransactionReasons to Cancel (if they exist) for this WorkEffortState.
        /// </summary>
        #region Allors
        [Id("4F283FBF-1A14-42DC-B277-A567AFE40111")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public InventoryTransactionReason[] InventoryTransactionReasonsToCancel { get; set; }

        #region Workspace
        #region Allors
        [Id("5dd8e375-5c37-4b6e-b43a-8af8945ef4f8")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public string IsCreated { get; set; }

        #region Allors
        [Id("2d8397e0-6165-4765-aaaa-e2b8a2575bf4")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public string IsInProgress { get; set; }

        #region Allors
        [Id("707b5fe1-3ed7-42a4-9578-efce378a32ab")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public string IsCancelled { get; set; }

        #region Allors
        [Id("e2938d67-dfcf-45b8-a1ff-fb7078600fce")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public string IsCompleted { get; set; }

        #region Allors
        [Id("a4a966a0-1929-429d-a462-e909d599fb26")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public string IsFinished { get; set; }
        #endregion

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

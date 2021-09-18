// <copyright file="SalesOrderItemState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("21f09e4c-7b3f-4152-8822-8c485011759c")]
    #endregion
    public partial class SalesOrderItemState : ObjectState
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Revocation ObjectRevocation { get; set; }

        public string Name { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        /// <summary>
        /// Gets or Sets the InventoryTransactionReasons to Create (if they do not exist) for this SalesOrderItem.
        /// </summary>
        #region Allors
        [Id("ADB361F0-E328-4FB1-9735-D4D9CDC40BFA")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public InventoryTransactionReason[] InventoryTransactionReasonsToCreate { get; set; }

        /// <summary>
        /// Gets or Sets the InventoryTransactionReasons to Cancel (if they exist) for this SalesOrderItem.
        /// </summary>
        #region Allors
        [Id("9E60A60B-C036-4E37-B71F-B02FD1B029E4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public InventoryTransactionReason[] InventoryTransactionReasonsToCancel { get; set; }

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

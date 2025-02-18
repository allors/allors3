// <copyright file="NonSerialisedInventoryItemState.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("41D19E80-8ABB-4515-AA44-3E0AF1146AE7")]
    #endregion
    public partial class NonSerialisedInventoryItemState : ObjectState
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Revocation ObjectRevocation { get; set; }

        public string Name { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion

        #region Allors
        [Id("1AB4364F-2DAA-4EC0-BF2E-77CAF2E354CD")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool AvailableForSale { get; set; }
    }
}

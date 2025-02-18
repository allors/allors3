// <copyright file="CostCenter.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("2ab70094-5481-4ecc-ae15-cb2131fbc2f1")]
    #endregion
    public partial class CostCenter : UniquelyIdentifiable, Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("a3168a59-38ea-4359-b258-c9cbd656ce35")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("2a2125fd-c715-4a0f-8c1a-c1207f02a494")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string Description { get; set; }

        #region Allors
        [Id("d7e01e38-d271-4c9c-847e-d26d9d4957af")]
        #endregion
        [Workspace(Default)]
        public bool Active { get; set; }

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

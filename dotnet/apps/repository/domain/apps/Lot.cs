// <copyright file="Lot.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("d900e278-7add-4e90-8bea-0a65d03f4fa7")]
    #endregion
    public partial class Lot : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("4888a06a-fcf5-42a7-a1c3-721d3abaa755")]
        #endregion
        [Workspace(Default)]
        public DateTime ExpirationDate { get; set; }

        #region Allors
        [Id("8680f7e2-c5f1-43af-a127-68ac8404fbf4")]
        #endregion
        [Workspace(Default)]
        public int Quantity { get; set; }

        #region Allors
        [Id("ca7a3e0f-e036-40ed-9346-0d1dae45c560")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string LotNumber { get; set; }

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

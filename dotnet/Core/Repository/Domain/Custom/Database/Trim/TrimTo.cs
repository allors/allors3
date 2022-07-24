// <copyright file="C2.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("C04D372D-6D58-4EA7-AFF8-33208D4A9519")]
    #endregion
    [Workspace(Default)]
    public partial class TrimTo : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("09E9D2E5-E406-4D3B-9E3D-5AA3D69408CF")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        #endregion
    }
}

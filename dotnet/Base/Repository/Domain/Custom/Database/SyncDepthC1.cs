// <copyright file="SyncDepthC1.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("4EA6AD12-C1FB-4661-B4F7-72B81435DD70")]
    #endregion
    public partial class SyncDepthC1 : SyncDepthI1
    {
        #region inherited properties
        public int DerivationCount { get; set; }

        public SyncDepth2 SyncDepth2 { get; set; }

        public int Value { get; set; }

        #endregion

        #region inherited methods

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}

// <copyright file="SyncDepth2.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("B9996F8F-12FB-4E42-8B7F-907433A622B2")]
    #endregion
    public partial class SyncDepth2 : Object, DerivationCounted
    {
        #region inherited properties
        public int DerivationCount { get; set; }
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

        #region Allors
        [Id("C6254113-00CB-475E-AE15-B45FC3E623BC")]
        #endregion
        [Required]
        public int Value { get; set; }
    }
}

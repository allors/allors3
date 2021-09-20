// <copyright file="Subdependee.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("46a437d1-455b-4ddd-b83c-068938c352bd")]
    #endregion
    public partial class Subdependee : Object
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("194930f9-9c3f-458d-93ec-3d7bea4cd538")]
        #endregion
        public int Subcounter { get; set; }

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

// <copyright file="C1.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("4AD76347-A82F-4AA0-9478-2B6BEF5985A2")]
    #endregion
    public partial class CC : Object
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("F12C9ECE-1B0F-4449-910B-6AC935805432")]
        #endregion
        [Size(256)]
        public string Assigned { get; set; }


        #region Allors
        [Id("53F7BD1C-BAD4-4FE6-888F-BF33DA2CBD95")]
        #endregion
        [Size(256)]
        [Derived]
        public string Derived { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        #endregion
    }
}

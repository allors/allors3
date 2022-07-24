// <copyright file="Post.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{

    using Attributes;

    #region Allors
    [Id("4B66BA88-A452-4AE4-91E7-7FBC28E65B31")]
    #endregion
    public partial class Post : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("D8714378-149D-4E4B-8A18-0D8622BCD32D")]
        #endregion
        [Required]
        public int Counter { get; set; }

        #region inherited methods
        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }
        #endregion
    }
}

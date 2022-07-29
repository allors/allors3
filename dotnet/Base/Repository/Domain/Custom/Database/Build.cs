// <copyright file="Build.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("FFFCC7BD-252E-4EE7-B825-99CCBE2D5F49")]
    #endregion
    public partial class Build : Object
    {
        #region Allors
        [Id("A3DED776-B516-4C38-9B5F-5DEBFAFD15CB")]
        #endregion
        public Guid Guid { get; set; }

        #region Allors
        [Id("19112701-B610-49FC-82B8-FB786EEBCDB4")]
        #endregion
        public string String { get; set; }

        #region Methods

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild()
        {
        }

        public void OnPostBuild()
        {
        }

        public void OnInit()
        {
        }

        public void OnPostDerive()
        {
        }

        #endregion

    }
}

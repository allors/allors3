// <copyright file="EmailTemplate.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("c78a49b1-9918-4f15-95f3-c537c82f59fd")]
    #endregion
    public partial class EmailTemplate : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("21bbeaa8-f4cf-4b09-9fcd-af72a70e6f15")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string Description { get; set; }

        #region Allors
        [Id("8bb431b6-a6ea-48d0-ad78-975ec26b470f")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string BodyTemplate { get; set; }

        #region Allors
        [Id("f05fc608-5dcd-4d7d-b472-5b84c2a195a4")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string SubjectTemplate { get; set; }

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

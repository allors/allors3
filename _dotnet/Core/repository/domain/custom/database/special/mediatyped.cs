// <copyright file="AccessClass.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("355AEFD2-F5B2-499A-81D2-DD9C9F62832C")]
    #endregion
    public partial class MediaTyped : Object
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("D23961DF-6688-44D1-87D4-0E5D0C2ED533")]
        #endregion
        [Size(-1)]
        [MediaType("text/markdown")]
        [Workspace(Default)]
        public string Markdown { get; set; }

        #region inherited methods

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

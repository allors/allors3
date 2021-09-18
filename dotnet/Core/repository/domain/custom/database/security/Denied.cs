// <copyright file="AccessClass.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("437A152B-09EF-4DAD-BD35-E629F46A9249")]
    #endregion
    [Workspace(Default)]
    public partial class Denied : Object
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("449C1F0C-A63C-47B0-ABAC-3EE3511C6B23")]
        #endregion
        public string DatabaseProperty { get; set; }

        #region Allors
        [Id("58A94BD3-8784-4A51-8CC0-219889B4561E")]
        #endregion
        [Workspace(Default)]
        public string DefaultWorkspaceProperty { get; set; }

        #region Allors
        [Id("E48302E5-184C-40A8-AEDB-C7B38A515906")]
        #endregion
        [Workspace(X)]
        public string WorkspaceXProperty { get; set; }

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

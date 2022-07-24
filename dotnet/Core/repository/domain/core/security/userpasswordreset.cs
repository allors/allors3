// <copyright file="UserPasswordReset.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("2e5cd966-d85d-4ad8-ba2a-48fd0c2894dd")]
    #endregion
    public partial interface UserPasswordReset
    {
        #region Allors
        [Id("1a03a20b-9cfe-4052-807b-2780ef81cffb")]
        [Size(256)]
        #endregion
        [Workspace(Default)]
        string InExistingUserPassword { get; set; }

        #region Allors
        [Id("DCE0EA9D-105B-4E46-A22E-9B02C28DA8DB")]
        [Size(256)]
        #endregion
        [Workspace(Default)]
        string InUserPassword { get; set; }
    }
}

// <copyright file="User.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    // A name of the test population, not a login. Core's own User declares no login name, an
    // authentication plug-in does; the test domain adds one so that its tests can name their users.
    public partial interface User
    {
        #region Allors
        [Id("930D8C3C-C855-49E4-A827-4D84E8DBB8CF")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string UserName { get; set; }
    }
}

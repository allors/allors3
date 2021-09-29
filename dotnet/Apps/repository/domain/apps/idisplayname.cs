// <copyright file="IDisplayName.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("3cd51168-748e-4030-8c78-7fbd4c5dc3f4")]
    #endregion
    public partial interface IDisplayName
    {
        #region Allors
        [Id("2e7bf53b-567b-4f36-8af6-07830b9d0868")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        string DisplayName { get; set; }
    }
}

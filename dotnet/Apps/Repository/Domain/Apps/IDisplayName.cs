// <copyright file="IDisplayName.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("b9e82cf8-06e1-4748-87ab-c43a98fc5d34")]
    #endregion
    public partial interface IDisplayName
    {
        #region Allors
        [Id("0077f435-6dd4-4050-a030-e7e094c27ca1")]
        #endregion
        [Workspace(Default)]
        [Derived]
        string DisplayName { get; set; }
    }
}

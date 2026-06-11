// <copyright file="User.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("a0309c3b-6f80-4777-983e-6e69800df5be")]
    #endregion
    public partial interface User : UniquelyIdentifiable, SecurityTokenOwner, Deletable
    {
        #region Allors
        [Id("5e8ab257-1a1c-4448-aacc-71dbaaba525b")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string UserName { get; set; }
    }
}

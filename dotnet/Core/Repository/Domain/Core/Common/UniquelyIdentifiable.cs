// <copyright file="UniquelyIdentifiable.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("122ccfe1-f902-44c1-9d6c-6f6a0afa9469")]
    #endregion
    public partial interface UniquelyIdentifiable : Object
    {
        #region Allors
        [Id("e1842d87-8157-40e7-b06e-4375f311f2c3")]
        #endregion
        [Workspace(Default)]
        [Indexed]
        [Required]
        Guid UniqueId { get; set; }
    }
}

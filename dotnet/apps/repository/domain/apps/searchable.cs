// <copyright file="Searchable.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("B34B917C-EB2D-49FE-B45E-C7C6F7FE5A6A")]
    #endregion
    public partial interface Searchable
    {
        #region Allors
        [Id("CD884022-9A53-4E59-A466-FB76946CF3C6")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Size(-1)]
        [Workspace(Default)]
        string SearchString { get; set; }
    }
}

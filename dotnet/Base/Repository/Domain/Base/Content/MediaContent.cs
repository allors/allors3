// <copyright file="MediaContent.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaContent type.</summary>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("a1bbd702-9af2-4827-8e4a-28cba2abf3e8")]
    #endregion
    public partial interface MediaContent : Deletable, Object
    {
        #region Allors
        [Id("890598a9-0be4-49ee-8dd8-3581ee9355e6")]
        #endregion
        [Required]
        [Indexed]
        [Size(1024)]
        [Workspace(Default)]
        string Type { get; set; }
    }
}

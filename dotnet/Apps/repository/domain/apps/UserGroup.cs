// <copyright file="UserGroup.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;

    public partial class UserGroup
    {
        #region Allors
        [Id("472e2bd2-40d4-4924-a9b1-57566b792f6a")]
        #endregion
        [Required]
        [Workspace]
        public bool isSelectable{ get; set; }
    }
}

// <copyright file="SecurityTokenOwner.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("a69cad9c-c2f1-463f-9af1-873ce65aeea6")]
    #endregion
    public partial interface SecurityTokenOwner : Object
    {
        #region Allors
        [Id("5fb15e8b-011c-46f7-83dd-485d4cc4f9f2")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Required]
        [Derived]
        SecurityToken OwnerSecurityToken { get; set; }

        #region Allors
        [Id("056914ed-a658-4ae5-b859-97300e1b8911")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        Grant OwnerGrant { get; set; }
    }
}

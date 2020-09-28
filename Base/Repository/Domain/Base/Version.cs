// <copyright file="Version.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    public partial interface Version
    {
        #region Allors
        [Id("561C7A91-5232-453F-BA26-9B84D871ECC9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        User LastModifiedBy { get; set; }
    }
}

// <copyright file="Version.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    public partial interface Version
    {
        #region Allors
        [Id("561C7A91-5232-453F-BA26-9B84D871ECC9")]

        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        User LastModifiedBy { get; set; }
    }
}

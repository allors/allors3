// <copyright file="User.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    public partial interface User : Localised
    {
        #region Allors
        [Id("372F197C-3B6E-4C12-9BD9-D50A42B99C80")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        UserProfile UserProfile { get; set; }
    }
}

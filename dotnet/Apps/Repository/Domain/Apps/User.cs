// <copyright file="User.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    public partial interface User : Localised, IDisplayName
    {
        #region Allors
        [Id("372F197C-3B6E-4C12-9BD9-D50A42B99C80")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        UserProfile UserProfile { get; set; }

        #region Allors
        [Id("f78367fc-0567-484f-ad3e-fdd4d12a1dcd")]
        #endregion
        [Required]
        [Derived]
        [Workspace]
        bool IsUser { get; set; }

        #region Allors
        [Id("fc3c192c-8d69-4e68-a996-825d89583ddf")]
        #endregion
        [Workspace(Default)]
        void ResetPassword();
    }
}

// <copyright file="Agent.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    // A user that is not a Person, so that the tests do not tie users to one class.
    #region Allors
    [Id("6A4E2F96-2539-46DC-9DA9-732805F6A38A")]
    #endregion
    [Workspace(Default)]
    public partial class Agent : User
    {
        #region inherited properties

        public Guid UniqueId { get; set; }

        public SecurityToken OwnerSecurityToken { get; set; }

        public Grant OwnerGrant { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string UserName { get; set; }

        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}

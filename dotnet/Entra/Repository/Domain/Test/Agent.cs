// <copyright file="Agent.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    // A user that is not a Person: a program that calls the application with a token of its own.
    // The Test domain creates one for a program's token, so that the tree proves that the plug-in
    // never decides the class of a user.
    #region Allors
    [Id("6A4E2F96-2539-46DC-9DA9-732805F6A38A")]
    #endregion
    public partial class Agent : User
    {
        #region inherited properties

        public Guid UniqueId { get; set; }

        public SecurityToken OwnerSecurityToken { get; set; }

        public Grant OwnerGrant { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid EntraTenantId { get; set; }

        public Guid EntraObjectId { get; set; }

        public string EntraUserName { get; set; }

        public string EntraDisplayName { get; set; }

        public string EntraEmail { get; set; }

        public string EntraIdentityProvider { get; set; }

        public bool EntraIsGuest { get; set; }

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

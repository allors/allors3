// <copyright file="Person.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("228badf5-958b-4339-988d-51415d601042")]
    #endregion
    [Workspace(Default)]
    public partial class Person : User
    {
        #region inherited properties

        public Guid UniqueId { get; set; }

        public SecurityToken OwnerSecurityToken { get; set; }

        public Grant OwnerGrant { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public string InExistingUserPassword { get; set; }

        public string InUserPassword { get; set; }

        public string UserPasswordHash { get; set; }

        public string UserEmail { get; set; }

        public string NormalizedUserEmail { get; set; }

        public bool UserEmailConfirmed { get; set; }

        public string UserSecurityStamp { get; set; }

        public string UserPhoneNumber { get; set; }

        public bool UserPhoneNumberConfirmed { get; set; }

        public bool UserTwoFactorEnabled { get; set; }

        public DateTime UserLockoutEnd { get; set; }

        public bool UserLockoutEnabled { get; set; }

        public int UserAccessFailedCount { get; set; }

        public Login[] Logins { get; set; }

        #endregion

        #region Allors
        [Id("a358cae5-83e9-490b-9895-d675eada3df2")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string FirstName { get; set; }

        #region Allors
        [Id("8b5520cb-ecf1-488c-b723-94a92168e2eb")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string LastName { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}

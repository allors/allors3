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
    [Id("569f21ac-e28a-4158-8c07-9588a2b7d907")]
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
        [Id("5f1736df-8159-44cb-b125-05b97579dbd1")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string FirstName { get; set; }

        #region Allors
        [Id("98fa7fd1-690f-4c5d-a4f1-ae43dc374e55")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string LastName { get; set; }

        #region Allors
        [Id("6a527d18-54a4-4a94-bfe8-0952becd4744")]
        #endregion
        [Derived]
        [Required]
        [Workspace(Default)]
        public int OpenWorkItemCount { get; set; }

        #region inherited methods
        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        public void Delete() { }
        #endregion
    }
}

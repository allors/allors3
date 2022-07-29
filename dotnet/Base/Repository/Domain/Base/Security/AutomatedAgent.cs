// <copyright file="AutomatedAgent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("3587d2e1-c3f6-4c55-a96c-016e0501d99c")]
    #endregion
    public partial class AutomatedAgent : User
    {
        #region inherited properties
        public Guid UniqueId { get; set; }

        public SecurityToken OwnerSecurityToken { get; set; }

        public Grant OwnerGrant { get; set; }

        public NotificationList NotificationList { get; set; }

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

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("4e158d75-d0b5-4cb7-ad41-e8ed3002d175")]
        #endregion
        [Indexed]
        [Size(256)]
        public string Name { get; set; }

        #region Allors
        [Id("58870c93-b066-47b7-95f7-5411a46dbc7e")]
        #endregion
        [Size(-1)]
        public string Description { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}

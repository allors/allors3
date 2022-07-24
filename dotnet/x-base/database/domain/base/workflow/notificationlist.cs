// <copyright file="NotificationList.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class NotificationList
    {
        public void BaseOnPostDerive(ObjectOnPostDerive _)
        {
            if (!this.ExistSecurityTokens && this.ExistUserWhereNotificationList)
            {
                var defaultSecurityToken = new SecurityTokens(this.Transaction()).DefaultSecurityToken;
                this.SecurityTokens = new[] { this.UserWhereNotificationList.OwnerSecurityToken, defaultSecurityToken };
            }
        }
    }
}

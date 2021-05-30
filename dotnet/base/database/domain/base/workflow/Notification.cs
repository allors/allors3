// <copyright file="Notification.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Notification
    {
        public void BaseOnBuild(ObjectOnBuild _)
        {
            if (!this.ExistDateCreated)
            {
                this.DateCreated = this.Strategy.Transaction.Now();
            }

            if (!this.ExistConfirmed)
            {
                this.Confirmed = false;
            }
        }

        public void BaseConfirm(NotificationConfirm method)
        {
            this.Confirmed = true;
            method.StopPropagation = true;
        }

        public void BaseOnPostDerive(ObjectOnPostDerive _)
        {
            if (!this.ExistSecurityTokens)
            {
                if (this.ExistNotificationListWhereNotification && this.NotificationListWhereNotification.ExistUserWhereNotificationList)
                {
                    var user = this.NotificationListWhereNotification.UserWhereNotificationList;
                    var defaultSecurityToken = new SecurityTokens(this.Transaction()).DefaultSecurityToken;

                    this.SecurityTokens = new[] { user.OwnerSecurityToken, defaultSecurityToken };
                }
            }
        }
    }
}

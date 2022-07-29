// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Derivations.Rules;
    using Meta;

    public class NotificationListRule : Rule
    {
        public NotificationListRule(MetaPopulation m) : base(m, new Guid("e8071e5b-18a4-4a52-8b22-09a75c3dbf72")) =>
            this.Patterns = new Pattern[]
            {
                m.NotificationList.RolePattern(v=>v.Notifications),
                m.Notification.RolePattern(v=>v.Confirmed, v=> v.NotificationListWhereNotification),
                m.Notification.RolePattern(v=>v.Confirmed, v=> v.NotificationListWhereUnconfirmedNotification),
                m.Notification.RolePattern(v=>v.Confirmed, v=> v.NotificationListWhereConfirmedNotification),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<NotificationList>())
            {
                @this.UnconfirmedNotifications = @this.Notifications.Where(notification => !notification.Confirmed).ToArray();
                @this.ConfirmedNotifications = @this.Notifications.Where(notification => notification.Confirmed).ToArray();

                if (!@this.ExistSecurityTokens)
                {
                    if (@this.ExistUserWhereNotificationList)
                    {
                        var defaultSecurityToken = new SecurityTokens(@this.Transaction()).DefaultSecurityToken;
                        @this.SecurityTokens = new[] { @this.UserWhereNotificationList.OwnerSecurityToken, defaultSecurityToken };
                    }
                }
            }
        }
    }
}

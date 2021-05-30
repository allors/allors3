// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations.Rules;
    using Meta;

    public class NotificationListRule : Rule
    {
        public NotificationListRule(MetaPopulation m) : base(m, new Guid("5affa463-9365-4916-89ef-cfc18d41b4fb")) =>
            this.Patterns = new Pattern[]
            {
                m.NotificationList.RolePattern(v=>v.Notifications),
                m.Notification.RolePattern(v=>v.Confirmed, v=> v.NotificationListWhereNotification),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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

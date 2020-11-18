// <copyright file="ContactMechanism.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class CommunicationTask
    {
        public void ManageNotification(TaskAssignment taskAssignment)
        {
            if (!taskAssignment.ExistNotification && this.CommunicationEvent.SendNotification == true && this.CommunicationEvent.RemindAt < this.Strategy.Session.Now())
            {
                var notification = new NotificationBuilder(this.Strategy.Session)
                    .WithTitle("CommunicationEvent: " + this.WorkItem.WorkItemDescription)
                    .WithDescription("CommunicationEvent: " + this.WorkItem.WorkItemDescription)
                    .WithTarget(this)
                    .Build();

                taskAssignment.Notification = notification;
                taskAssignment.User.NotificationList.AddNotification(notification);
            }
        }
    }
}

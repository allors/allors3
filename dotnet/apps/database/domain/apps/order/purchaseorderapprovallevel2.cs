// <copyright file="PurchaseOrderApprovalLevel2.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PurchaseOrderApprovalLevel2
    {
        public void AppsApprove(PurchaseOrderApprovalLevel2Approve method)
        {
            this.AssignPerformer();

            this.PurchaseOrder.Approve();

            if (!this.ExistApprovalNotification && this.PurchaseOrder.ExistCreatedBy)
            {
                var now = this.Strategy.Transaction.Now();
                var workItemDescription = this.WorkItem.WorkItemDescription;
                var performerName = this.Performer.UserName;
                var comment = this.Comment ?? "N/A";

                var description = $"<h2>Approved...</h2>" +
                                  $"<p>On {now:D} {workItemDescription} was approved by {performerName}</p>" +
                                  $"<h3>Comment</h3>" +
                                  $"<p>{comment}</p>";

                this.ApprovalNotification = new NotificationBuilder(this.Transaction())
                    .WithTitle("PurchaseOrder approved")
                    .WithDescription(description)
                    .Build();

                this.PurchaseOrder.CreatedBy.NotificationList.AddNotification(this.ApprovalNotification);
            }

            method.StopPropagation = true;
        }

        public void AppsReject(PurchaseOrderApprovalLevel2Reject method)
        {
            this.AssignPerformer();

            this.PurchaseOrder.Reject();

            if (!this.ExistRejectionNotification && this.PurchaseOrder.ExistCreatedBy)
            {
                var now = this.Strategy.Transaction.Now();
                var workItemDescription = this.WorkItem.WorkItemDescription;
                var performerName = this.Performer.UserName;
                var comment = this.Comment ?? "N/A";

                var description = $"<h2>Approval Rejected...</h2>" +
                                  $"<p>On {now:D} {workItemDescription} was rejected by {performerName}</p>" +
                                  $"<h3>Comment</h3>" +
                                  $"<p>{comment}</p>";

                this.RejectionNotification = new NotificationBuilder(this.Transaction())
                    .WithTitle("Approval Rejected")
                    .WithDescription(description)
                    .Build();

                this.PurchaseOrder.CreatedBy.NotificationList.AddNotification(this.RejectionNotification);
            }

            method.StopPropagation = true;
        }

        public void ManageNotification(TaskAssignment taskAssignment)
        {
            if (!taskAssignment.ExistNotification)
            {
                var notification = new NotificationBuilder(this.Strategy.Transaction).WithTitle(
                        "Approval: " + this.WorkItem.WorkItemDescription)
                        .WithDescription("Approval: " + this.WorkItem.WorkItemDescription)
                        .WithTarget(this)
                        .Build();

                taskAssignment.Notification = notification;
                taskAssignment.User.NotificationList.AddNotification(notification);
            }
        }
    }
}

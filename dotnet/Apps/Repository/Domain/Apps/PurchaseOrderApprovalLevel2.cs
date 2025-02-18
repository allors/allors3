// <copyright file="PurchaseOrderApprovalLevel2.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("AC2A9DE8-9368-4BD5-9114-7F14DE98027B")]
    #endregion
    [Plural("PurchaseOrderApprovalsLevel2")]
    public partial class PurchaseOrderApprovalLevel2 : ApproveTask
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public WorkItem WorkItem { get; set; }

        public string Title { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateDue { get; set; }

        public DateTime DateClosed { get; set; }

        public User[] Participants { get; set; }

        public User Performer { get; set; }

        public string Comment { get; set; }

        public Notification ApprovalNotification { get; set; }

        public Notification RejectionNotification { get; set; }

        #endregion

        #region Allors
        [Id("FC25DD98-D812-4FF0-8C5A-83CF77E6C3E7")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Required]
        public PurchaseOrder PurchaseOrder { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        public void Approve() { }

        public void Reject() { }

        #endregion
    }
}

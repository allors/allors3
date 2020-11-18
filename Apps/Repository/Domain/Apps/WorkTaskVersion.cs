// <copyright file="WorkTaskVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("448FA98B-5683-4F0E-9745-AAA1093F5614")]
    #endregion
    public partial class WorkTaskVersion : WorkEffortVersion
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public WorkEffortState WorkEffortState { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Priority Priority { get; set; }

        public WorkEffortPurpose[] WorkEffortPurposes { get; set; }

        public DateTime ActualCompletion { get; set; }

        public DateTime ScheduledStart { get; set; }

        public DateTime ScheduledCompletion { get; set; }

        public decimal ActualHours { get; set; }

        public decimal EstimatedHours { get; set; }

        public WorkEffort[] Precendencies { get; set; }

        public Facility Facility { get; set; }

        public Deliverable[] DeliverablesProduced { get; set; }

        public DateTime ActualStart { get; set; }

        public WorkEffort[] Children { get; set; }

        public OrderItem OrderItemFulfillment { get; set; }

        public WorkEffortType WorkEffortType { get; set; }

        public Requirement[] RequirementFulfillments { get; set; }

        public string SpecialTerms { get; set; }

        public WorkEffort[] Concurrencies { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public User LastModifiedBy { get; set; }

        public Organisation TakenBy { get; set; }

        #endregion

        #region Allors
        [Id("2C32FA14-D400-407E-B701-0B02BBB30404")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public bool SendNotification { get; set; }

        #region Allors
        [Id("D15B652A-76B9-46C0-A948-ABC7F78E3AA9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public bool SendReminder { get; set; }

        #region Allors
        [Id("5119D560-AA72-4D16-B770-6155D21D0321")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public DateTime RemindAt { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion
    }
}

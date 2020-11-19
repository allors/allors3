// <copyright file="TimeEntry.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("6360b45d-3556-41c6-b183-f42a15b9424f")]
    #endregion
    [Plural("TimeEntries")]
    public partial class TimeEntry : ServiceEntry, DelegatedAccessControlledObject
    {
        #region inherited properties
        public EngagementItem EngagementItem { get; set; }

        public bool IsBillable { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public string Description { get; set; }

        public WorkEffort WorkEffort { get; set; }

        public string Comment { get; set; }

        public Guid DerivationTrigger { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("0BF79180-E5A6-44BD-ACC7-1A1563E29152")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public Person Worker { get; set; }

        #region Allors
        [Id("E086CAE8-62C2-4892-AC97-004A811A3904")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public RateType RateType { get; set; }

        #region Allors
        [Id("1b07c419-42af-480b-87ba-1c001995dc51")]
        #endregion
        [Required]
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal Cost { get; set; }

        #region Allors
        [Id("1bb9affa-1390-4f54-92b5-64997e55525e")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal GrossMargin { get; set; }

        #region Allors
        [Id("258a33cc-06ea-45a0-9b15-1b6d58385910")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public QuoteTerm QuoteTerm { get; set; }

        #region Allors
        [Id("E478A603-B2DA-4C76-91A5-96C5A737FCFC")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal AssignedBillingRate { get; set; }

        #region Allors
        [Id("2c33de6e-b4fd-47e4-b254-2991f33f01f1")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Derived]
        [Workspace(Default)]
        public decimal BillingRate { get; set; }

        #region Allors
        [Id("409ff1fb-1531-4829-9d6b-7b3e7113594a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public TimeFrequency BillingFrequency { get; set; }

        #region Allors
        [Id("c163457c-6a36-45ab-8c62-e555128afbfc")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public decimal AmountOfTime { get; set; }

        #region Allors
        [Id("B53DFC71-1BEA-40BC-B3F9-02F4FEA0EC43")]
        #endregion
        [Workspace(Default)]
        public decimal AssignedAmountOfTime { get; set; }

        #region Allors
        [Id("816719D2-8386-4D19-BF3F-D1AC9A6BFB4F")]
        #endregion
        [Workspace(Default)]
        public decimal BillableAmountOfTime { get; set; }

        #region Allors
        [Id("430F0646-64C9-40EA-89AE-A07A30AF85B4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public TimeFrequency TimeFrequency { get; set; }

        #region Allors
        [Id("E1121F4A-5CE3-4966-9DF0-CCA9A7DCCEB6")]
        #endregion
        [Required]
        [Derived]
        [Workspace(Default)]
        public decimal BillingAmount { get; set; }

        #region Allors
        [Id("aceb7dfb-6838-4754-a3ac-bff9c8f942e8")]
        #endregion
        [Derived]
        [Required]
        [Workspace(Default)]
        public decimal AmountOfTimeInMinutes { get; set; }

        #region Allors
        [Id("2c80b0e5-ae04-423a-841b-a6a5d1481bf1")]
        #endregion
        [Derived]
        [Required]
        [Workspace(Default)]
        public decimal BillableAmountOfTimeInMinutes { get; set; }

        #region inherited methods
        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }

        public void DelegateAccess() { }

        #endregion
    }
}

// <copyright file="FaxCommunicationVersion.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("B64D00D7-03C7-4D7A-B0A2-0825234C2070")]
    #endregion
    public partial class FaxCommunicationVersion : CommunicationEventVersion
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public DateTime ScheduledStart { get; set; }

        public Party FromParty { get; set; }

        public Party ToParty { get; set; }

        public ContactMechanism[] ContactMechanisms { get; set; }

        public Party[] InvolvedParties { get; set; }

        public DateTime InitialScheduledStart { get; set; }

        public CommunicationEventState CommunicationEventState { get; set; }

        public string Comment { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public CommunicationEventPurpose[] EventPurposes { get; set; }

        public DateTime ScheduledEnd { get; set; }

        public DateTime ActualEnd { get; set; }

        public WorkEffort[] WorkEfforts { get; set; }

        public string Description { get; set; }

        public DateTime InitialScheduledEnd { get; set; }

        public string Subject { get; set; }

        public Media[] Documents { get; set; }

        public Case Case { get; set; }

        public Priority Priority { get; set; }

        public Person Owner { get; set; }

        public DateTime ActualStart { get; set; }

        public bool SendNotification { get; set; }

        public bool SendReminder { get; set; }

        public DateTime RemindAt { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }
        #endregion

        #region Allors
        [Id("23DDDD16-BD4F-4CAD-9B45-1B34D17A4D97")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public TelecommunicationsNumber FaxNumber { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}

// <copyright file="FaxCommunication.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("1e67320b-9680-4477-bf1b-70ccd24ab758")]
    #endregion
    public partial class FaxCommunication : CommunicationEvent, Versioned
    {
        #region inherited properties

        public CommunicationEventState PreviousCommunicationEventState { get; set; }

        public CommunicationEventState LastCommunicationEventState { get; set; }

        public CommunicationEventState CommunicationEventState { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public DateTime ScheduledStart { get; set; }

        public Party FromParty { get; set; }

        public Party ToParty { get; set; }

        public ContactMechanism[] ContactMechanisms { get; set; }

        public Party[] InvolvedParties { get; set; }

        public DateTime InitialScheduledStart { get; set; }

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

        public Permission[] DeniedPermissions { get; set; }

        

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Guid UniqueId { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }
        #endregion

        #region Allors
        [Id("8797fd5b-0d89-420f-b656-aff35b50e75c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public TelecommunicationsNumber FaxNumber { get; set; }

        #region Versioning
        #region Allors
        [Id("D535262B-CD3F-4440-AC29-6211B3036A49")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public FaxCommunicationVersion CurrentVersion { get; set; }

        #region Allors
        [Id("E4B068A0-15ED-422A-AE2F-EAA6BC975618")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public FaxCommunicationVersion[] AllVersions { get; set; }
        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Cancel() { }

        public void Close() { }

        public void Reopen() { }

        public void Delete() { }

        #endregion

        public string WorkItemDescription { get; set; }
    }
}

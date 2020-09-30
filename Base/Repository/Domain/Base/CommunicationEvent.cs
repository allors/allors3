// <copyright file="CommunicationEvent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("b05371ff-0c9e-4ee3-b31d-e2edeed8649e")]
    #endregion
    public partial interface CommunicationEvent : Deletable, Commentable, UniquelyIdentifiable, Auditable, WorkItem, Transitional
    {
        #region ObjectStates
        #region EmailCommunicationState
        #region Allors
        [Id("CEB2FD49-5104-454F-BA3A-5B711B36CF84")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        CommunicationEventState PreviousCommunicationEventState { get; set; }

        #region Allors
        [Id("F26D8789-D8D8-47C0-A4A2-30A3B2F648F5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        CommunicationEventState LastCommunicationEventState { get; set; }

        #region Allors
        [Id("80D2E559-CBF6-4C2F-8F89-43921EEF437C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        CommunicationEventState CommunicationEventState { get; set; }
        #endregion
        #endregion

        #region Allors
        [Id("01665c57-a343-441d-9760-53763badce51")]
        #endregion
        [Workspace(Default)]
        DateTime ScheduledStart { get; set; }

        #region Allors
        [Id("7384d5c7-9af9-45b0-9969-dffe9781cc8c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        Party FromParty { get; set; }

        #region Allors
        [Id("16c8aada-318c-4bbb-b8a7-7fa20120eda4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        Party ToParty { get; set; }

        #region Allors
        [Id("1aacf179-cf9f-43e1-b950-4121809fde2d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        ContactMechanism[] ContactMechanisms { get; set; }

        #region Allors
        [Id("28874ffe-f3b3-4aba-9f28-ba7c15b0cb65")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        Party[] InvolvedParties { get; set; }

        #region Allors
        [Id("2fa315f8-6208-495c-bcc4-2ccda734cc09")]
        #endregion
        [Workspace(Default)]
        DateTime InitialScheduledStart { get; set; }

        #region Allors
        [Id("3a5658bd-b1b9-47e3-b542-ea9de348a44e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        CommunicationEventPurpose[] EventPurposes { get; set; }

        #region Allors
        [Id("3bc21bd3-1af9-492d-8dde-b0696e20a11a")]
        #endregion
        [Workspace(Default)]
        DateTime ScheduledEnd { get; set; }

        #region Allors
        [Id("43c26f1f-25bd-4b45-9cdf-c81d021b0b37")]
        #endregion
        [Workspace(Default)]
        DateTime ActualEnd { get; set; }

        #region Allors
        [Id("51f3e08a-7b1b-4d5b-989c-ad2c734a1b2f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        WorkEffort[] WorkEfforts { get; set; }

        #region Allors
        [Id("52adc5f3-d6ef-4804-8755-b86532d8b6fe")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("65499ae5-ab06-4d21-8f94-8bf95a665e3d")]
        #endregion
        [Workspace(Default)]
        DateTime InitialScheduledEnd { get; set; }

        #region Allors
        [Id("79e945d3-1200-4a90-8e80-eba298bcda40")]
        #endregion
        [Required]
        [Size(-1)]
        [Workspace(Default)]
        string Subject { get; set; }

        #region Allors
        [Id("91a1555b-a126-4727-86a4-e57e20ebb5da")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        Media[] Documents { get; set; }

        #region Allors
        [Id("9e52b6a3-3f94-43d6-9fda-879f57499c05")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Case Case { get; set; }

        #region Allors
        [Id("bdf87e9c-4ca3-4fba-8b3e-c1252f849953")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Priority Priority { get; set; }

        #region Allors
        [Id("c43b6f6f-0fda-4794-9199-84b39373ecb3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Person Owner { get; set; }

        #region Allors
        [Id("ecc20a6a-ef70-4a09-8a3b-c8dce88eaa27")]
        #endregion
        [Workspace(Default)]
        DateTime ActualStart { get; set; }

        #region Allors
        [Id("7A604BA4-05CA-4F01-8DF5-200D1831F8D7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        bool SendNotification { get; set; }

        #region Allors
        [Id("A6EBA67F-7C65-44AF-9B66-03EB07165CD6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        bool SendReminder { get; set; }

        #region Allors
        [Id("93759DE2-9170-41C9-A641-24D44B89F10F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        DateTime RemindAt { get; set; }

        #region Allors
        [Id("F1D66D21-15CC-45C3-980C-E4179F66FD57")]
        #endregion
        [Workspace(Default)]
        void Cancel();

        #region Allors
        [Id("97011DA3-10B1-4B27-A4A0-E06D5D6CE04A")]
        #endregion
        [Workspace(Default)]
        void Close();

        #region Allors
        [Id("731D1CF2-01CE-44FE-8065-762E4DB1C5E0")]
        #endregion
        [Workspace(Default)]
        void Reopen();
    }
}

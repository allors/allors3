// <copyright file="CommunicationEventVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;
    using static Workspaces;

    #region Allors
    [Id("8407B4DF-88BF-43E5-89DA-999A32B16CF5")]
    #endregion
    public partial interface CommunicationEventVersion : Version
    {
        #region Allors
        [Id("2B30E8D7-CF09-448C-9339-D90C5111CF6E")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Required]
        [Workspace(Default)]
        CommunicationEventState CommunicationEventState { get; set; }

        #region Allors
        [Id("A4FD08D4-8665-4740-A3D0-04A7DF1B019E")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Comment { get; set; }

        #region Allors
        [Id("660C7D51-2EF0-416A-8136-767469DBD9A1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        User CreatedBy { get; set; }

        #region Allors
        [Id("763B74F2-5CBA-4868-A405-CAE9283175AE")]
        #endregion
        [Workspace(Default)]
        DateTime CreationDate { get; set; }

        #region Allors
        [Id("07F06A16-BB1E-48D8-919B-4DFF31A1F4FD")]
        #endregion
        [Workspace(Default)]
        DateTime LastModifiedDate { get; set; }

        #region Allors
        [Id("9D549ABA-3EB6-44C1-BB29-BEF13A50D41E")]
        #endregion
        [Workspace(Default)]
        DateTime ScheduledStart { get; set; }

        #region Allors
        [Id("2759C5DA-45E4-4715-83C7-B68A51E213DB")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        Party FromParty { get; set; }

        #region Allors
        [Id("81B2EC68-0FF0-49D9-813B-E32BBC3F8872")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        Party ToParty { get; set; }

        #region Allors
        [Id("0273F21F-8794-47FD-A5D9-34912D54C281")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        ContactMechanism[] ContactMechanisms { get; set; }

        #region Allors
        [Id("E523242D-553F-4A2D-8663-072CCC693089")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        Party[] InvolvedParties { get; set; }

        #region Allors
        [Id("5E653B50-7BC1-4D0A-BA2F-965A8FC4AD6C")]
        #endregion
        [Workspace(Default)]
        DateTime InitialScheduledStart { get; set; }

        #region Allors
        [Id("8FFCBA83-FEE3-42C8-9102-F2F0380A411A")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        CommunicationEventPurpose[] EventPurposes { get; set; }

        #region Allors
        [Id("D0E28A3B-0136-453B-A4A5-3117C47F7F1E")]
        #endregion
        [Workspace(Default)]
        DateTime ScheduledEnd { get; set; }

        #region Allors
        [Id("37C90ED0-40BA-4210-88E2-8539EAB440A9")]
        #endregion
        [Workspace(Default)]
        DateTime ActualEnd { get; set; }

        #region Allors
        [Id("8CC7CCAD-293D-4264-A20F-558159673273")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        WorkEffort[] WorkEfforts { get; set; }

        #region Allors
        [Id("FB90D1C1-0A0D-4C31-BF4C-4D0CA255231C")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("B5188C59-241A-4695-9C3D-A162D44A7240")]
        #endregion
        [Workspace(Default)]
        DateTime InitialScheduledEnd { get; set; }

        #region Allors
        [Id("22D2407E-86FE-46FB-9C59-E92306330027")]
        #endregion
        [Required]
        [Size(-1)]
        [Workspace(Default)]
        string Subject { get; set; }

        #region Allors
        [Id("9AD00AF2-8DFA-4167-9619-BD9C265DADFC")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        Media[] Documents { get; set; }

        #region Allors
        [Id("325F7DE6-356D-4A21-913A-4EFE6EDE6A95")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Case Case { get; set; }

        #region Allors
        [Id("26063624-EDAF-4E2F-A2FC-75523471FD3F")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Priority Priority { get; set; }

        #region Allors
        [Id("26868123-D390-4EB2-8570-EB26BE158D1B")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Person Owner { get; set; }

        #region Allors
        [Id("4081E134-3D15-474F-9C0E-8E7C20CE5EB2")]
        #endregion
        [Workspace(Default)]
        DateTime ActualStart { get; set; }

        #region Allors
        [Id("B5DF3131-04F2-4DEF-BC41-2581F3CC5923")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        bool SendNotification { get; set; }

        #region Allors
        [Id("C7CE474E-1677-4EF2-ABF9-CE5FF26CA075")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        bool SendReminder { get; set; }

        #region Allors
        [Id("48329980-DACF-436E-A8B2-2E46CE0B07C3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        DateTime RemindAt { get; set; }
    }
}

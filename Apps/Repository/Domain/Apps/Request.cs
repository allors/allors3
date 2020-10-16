// <copyright file="Request.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("321a6047-2233-4bec-a1b1-9b965c0099e5")]
    #endregion
    public partial interface Request : Transitional, Commentable, Auditable, Printable, Deletable
    {
        #region ObjectStates
        #region RequestState
        #region Allors
        [Id("6F9BFBFD-2A58-470A-894C-17754EC725A7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        RequestState PreviousRequestState { get; set; }

        #region Allors
        [Id("5EC7CC5D-2652-4389-832A-999007764316")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        RequestState LastRequestState { get; set; }

        #region Allors
        [Id("758AF277-DB45-4E3A-9055-3507BB52DF46")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        RequestState RequestState { get; set; }
        #endregion
        #endregion

        #region Allors
        [Id("58D0A882-0E19-4158-B6D7-B27D5DC5CD4B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        InternalOrganisation Recipient { get; set; }

        #region Allors
        [Id("918022F4-D2D6-4596-AF42-2009E981AE73")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string InternalComment { get; set; }

        #region Allors
        [Id("1bb3a4b8-224a-47ab-b05b-c0c8a87ec09c")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("0DE2002C-D38C-44A3-9475-471C95B90919")]
        #endregion
        [Required]
        [Workspace(Default)]
        DateTime RequestDate { get; set; }

        #region Allors
        [Id("208f711f-5d9d-4dc3-89ad-b1583ad06582")]
        #endregion
        [Workspace(Default)]
        DateTime RequiredResponseDate { get; set; }

        #region Allors
        [Id("25332874-3ec6-41d8-ac6a-77dd4328e503")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        RequestItem[] RequestItems { get; set; }

        #region Allors
        [Id("8ac90ec6-9d3e-45fe-aaba-27d0c1c058a1")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string RequestNumber { get; set; }

        #region Allors
        [Id("c3389cec-ee8e-45e2-a1eb-01c9a87b2df0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        RespondingParty[] RespondingParties { get; set; }

        #region Allors
        [Id("f1a50d9d-2e79-45ac-8f23-8f38fab985c1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Party Originator { get; set; }

        #region Allors
        [Id("BBCF2C40-E793-4FA4-B4E1-612E20971408")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Currency Currency { get; set; }

        #region Allors
        [Id("0FB57C62-05C2-47FA-940C-5285BECB7458")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        ContactMechanism FullfillContactMechanism { get; set; }

        #region Allors
        [Id("00066124-DEC8-47DA-B00D-28A3D4BC949D")]
        #endregion
        [Workspace(Default)]
        string EmailAddress { get; set; }

        #region Allors
        [Id("04D35F19-31F1-4AA5-A2FB-C02B2D3CBC08")]
        #endregion
        [Workspace(Default)]
        string TelephoneNumber { get; set; }

        #region Allors
        [Id("C38275E1-AC46-46C9-9E27-B08D6BA30BE3")]
        #endregion
        [Workspace(Default)]
        string TelephoneCountryCode { get; set; }

        #region Allors
        [Id("44877BC8-F0E2-4D22-BEC5-8CA4E31BF953")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Person ContactPerson { get; set; }

        #region Allors
        [Id("2556a000-cdbf-44a8-83b7-17dff2a5a10b")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        int SortableRequestNumber { get; set; }

        #region Allors

        [Id("8C8032CB-4FEC-4EAC-8EB7-C51A2223F556")]

        #endregion
        [Workspace(Default)]
        void CreateQuote();

        #region Allors
        [Id("B30EDA48-5E99-44AE-B3A9-D053BCFA4895")]
        #endregion
        [Workspace(Default)]
        void Cancel();

        #region Allors
        [Id("2510F8F6-52E1-4024-A0B1-623DFB62395A")]
        #endregion
        [Workspace(Default)]
        void Reject();

        #region Allors
        [Id("7458A51F-5EAD-41A0-B44C-A22B4BA2A372")]
        #endregion
        [Workspace(Default)]
        void Submit();

        #region Allors
        [Id("0E26CA10-B0D4-47B0-BEDE-F8EC1AE6BD36")]
        #endregion
        [Workspace(Default)]
        void Hold();
    }
}

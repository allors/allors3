// <copyright file="Shipment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("9c6f4ad8-5a4e-4b6e-96b7-876f7aabcffb")]
    #endregion
    public partial interface Shipment : Printable, Commentable, Auditable, Transitional, Deletable
    {
        #region ObjectStates
        #region ShipmentState
        #region Allors
        [Id("DBC484A2-6EA0-47E9-8EAF-DFC5067CF34C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        ShipmentState PreviousShipmentState { get; set; }

        #region Allors
        [Id("589116AF-CE7E-4894-BD4D-8089FBBA7358")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        ShipmentState LastShipmentState { get; set; }

        #region Allors
        [Id("A65BBB49-2B63-4573-BBCC-5DDF13C86518")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        ShipmentState ShipmentState { get; set; }
        #endregion
        #endregion

        #region Allors
        [Id("05221b28-9c80-4d3b-933f-12a8a17bc261")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        ShipmentMethod ShipmentMethod { get; set; }

        #region Allors
        [Id("17234c66-6b61-4ac9-a23b-4388e19f4888")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        string ShipmentNumber { get; set; }

        #region Allors
        [Id("f1e92d31-db63-419c-8ed7-49f5db66c63d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Party ShipFromParty { get; set; }

        #region Allors
        [Id("c8b0eff8-4dff-449c-9d44-a7235cd24928")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        PostalAddress ShipFromAddress { get; set; }

        #region Allors
        [Id("AC3FC462-A86D-481D-A5A9-F4B397E8B774")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Person ShipFromContactPerson { get; set; }

        #region Allors
        [Id("40277d59-6ab8-40b0-acee-c95ba759e2c8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Facility ShipFromFacility { get; set; }

        #region Allors
        [Id("5891b368-89cd-4a0e-aaef-439f442909c8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Party ShipToParty { get; set; }

        #region Allors
        [Id("7e1325e0-a072-46da-adb5-b997dde9980a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        PostalAddress ShipToAddress { get; set; }

        #region Allors
        [Id("D0A863A1-5A97-42B2-AE35-CFD964B6DE4F")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Person ShipToContactPerson { get; set; }

        #region Allors
        [Id("E709E429-F7FA-4EAD-8AB4-3E029F64AE6B")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Facility ShipToFacility { get; set; }

        #region Allors
        [Id("6a568bea-6718-414a-b822-d8304502be7b")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        ShipmentItem[] ShipmentItems { get; set; }

        #region Allors
        [Id("894ecdf3-1322-4513-bf94-63882c5c29bf")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal EstimatedShipCost { get; set; }

        #region Allors
        [Id("97788e21-ec31-4fb2-9ef7-0b7b5a7367a1")]
        #endregion
        [Workspace(Default)]
        DateTime EstimatedShipDate { get; set; }

        #region Allors
        [Id("a74391e5-bd03-4247-93b8-e7081d939823")]
        #endregion
        [Workspace(Default)]
        DateTime LatestCancelDate { get; set; }

        #region Allors
        [Id("b37c7c90-0287-4f12-b000-025e2505499c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Carrier Carrier { get; set; }

        #region Allors
        [Id("b69c6812-bdc4-4a06-a782-fa8ff4a71aca")]
        #endregion
        [Workspace(Default)]
        DateTime EstimatedReadyDate { get; set; }

        #region Allors
        [Id("ee49c6ca-bb03-40d3-97f1-004cc5a31132")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string HandlingInstruction { get; set; }

        #region Allors
        [Id("f1059139-6664-43d5-801f-79a4cc4288a6")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Store Store { get; set; }

        #region Allors
        [Id("165b529f-df1c-45b6-bbed-d19ffcb375f2")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        ShipmentPackage[] ShipmentPackages { get; set; }

        #region Allors
        [Id("18808545-f941-4c5a-8809-0f1fb0cca2d8")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        Document[] Documents { get; set; }

        #region Allors
        [Id("A6444620-783D-40E1-A908-001B41A5F097")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        Media[] ElectronicDocuments { get; set; }

        #region Allors
        [Id("f403ab39-cc81-4e09-8794-a45db9ef178f")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        ShipmentRouteSegment[] ShipmentRouteSegments { get; set; }

        #region Allors
        [Id("fdac3beb-edf8-4d1b-80d4-21b643ef43ce")]
        #endregion
        [Workspace(Default)]
        DateTime EstimatedArrivalDate { get; set; }

        #region Allors
        [Id("6708b143-57a5-4634-8eef-29fa143665b1")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        int SortableShipmentNumber { get; set; }

        #region Allors

        [Id("11D44169-2D96-4310-AD6C-59417D8CA0C2")]

        #endregion
        [Workspace(Default)]
        void Create();

        #region Allors
        [Id("F6B4B2D0-A896-480E-A441-F15AB11A3CC9")]
        #endregion
        [Workspace(Default)]
        void Invoice();
    }
}

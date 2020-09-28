// <copyright file="ShipmentVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("F76BB372-D433-4479-8324-3A232AC50A25")]
    #endregion
    public partial interface ShipmentVersion : Version
    {
        #region Allors
        [Id("2045FACF-3F58-4A6F-94CB-CC8369619EBB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        ShipmentState ShipmentState { get; set; }

        #region Allors
        [Id("91A8835F-1628-43BC-8564-C5D3DB90F40F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        ShipmentMethod ShipmentMethod { get; set; }

        #region Allors
        [Id("92BA5224-5123-462F-A1DF-EDDE727B2054")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace]
        string ShipmentNumber { get; set; }

        #region Allors
        [Id("DFFCADE4-DECB-4BC0-94E2-E1F1FC46D9B0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        Party ShipFromParty { get; set; }

        #region Allors
        [Id("C1E6385D-6893-49A0-B88B-CA1A0E3BC2DC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        PostalAddress ShipFromAddress { get; set; }

        #region Allors
        [Id("01E13426-3D0A-44FE-A24D-6155BED5BFB7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        Person ShipFromContactPerson { get; set; }

        #region Allors
        [Id("D32E5998-6193-47AF-AEAA-F754FCCD9879")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        Facility ShipFromFacility { get; set; }

        #region Allors
        [Id("A8EBADA5-1B73-4FB5-86C0-FC2EDD9C3264")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        Party ShipToParty { get; set; }

        #region Allors
        [Id("37AF13D7-3530-4249-872F-FF1E835F33F1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        PostalAddress ShipToAddress { get; set; }

        #region Allors
        [Id("15E1AB0C-18E4-4C7C-96AF-E27FACEAF6EC")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        Person ShipToContactPerson { get; set; }

        #region Allors
        [Id("365A1590-E9B5-4183-9C6D-485E23C4D84E")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        Facility ShipToFacility { get; set; }

        #region Allors
        [Id("926845AF-224F-455E-B874-E5AF7D25F63F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace]
        Document[] Documents { get; set; }

        #region Allors
        [Id("50757F14-FA1C-48B0-85E0-2EC4491C335C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace]
        ShipmentItem[] ShipmentItems { get; set; }

        #region Allors
        [Id("95B1D9E9-9FE4-4639-8D3B-47CFA50AF752")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        decimal EstimatedShipCost { get; set; }

        #region Allors
        [Id("E7BAFA23-3DE0-472B-B1D3-08CF54CD2F6C")]
        #endregion
        [Workspace]
        DateTime EstimatedShipDate { get; set; }

        #region Allors
        [Id("0F39AB42-3546-4AAB-8950-92A1CFE74812")]
        #endregion
        [Workspace]
        DateTime LatestCancelDate { get; set; }

        #region Allors
        [Id("ACB742CA-0A75-4056-B17F-C3A163954757")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        Carrier Carrier { get; set; }

        #region Allors
        [Id("F216F24A-286B-4C64-9673-AFA8D499FD25")]
        #endregion
        [Workspace]
        DateTime EstimatedReadyDate { get; set; }

        #region Allors
        [Id("7147BB41-5937-416E-8F90-A8AFAB2E48FD")]
        #endregion
        [Size(-1)]
        [Workspace]
        string HandlingInstruction { get; set; }

        #region Allors
        [Id("E29E9265-EFC3-41DF-9596-DF5F145549D3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        Store Store { get; set; }

        #region Allors
        [Id("94A73829-633B-4DD0-8388-82E3BE795BBC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace]
        ShipmentPackage[] ShipmentPackages { get; set; }

        #region Allors
        [Id("35AE034A-73D8-4478-8E29-F9A2058CA6FE")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace]
        ShipmentRouteSegment[] ShipmentRouteSegments { get; set; }

        #region Allors
        [Id("7A302883-FC2E-483A-9A60-FFD783453770")]
        #endregion
        [Workspace]
        DateTime EstimatedArrivalDate { get; set; }
    }
}

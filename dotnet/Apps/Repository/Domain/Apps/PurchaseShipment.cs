// <copyright file="PurchaseShipment.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("2bf859c6-de64-476f-a437-5eb57a778262")]
    #endregion
    public partial class PurchaseShipment : Shipment, Versioned
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public Revocation[] TransitionalRevocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public PrintDocument PrintDocument { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public ShipmentState PreviousShipmentState { get; set; }

        public ShipmentState LastShipmentState { get; set; }

        public ShipmentState ShipmentState { get; set; }

        public ShipmentMethod ShipmentMethod { get; set; }

        public ShipmentPackage[] ShipmentPackages { get; set; }

        public string ShipmentNumber { get; set; }

        public Document[] Documents { get; set; }
        public Media[] ElectronicDocuments { get; set; }

        public Person ShipFromContactPerson { get; set; }

        public Facility ShipFromFacility { get; set; }

        public Party ShipToParty { get; set; }

        public ShipmentItem[] ShipmentItems { get; set; }

        public PostalAddress ShipToAddress { get; set; }

        public Person ShipToContactPerson { get; set; }

        public Facility ShipToFacility { get; set; }

        public decimal EstimatedShipCost { get; set; }

        public DateTime EstimatedShipDate { get; set; }

        public DateTime LatestCancelDate { get; set; }

        public Carrier Carrier { get; set; }

        public DateTime EstimatedReadyDate { get; set; }

        public PostalAddress ShipFromAddress { get; set; }

        public string HandlingInstruction { get; set; }

        public Store Store { get; set; }

        public Party ShipFromParty { get; set; }

        public ShipmentRouteSegment[] ShipmentRouteSegments { get; set; }

        public DateTime EstimatedArrivalDate { get; set; }

        public int SortableShipmentNumber { get; set; }

        public Guid DerivationTrigger { get; set; }

        public string SearchString { get; set; }

        #endregion

        #region Versioning
        #region Allors
        [Id("828D99CE-40CC-47C1-95BD-CFF18DF096F3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public PurchaseShipmentVersion CurrentVersion { get; set; }

        #region Allors
        [Id("3F12F6E5-4080-48A7-A7EF-E5F2157DCB0C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public PurchaseShipmentVersion[] AllVersions { get; set; }
        #endregion

        #region inherited methods
        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Invoice() { }

        public void Print() { }

        public void Create() { }

        public void Delete() { }
        #endregion

        #region Allors
        [Id("B1A110DB-2F5F-41B7-A325-B37EC24BDD9E")]
        #endregion
        [Workspace(Default)]
        public void Receive() { }
    }
}

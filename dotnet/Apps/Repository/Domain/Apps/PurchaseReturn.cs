// <copyright file="PurchaseReturn.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("a0cf565a-2dcf-4513-9110-8c34468d993f")]
    #endregion
    public partial class PurchaseReturn : Shipment, Versioned
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
        [Id("76a0fee6-f611-4705-9448-d4b399ea248c")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public PurchaseReturnVersion CurrentVersion { get; set; }

        #region Allors
        [Id("7d8cf556-64da-43cd-9cbe-c4e5a658cb48")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public PurchaseReturnVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("a6178073-2743-49a2-8a1d-59480b12b9d5")]
        #endregion
        [Derived]
        [Required]
        [Workspace(Default)]
        public bool CanShip { get; set; }

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
        [Id("ef13cb5c-96ab-4011-854d-cb52a68d21d7")]
        #endregion
        [Workspace(Default)]
        public void Cancel() { }

        #region Allors
        [Id("7ff94863-ecb2-4866-8632-1d5091cf1052")]
        #endregion
        [Workspace(Default)]
        public void Ship() { }
    }
}

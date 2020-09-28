// <copyright file="CustomerShipmentVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("EB27ECDA-EE0D-4BC5-8FB1-88CF8501D7B0")]
    #endregion
    public partial class CustomerShipmentVersion : ShipmentVersion
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public User LastModifiedBy { get; set; }

        public ShipmentMethod ShipmentMethod { get; set; }

        public ShipmentPackage[] ShipmentPackages { get; set; }

        public string ShipmentNumber { get; set; }

        public Facility ShipToFacility { get; set; }

        public Document[] Documents { get; set; }

        public Facility ShipFromFacility { get; set; }

        public Party ShipToParty { get; set; }

        public ShipmentItem[] ShipmentItems { get; set; }

        public PostalAddress ShipToAddress { get; set; }

        public Person ShipToContactPerson { get; set; }

        public decimal EstimatedShipCost { get; set; }

        public DateTime EstimatedShipDate { get; set; }

        public DateTime LatestCancelDate { get; set; }

        public Carrier Carrier { get; set; }

        public DateTime EstimatedReadyDate { get; set; }

        public PostalAddress ShipFromAddress { get; set; }

        public Person ShipFromContactPerson { get; set; }

        public string HandlingInstruction { get; set; }

        public Store Store { get; set; }

        public Party ShipFromParty { get; set; }

        public ShipmentRouteSegment[] ShipmentRouteSegments { get; set; }

        public DateTime EstimatedArrivalDate { get; set; }

        #endregion

        #region Allors
        [Id("578B9E5B-5ACA-4E0A-9037-80F90A527AE2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public ShipmentState ShipmentState { get; set; }

        #region Allors
        [Id("BC2DC6F6-143E-42DA-BFA8-B65A213D61AB")]
        #endregion
        [Required]
        [Workspace]
        public bool ReleasedManually { get; set; }

        #region Allors
        [Id("C9CF7242-4C5E-4948-94F9-6AF30DE2B78B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public PaymentMethod PaymentMethod { get; set; }

        #region Allors
        [Id("02055EF3-C530-404C-A814-930E325F4763")]
        #endregion
        [Required]
        [Workspace]
        public bool WithoutCharges { get; set; }

        #region Allors
        [Id("61370BA4-FC62-4B1C-A846-1E0DB65E8713")]
        #endregion
        [Required]
        [Workspace]
        public bool HeldManually { get; set; }

        #region Allors
        [Id("96CAFF72-6886-4EA5-A574-1ABA52E8F39A")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal ShipmentValue { get; set; }

        #region inherited methods
        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion
    }
}

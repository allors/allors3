// <copyright file="ShipmentRouteSegment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("8e6eaa35-85da-4c80-848c-3f1ed6cd2f8a")]
    #endregion
    public partial class ShipmentRouteSegment : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("02ef1727-e135-4af3-9d76-02bad7b122f3")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal EndKilometers { get; set; }

        #region Allors
        [Id("2a697cc1-cdeb-4e40-a929-2a8df593877e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Facility FromFacility { get; set; }

        #region Allors
        [Id("3f46506d-ea90-4103-b986-965194037cef")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal StartKilometers { get; set; }

        #region Allors
        [Id("4a30a93c-d50b-44cf-b0a2-c29c970e6290")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public ShipmentMethod ShipmentMethod { get; set; }

        #region Allors
        [Id("57f25750-a517-47a8-a6a0-feb160cd5f3e")]
        #endregion

        public DateTime EstimatedStartDateTime { get; set; }

        #region Allors
        [Id("591427f6-b61c-4c19-9f82-e97570d9bead")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Facility ToFacility { get; set; }

        #region Allors
        [Id("6b3d4c25-823c-4197-8c05-80aeb887eb8b")]
        #endregion

        public DateTime EstimatedArrivalDateTime { get; set; }

        #region Allors
        [Id("6bf54f85-7781-4fd3-a87f-6e7103042ecb")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Vehicle Vehicle { get; set; }

        #region Allors
        [Id("928b9d1e-903b-4d56-aa72-b7aeaf3ba340")]
        #endregion

        public DateTime ActualArrivalDateTime { get; set; }

        #region Allors
        [Id("b080fe6b-382e-475d-be81-8632ddedb183")]
        #endregion

        public DateTime ActualStartDateTime { get; set; }

        #region Allors
        [Id("c04769b1-f8dc-40c7-87d2-1e55a4702e71")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Organisation Carrier { get; set; }

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

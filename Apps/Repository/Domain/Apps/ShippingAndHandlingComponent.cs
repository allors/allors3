// <copyright file="ShippingAndHandlingComponent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("1a174f59-c8cd-49ad-b0f4-a561cdcdcfb2")]
    #endregion
    public partial class ShippingAndHandlingComponent : Period, Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        #endregion

        #region Allors
        [Id("0021e1ff-bfc3-4d0b-8168-a8f5789121f7")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal Cost { get; set; }

        #region Allors
        [Id("4dfb4bda-1add-45d5-92c7-6393186301f0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public ShipmentMethod ShipmentMethod { get; set; }

        #region Allors
        [Id("a029fb4c-4f80-4216-8fc9-9d9b44997816")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Carrier Carrier { get; set; }

        #region Allors
        [Id("ab4377d4-69c6-4b0c-b9d4-e3a01c1a6a94")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public ShipmentValue ShipmentValue { get; set; }

        #region Allors
        [Id("df4727ab-29a8-448c-97b4-c16033e03dcf")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Currency Currency { get; set; }

        #region Allors
        [Id("f2bfd9d5-01b2-4bec-8dc2-018cc2187037")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public GeographicBoundary GeographicBoundary { get; set; }

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

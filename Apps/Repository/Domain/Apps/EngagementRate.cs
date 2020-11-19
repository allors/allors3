// <copyright file="EngagementRate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("6b666a30-7a55-4986-8411-b6179768e70b")]
    #endregion
    public partial class EngagementRate : Period, Object
    {
        #region inherited properties
        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("0c2c005b-f652-47b2-a42b-7cd511382dd3")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal BillingRate { get; set; }

        #region Allors
        [Id("1df6f7fe-6cb9-4c1b-b664-e7ee1e2cec6f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public RateType RateType { get; set; }

        #region Allors
        [Id("a920a2c5-021e-4fc9-b38b-21be0003e40f")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal Cost { get; set; }

        #region Allors
        [Id("c54c15ad-0b9b-490c-bdbb-90a49c728b94")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        public PriceComponent[] GoverningPriceComponents { get; set; }

        #region Allors
        [Id("d030a71e-10ba-48cc-9964-456518b705de")]
        #endregion
        [Size(-1)]
        public string ChangeReason { get; set; }

        #region Allors
        [Id("e7dafa85-712a-4ea4-abe9-82ddd9afc80c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public UnitOfMeasure UnitOfMeasure { get; set; }

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

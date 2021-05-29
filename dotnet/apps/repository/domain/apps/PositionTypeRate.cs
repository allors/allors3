// <copyright file="PositionTypeRate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("17b9c8f1-ddf2-4db0-8358-ae66a02395ce")]
    #endregion
    public partial class PositionTypeRate : Period, Object
    {
        #region inherited properties

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("ab942018-51fd-4135-9005-c81443b72a96")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal Rate { get; set; }

        #region Allors
        [Id("c49a44b8-dff1-471c-8309-cf9c7e9188c2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public RateType RateType { get; set; }

        #region Allors
        [Id("CA9D5A86-F155-4EB8-A8C0-D600F9EA0919")]
        [Indexed]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal Cost { get; set; }

        #region Allors
        [Id("f49e4e9e-2e8f-49f6-9c10-4aefb4bb61bf")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public TimeFrequency Frequency { get; set; }

        #region inherited methods
        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}

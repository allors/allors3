// <copyright file="PartyRate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("2E6F43A2-3D1D-401B-86BF-7DE63FC9FF3E")]
    #endregion
    public partial class PartyRate : Period, Object
    {
        #region inherited properties
        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("6906DD20-C4B4-4516-B09F-9F93E2849F19")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal Rate { get; set; }

        #region Allors
        [Id("C99C4330-5578-4B8D-872A-C72C43DA42FA")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public RateType RateType { get; set; }

        #region Allors
        [Id("AE04E207-CCEC-492C-8C66-1AB5A1A4BFA7")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal Cost { get; set; }

        #region Allors
        [Id("5563FC19-D342-4254-A4A7-9CBD41B43868")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public PriceComponent[] GoverningPriceComponents { get; set; }

        #region Allors
        [Id("6943FC0E-9A7C-4024-90A9-D0848C148AEF")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string ChangeReason { get; set; }

        #region Allors
        [Id("21C7B4A0-F33B-401F-B136-AFA7B226EACE")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
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

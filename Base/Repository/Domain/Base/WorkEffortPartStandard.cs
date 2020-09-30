// <copyright file="WorkEffortPartStandard.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("a12e5d28-e431-48d3-bbb1-8a2f5e3c4991")]
    #endregion
    public partial class WorkEffortPartStandard : Period, Deletable
    {
        #region inherited properties
        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("4d4913e2-649d-4589-86ee-93cfa6c426a7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Part Part { get; set; }

        #region Allors
        [Id("68d4af49-a55f-416c-8097-d93da90e1132")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal EstimatedCost { get; set; }

        #region Allors
        [Id("ec3e9aee-c39b-46a1-9968-af914f9057f3")]
        #endregion
        [Workspace(Default)]
        public int Quantity { get; set; }

        #region Allors
        [Id("8fc60462-287d-47df-a6d2-6ac857f2afbb")]
        #endregion
        [Derived]
        [Size(-1)]
        [Workspace(Default)]
        public string PartDisplayName { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}

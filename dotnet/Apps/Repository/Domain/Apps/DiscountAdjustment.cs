// <copyright file="DiscountAdjustment.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("0346a1e2-03c7-4f1e-94ae-35fdf64143a9")]
    #endregion
    public partial class DiscountAdjustment : OrderAdjustment
    {
        #region inherited properties
        public decimal Amount { get; set; }

        public decimal Percentage { get; set; }

        public string Description { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        #endregion

        #region Versioning
        #region Allors
        [Id("69911C68-D349-4C37-8819-08700AE61681")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public DiscountAdjustmentVersion CurrentVersion { get; set; }

        #region Allors
        [Id("D5901ABB-AFBC-4983-82AD-C143BE40B05F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public DiscountAdjustmentVersion[] AllVersions { get; set; }
        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete()
        {
        }

        #endregion
    }
}

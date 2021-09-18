// <copyright file="OrderLine.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region
    [Id("721008C3-C87C-40AB-966B-094E1271ED5F")]
    #endregion
    public partial class OrderLine : Versioned, Object
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("7022167A-046E-45B3-A14E-AE0290C0F1D6")]
        #endregion
        public decimal Amount { get; set; }

        #region Versioning
        #region Allors
        [Id("55F3D531-C58D-4FA7-B745-9E38D8CEC4C6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public OrderLineVersion CurrentVersion { get; set; }

        #region Allors
        [Id("CFC88B59-87A1-4F9E-ABBE-168694AB6CB5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public OrderLineVersion[] AllVersions { get; set; }
        #endregion

        #region inherited methods

        public void OnBuild()
        {
        }

        public void OnPostBuild()
        {
        }

        public void OnInit()
        {
        }

        public void OnPostDerive()
        {
        }

        #endregion
    }
}

// <copyright file="WorkRequirementFulfillment.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("41276954-bb1e-4331-bdb1-494deeec1424")]
    #endregion
    public partial class WorkRequirementFulfillment : Deletable
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("9ce03908-7123-4d31-b638-377f9ee3fe34")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public WorkEffort FullfillmentOf { get; set; }

        #region Allors
        [Id("23a6ec3d-0f51-421b-a158-cdf2a18362a5")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string WorkEffortNumber { get; set; }

        #region Allors
        [Id("13bd1c8f-365a-4b7e-b363-0f8c83192723")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string WorkEffortName { get; set; }

        #region Allors
        [Id("5acce8f1-93be-4866-b668-1dfccfa73c49")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public WorkRequirement FullfilledBy{ get; set; }

        #region Allors
        [Id("45d55984-279d-4d26-b961-e5a3fe68d44a")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string WorkRequirementNumber { get; set; }

        #region Allors
        [Id("3d43d912-f068-41a5-a01d-accdb1d5b2ee")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string WorkRequirementDescription { get; set; }

        #region Allors
        [Id("18987460-bf33-4571-9084-d3c55ea36bec")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public FixedAsset FixedAsset { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}

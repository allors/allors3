// <copyright file="WorkEffortFixedAssetAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("3b43da7f-5252-4824-85fe-c85d6864838a")]
    #endregion
    public partial class WorkEffortFixedAssetAssignment : Commentable, Period, Deletable, DelegatedAccessControlledObject
    {
        #region inherited properties
        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        #endregion

        #region Allors
        [Id("2b6eca80-294c-4a2d-a15c-a57c0c815aa1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public AssetAssignmentStatus AssetAssignmentStatus { get; set; }

        #region Allors
        [Id("2d7dd4b3-a0bd-45aa-9d1a-a0ffa4a98061")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public WorkEffort Assignment { get; set; }

        #region Allors
        [Id("a2816fd1-babb-480c-8e29-0f7192aaff71")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal AllocatedCost { get; set; }

        #region Allors
        [Id("e90cb555-e6d9-4d7d-8d98-6f9c28c4bc14")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public FixedAsset FixedAsset { get; set; }

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

        public void DelegateAccess() { }

        #endregion
    }
}

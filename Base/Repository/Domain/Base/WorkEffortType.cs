// <copyright file="WorkEffortType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("7d2d9452-f250-47c3-81e0-4e1c0655cc86")]
    #endregion
    public partial class WorkEffortType : Object, Deletable
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("1ecbdb5f-5fb7-492d-9e08-df3aa322371d")]
        #endregion
        [Required]
        [Workspace]
        public string Name { get; set; }

        #region Allors
        [Id("93cfed3d-ae24-4a07-becf-34cdc3cdef3e")]
        #endregion
        [Required]
        [Workspace]
        [Size(-1)]
        public string Description { get; set; }

        #region Allors
        [Id("5ce1a600-62a9-4d2c-bfb5-bfe374b2099f")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        public WorkEffortFixedAssetStandard[] WorkEffortFixedAssetStandards { get; set; }

        #region Allors
        [Id("d51d620e-250e-4492-8926-c8535fad19ec")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        public WorkEffortPartStandard[] WorkEffortPartStandards { get; set; }

        #region Allors
        [Id("df104ec4-6247-4199-bce1-635978fa8ad4")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        public WorkEffortSkillStandard[] WorkEffortSkillStandards { get; set; }

        #region Allors
        [Id("89eef4e3-eda7-4336-91cb-ce7a7e96521f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace]
        public WorkEffortType[] Children { get; set; }

        #region Allors
        [Id("8d9f51b5-2c8d-4a25-a45e-c79542a09434")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public FixedAsset FixedAssetToRepair { get; set; }

        #region Allors
        [Id("b6d68eff-8a3a-473f-bb4e-9bc46808bde0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace]
        public WorkEffortType[] Dependencies { get; set; }

        #region Allors
        [Id("df1fa89e-25e2-4b72-a928-67fa2c95ad70")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal StandardWorkHours { get; set; }

        #region Allors
        [Id("ee521062-a2bf-4a7f-80e4-8da6f63439fe")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Product ProductToProduce { get; set; }

        #region Allors
        [Id("f8766ab1-b0ed-42fa-806c-c40a2e68d72a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Deliverable DeliverableToProduce { get; set; }

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

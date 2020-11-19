// <copyright file="BudgetRevisionImpact.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("ebae3ca2-5dca-486d-bbc0-30550313f153")]
    #endregion
    public partial class BudgetRevisionImpact : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("16b0c91f-5746-4ebe-a071-7c42887cccb1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public BudgetItem BudgetItem { get; set; }

        #region Allors
        [Id("55e9b1e3-0545-471e-97b0-07d8968629c2")]
        #endregion
        [Required]
        [Size(-1)]

        public string Reason { get; set; }

        #region Allors
        [Id("6b3a80c1-eff1-478c-a54e-4912bc4a1242")]
        #endregion

        public bool Deleted { get; set; }

        #region Allors
        [Id("7d0ad499-1e3d-41cd-bc6c-79aac1a7fa57")]
        #endregion

        public bool Added { get; set; }

        #region Allors
        [Id("80106b6d-8e1d-4db1-a4eb-71a56e9a4c94")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal RevisedAmount { get; set; }

        #region Allors
        [Id("b93df76d-439a-45cf-885d-4887afe5fd6f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public BudgetRevision BudgetRevision { get; set; }

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

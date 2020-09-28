// <copyright file="WorkEffortGoodStandard.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("81ddff76-9b82-4309-9c9f-f7f9dbd2db21")]
    #endregion
    public partial class WorkEffortGoodStandard : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("086907b1-97c2-47c1-ade4-f7749f615ae1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public UnifiedProduct UnifiedProduct { get; set; }

        #region Allors
        [Id("28b3b976-3354-4095-b928-7c1474e8c492")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal EstimatedCost { get; set; }

        #region Allors
        [Id("c94d5e97-ec2b-4d32-ae8d-145595f0ad91")]
        #endregion

        public int EstimatedQuantity { get; set; }

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

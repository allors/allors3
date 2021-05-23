// <copyright file="WorkEffortFixedAssetStandard.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("9b9f2a59-ae10-49df-b0b5-98b48ec99157")]
    #endregion
    public partial class WorkEffortFixedAssetStandard : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("5aca8d2b-0073-4890-b02a-f4c9a5fc8a2b")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal EstimatedCost { get; set; }

        #region Allors
        [Id("73900f38-242a-4aac-ba8e-d8ffa57a125f")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal EstimatedDuration { get; set; }

        #region Allors
        [Id("98ca7e1a-8f15-4533-9de7-819b6c868788")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public FixedAsset FixedAsset { get; set; }

        #region Allors
        [Id("b9d782af-1f4c-4639-bd11-fda3651982df")]
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

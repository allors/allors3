// <copyright file="Left.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region

    [Id("DBAF849D-E8B0-4CEA-85E0-DFB934A06F96")]

    #endregion
    public partial class Left : DerivationCounted
    {
        #region inherited properties
        public int DerivationCount { get; set; }

        #endregion

        #region Allors
        [Id("C86BBB90-F678-4627-B651-657F86B2D2EB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        public Middle Middle { get; set; }

        #region Allors
        [Id("92CF5496-063D-428E-9A24-F36321A10675")]
        #endregion
        [Required]
        public int Counter { get; set; }

        #region Allors
        [Id("8C454674-AE11-4305-A055-55A915139F16")]
        #endregion
        [Required]
        public bool CreateMiddle { get; set; }

        #region inherited methods

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

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

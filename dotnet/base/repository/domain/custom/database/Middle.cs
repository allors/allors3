// <copyright file="Middle.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region

    [Id("62173428-589F-43FA-8FA5-5579F084B8E3")]

    #endregion
    public partial class Middle : DerivationCounted
    {
        #region inherited properties
        public int DerivationCount { get; set; }

        #endregion

        #region Allors
        [Id("27D0ABFD-EBA0-46FE-812C-C67D8E3D12D0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        public Right Right { get; set; }

        #region Allors
        [Id("4616201B-7C52-4C5D-B390-4D9C0A8CADAD")]
        #endregion
        [Required]
        public int Counter { get; set; }

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

// <copyright file="Citizenship.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("38b0ac1b-497c-4286-976e-64b3d523ad9d")]
    #endregion
    public partial class Citizenship : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("45d0dd4b-6d8c-4727-b38b-f7ed850023c1")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]

        public Passport[] Passports { get; set; }

        #region Allors
        [Id("ca2b2d3e-ba3c-4e92-a86f-92d5d47b8e01")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Country Country { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion

    }
}

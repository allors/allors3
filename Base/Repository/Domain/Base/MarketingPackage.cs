// <copyright file="MarketingPackage.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("42adee8e-5994-42e3-afe1-aa3d3089d594")]
    #endregion
    public partial class MarketingPackage : ProductAssociation
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
        [Id("29cb7841-1793-43c3-bcbe-3d69a8e651b5")]
        #endregion
        [Size(-1)]

        public string Instruction { get; set; }

        #region Allors
        [Id("70c7d06c-2086-4a60-b2b9-aba2c6f07669")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]

        public Product[] ProductsUsedIn { get; set; }

        #region Allors
        [Id("a687e8ff-624c-4794-866f-f4cc653d874c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Product Product { get; set; }

        #region Allors
        [Id("ccabc13b-63cc-4cdf-909d-411edc26d648")]
        #endregion
        [Required]
        [Size(-1)]

        public string Description { get; set; }

        #region Allors
        [Id("dc3c4217-5c42-4ac3-ad16-33f50653bcfc")]
        #endregion

        public int QuantityUsed { get; set; }

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

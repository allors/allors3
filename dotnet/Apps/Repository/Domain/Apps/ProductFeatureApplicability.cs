// <copyright file="ProductFeatureApplicability.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("003433eb-a0c6-454d-8517-0c03e9be3e96")]
    #endregion
    public partial class ProductFeatureApplicability : Period, Deletable
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        #endregion

        #region Allors
        [Id("3198ade4-8080-4584-9b67-b00af681c5cf")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Product AvailableFor { get; set; }

        #region Allors
        [Id("c17d3bde-ebbc-463c-b9cb-b0a5a700c6a1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public ProductFeature ProductFeature { get; set; }

        #region Allors
        [Id("A1AE46BD-FB2B-4454-8A4B-9D4C7025A577")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public ProductFeatureApplicabilityKind ProductFeatureApplicabilityKind { get; set; }

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

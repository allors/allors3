// <copyright file="PartyFixedAssetAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("40ee178e-7564-4dfa-ab6f-8bcd4e62b498")]
    #endregion
    public partial class PartyFixedAssetAssignment : Period, Commentable, Deletable
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        #endregion

        #region Allors
        [Id("28afdc0d-ebc7-4f53-b5a1-0cc0eb377887")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public FixedAsset FixedAsset { get; set; }

        #region Allors
        [Id("59187015-4689-4ef8-942f-c36ff4c74e64")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Party Party { get; set; }

        #region Allors
        [Id("70c38a47-79c4-4ec8-abfd-3c40ef4239ea")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public AssetAssignmentStatus AssetAssignmentStatus { get; set; }

        #region Allors
        [Id("c70f014b-345b-48ad-8075-2a1835a19f57")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal AllocatedCost { get; set; }

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

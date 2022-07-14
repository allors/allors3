// <copyright file="GeneralLedgerAccountClassification.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("4a600c96-b813-46fc-8674-06bd3f85eae4")]
    #endregion
    public partial class GeneralLedgerAccountClassification : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("186dff79-0769-443c-bc4d-6f60c317b5c8")]
        #endregion
        [Workspace(Default)]
        public int RgsLevel { get; set; }

        #region Allors
        [Id("250619a1-fd18-4103-9817-346e25ac7f03")]
        #endregion
        [Workspace(Default)]
        public string ReferenceCode { get; set; }

        #region Allors
        [Id("f2d02d7a-e536-49e9-8fa0-2f4e644a8ad9")]
        #endregion
        [Workspace(Default)]
        public string SortCode { get; set; }

        #region Allors
        [Id("e43a6308-cb95-474e-aacb-038855fcccfe")]
        #endregion
        [Workspace(Default)]
        public string Code { get; set; }

        #region Allors
        [Id("3ab2ad60-3560-4817-9862-7f60c55bbc32")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccountClassification Parent { get; set; }

        #region Allors
        [Id("a48c3601-3d4c-43af-9502-d6beda764118")]
        #endregion
        [Required]
        [Workspace(Default)]
        public string Name { get; set; }

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

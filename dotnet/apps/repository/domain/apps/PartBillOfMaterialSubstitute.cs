// <copyright file="PartBillOfMaterialSubstitute.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("5906f4cd-3950-43ee-a3ba-84124c4180f6")]
    #endregion
    public partial class PartBillOfMaterialSubstitute : Period, Commentable, Object
    {
        #region inherited properties
        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        #endregion

        #region Allors
        [Id("3d84d60f-c8b7-4e33-847a-9720d6570dd1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public PartBillOfMaterial SubstitutionPartBillOfMaterial { get; set; }

        #region Allors
        [Id("9bff7f7d-c35c-426d-95f3-6a681d283914")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Ordinal Preference { get; set; }

        #region Allors
        [Id("a5273118-61c9-43de-9754-22555332cc27")]
        #endregion

        public int Quantity { get; set; }

        #region Allors
        [Id("ef45301b-415a-417f-a952-fd71704a05e5")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public PartBillOfMaterial PartBillOfMaterial { get; set; }

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

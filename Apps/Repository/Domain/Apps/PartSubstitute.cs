// <copyright file="PartSubstitute.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("c0ea51d6-e9f1-4cb3-80ea-36d8ac4f8a15")]
    #endregion
    public partial class PartSubstitute : Commentable, Deletable
    {
        #region inherited properties
        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("23f8fda9-9109-4826-988f-74e115a430f4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Part SubstitutionPart { get; set; }

        #region Allors
        [Id("510f8f4c-ff09-4d32-8c1c-e905dbbd684b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Ordinal Preference { get; set; }

        #region Allors
        [Id("9cd198eb-2c25-425e-a23b-c321938f2512")]
        #endregion

        public DateTime FromDate { get; set; }

        #region Allors
        [Id("ccb0a290-b3f4-4e55-b52c-67ca70d67439")]
        #endregion
        [Required]

        public int Quantity { get; set; }

        #region Allors
        [Id("e7d4ae25-175a-4e2a-88c2-9d8d5a468d1a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Part Part { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion

    }
}

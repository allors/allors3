// <copyright file="PerformanceReviewItem.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("962e5149-546b-4b18-ab09-e4de59b709ff")]
    #endregion
    public partial class PerformanceReviewItem : Commentable, DelegatedAccessObject
    {
        #region inherited properties
        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Object DelegatedAccess { get; set; }

        #endregion

        #region Allors
        [Id("6d7bb4b2-885d-4f7b-9d31-d517c3d03ac2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public RatingType RatingType { get; set; }

        #region Allors
        [Id("d62d7236-458f-4e30-8df4-27eb877d0931")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public PerformanceReviewItemType PerformanceReviewItemType { get; set; }

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

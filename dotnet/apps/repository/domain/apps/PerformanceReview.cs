// <copyright file="PerformanceReview.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("89c49578-bb5d-4589-b908-bf09c6495011")]
    #endregion
    public partial class PerformanceReview : Commentable, Period, Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        #endregion

        #region Allors
        [Id("22ec2f64-1099-49aa-908b-abb2703ccf33")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Person Manager { get; set; }

        #region Allors
        [Id("3704d6ac-52c1-4af0-ad6e-151defc2fa05")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public PayHistory PayHistory { get; set; }

        #region Allors
        [Id("a16503ae-6371-4e97-9d34-f21a0f52002f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public PayCheck BonusPayCheck { get; set; }

        #region Allors
        [Id("a5057413-950e-4825-8036-7f398c4b5d39")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]

        public PerformanceReviewItem[] PerformanceReviewItems { get; set; }

        #region Allors
        [Id("ddeb9c39-9bfc-437d-8f5a-434028d8ad6f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Person Employee { get; set; }

        #region Allors
        [Id("f3210e4a-a8ee-442c-85a5-34290deffe2a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Position ResultingPosition { get; set; }

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

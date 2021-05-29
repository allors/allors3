// <copyright file="ProductDeliverySkillRequirement.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("fd342cb7-53d3-4377-acd8-ee586b924678")]
    #endregion
    public partial class ProductDeliverySkillRequirement : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("12c6abaf-a080-45f3-820d-b462978d2539")]
        #endregion

        public DateTime StartedUsingDate { get; set; }

        #region Allors
        [Id("5a52b67e-23e4-45ac-a1d4-cb083bf897cc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Service Service { get; set; }

        #region Allors
        [Id("6d4ec793-41a7-4044-9744-42d1bd44bbd4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Skill Skill { get; set; }

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

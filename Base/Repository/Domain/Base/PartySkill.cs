// <copyright file="PartySkill.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("1d157965-59b5-4ead-b4e4-c722495d7658")]
    #endregion
    public partial class PartySkill : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("3254f43d-7b3a-49c8-8c1b-19fa0e4f6901")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal YearsExperience { get; set; }

        #region Allors
        [Id("7ed819c8-78ef-4fe3-b499-b381c246711f")]
        #endregion

        public DateTime StartedUsingDate { get; set; }

        #region Allors
        [Id("a341478c-503c-49ee-8c9a-e85b777e9ff4")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]

        public SkillRating SkillRating { get; set; }

        #region Allors
        [Id("eb3e02dc-6ee5-4aca-9f35-68edafed6dd2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public SkillLevel SkillLevel { get; set; }

        #region Allors
        [Id("fec11de5-a33c-4dd7-9af9-b32c3889c8a3")]
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

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion

    }
}

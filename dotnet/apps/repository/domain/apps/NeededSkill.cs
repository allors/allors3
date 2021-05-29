// <copyright file="NeededSkill.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("5e31a968-5f7d-4109-9821-b94459f13382")]
    #endregion
    public partial class NeededSkill : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("079ef934-26e1-4dba-a69a-73fcc22d380e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public SkillLevel SkillLevel { get; set; }

        #region Allors
        [Id("21207c09-22b0-469a-84a7-6edd300c73f7")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal YearsExperience { get; set; }

        #region Allors
        [Id("590d749a-52d4-448a-8f95-8412c0115825")]
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

// <copyright file="WorkEffortSkillStandard.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("da16f5ee-0e04-41a7-afd7-b12e20414135")]
    #endregion
    public partial class WorkEffortSkillStandard : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("13a68eeb-7ca1-4ecd-a82b-ecbd75da99b6")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Skill Skill { get; set; }

        #region Allors
        [Id("20623472-f4f3-40fc-bd7c-cd3da44fe224")]
        #endregion

        public int EstimatedNumberOfPeople { get; set; }

        #region Allors
        [Id("e05c673f-6c4b-492d-bf68-b4af15310aea")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal EstimatedDuration { get; set; }

        #region Allors
        [Id("ed6a55d4-def6-49e0-8b1a-9ee99d8b3c3d")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal EstimatedCost { get; set; }

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

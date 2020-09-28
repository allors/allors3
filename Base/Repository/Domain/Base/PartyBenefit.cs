// <copyright file="PartyBenefit.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("d520cf1a-8d3a-4380-8b88-85cd63a5ad05")]
    #endregion
    public partial class PartyBenefit : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("15638ba3-73c7-4c32-aaa7-a91d4a5e9951")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public TimeFrequency Frequency { get; set; }

        #region Allors
        [Id("1c4a69e7-62c7-4e6b-b7a5-69817d1788df")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal Cost { get; set; }

        #region Allors
        [Id("320e98c9-adff-41cf-894a-500730cf6c09")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal ActualEmployerPaidPercentage { get; set; }

        #region Allors
        [Id("9a8fcada-bf2c-450d-a941-e0c7ec414cf3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Benefit Benefit { get; set; }

        #region Allors
        [Id("e4bd1497-824b-477a-9842-a87b4193b430")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal ActualAvailableTime { get; set; }

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

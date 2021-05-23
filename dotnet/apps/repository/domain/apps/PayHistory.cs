// <copyright file="PayHistory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("208a5af6-8dd8-4a48-acb2-2ecb89e8d322")]
    #endregion
    public partial class PayHistory : Period, Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        #endregion

        #region Allors
        [Id("6d26369b-eea2-4712-a7d1-56884a3cc715")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public TimeFrequency Frequency { get; set; }

        #region Allors
        [Id("b3f1071f-7e71-4ef1-aa9b-545ad694f44c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public SalaryStep SalaryStep { get; set; }

        #region Allors
        [Id("b7ef1bf8-b16b-400e-903e-d0a7454572a0")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal Amount { get; set; }

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

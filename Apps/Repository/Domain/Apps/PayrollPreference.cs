// <copyright file="PayrollPreference.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("92f48c0c-31d9-4ed5-8f92-753de6af471a")]
    #endregion
    public partial class PayrollPreference : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("2cb969f7-6415-4d5b-be55-7e691c2254e1")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal Percentage { get; set; }

        #region Allors
        [Id("802de3ea-0cb9-4815-bc56-497e75f487ae")]
        #endregion
        [Size(256)]

        public string AccountNumber { get; set; }

        #region Allors
        [Id("a37e2763-6d8c-46c3-a69f-148458d2981b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public PaymentMethod PaymentMethod { get; set; }

        #region Allors
        [Id("b576883f-0cfd-4973-aa49-479b6e712c75")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public TimeFrequency Frequency { get; set; }

        #region Allors
        [Id("c71eb13a-8053-4d56-a3e3-dcd38a1e4f29")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public DeductionType DeductionType { get; set; }

        #region Allors
        [Id("ded05ab7-351b-4b05-9e0a-010e6b4fbd0f")]
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

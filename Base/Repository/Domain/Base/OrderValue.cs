// <copyright file="OrderValue.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("a3ca36e6-960d-4e3a-96d0-6ca1d71d05d7")]
    #endregion
    public partial class OrderValue : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("077a33bc-a822-4a23-918c-7fcaacdc61d1")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal ThroughAmount { get; set; }

        #region Allors
        [Id("b25816e8-4b0c-4857-907f-7a391df2c55e")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal FromAmount { get; set; }

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

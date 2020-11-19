// <copyright file="ShipmentValue.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("20ef8456-83f2-4722-b8a8-1d8ab3129843")]
    #endregion
    public partial class ShipmentValue : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("a87bb4e3-ca1c-4887-9305-19febfc531fd")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal ThroughAmount { get; set; }

        #region Allors
        [Id("d637b8ab-c6ac-4855-81db-f0a1f1584219")]
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

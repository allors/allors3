// <copyright file="PostalCode.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("9d0065b8-2760-4ec5-928a-9ebd128bbfdd")]
    #endregion
    public partial class PostalCode : CountryBound, GeographicBoundary, Object
    {
        #region inherited properties
        public Country Country { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Abbreviation { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("20267bfe-b651-4ed7-bd22-f4300022e39c")]
        #endregion
        [Required]
        [Size(256)]

        public string Code { get; set; }

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

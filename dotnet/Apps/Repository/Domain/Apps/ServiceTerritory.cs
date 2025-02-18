// <copyright file="ServiceTerritory.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("987f8328-2bfa-47cd-9521-8b7bda78f90a")]
    #endregion
    public partial class ServiceTerritory : GeographicBoundaryComposite, Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public GeographicBoundary[] Associations { get; set; }

        public string Abbreviation { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("a268313d-db1e-44e1-9fb1-7135d1157083")]
        #endregion
        [Required]
        [Size(256)]

        public string Name { get; set; }

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

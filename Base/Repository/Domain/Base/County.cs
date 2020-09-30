// <copyright file="County.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("e6f97f86-6aec-4dde-b828-4de04d42c248")]
    #endregion
    public partial class County : GeographicBoundary, CityBound, Object
    {
        #region inherited properties
        public string Abbreviation { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public City[] Cities { get; set; }

        #endregion

        #region Allors
        [Id("89a67d5c-8f78-41aa-9152-91f8496535bc")]
        #endregion
        [Required]
        [Size(256)]

        public string Name { get; set; }

        #region Allors
        [Id("926ce4e6-cc76-4005-964f-f4d5af5fe944")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public State State { get; set; }

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

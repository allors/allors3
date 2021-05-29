// <copyright file="StringTemplate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("0c50c02a-cc9c-4617-8530-15a24d4ac969")]
    #endregion
    public partial class StringTemplate : UniquelyIdentifiable, Localised
    {
        #region inherited properties
        public Guid UniqueId { get; set; }

        public Locale Locale { get; set; }

        #endregion

        #region Allors
        [Id("2f88f9f8-3c22-40d3-885c-2abd43af96cc")]
        #endregion
        [Size(-1)]

        public string Body { get; set; }

        #region Allors
        [Id("c501103b-037a-4961-93df-2dbb74b88a76")]
        #endregion
        [Required]
        [Size(256)]

        public string Name { get; set; }

        #region inherited methods

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion

    }
}

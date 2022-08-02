// <copyright file="Role.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("af6fe5f4-e5bc-4099-bcd1-97528af6505d")]
    #endregion
    public partial class Role : UniquelyIdentifiable
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("51e56ae1-72dc-443f-a2a3-f5aa3650f8d2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public Permission[] Permissions { get; set; }

        #region Allors
        [Id("934bcbbe-5286-445c-a1bd-e2fcc786c448")]
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

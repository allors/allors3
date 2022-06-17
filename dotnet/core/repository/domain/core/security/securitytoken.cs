// <copyright file="SecurityToken.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("a53f1aed-0e3f-4c3c-9600-dc579cccf893")]
    #endregion
    public partial class SecurityToken : UniquelyIdentifiable, Deletable
    {
        #region inherited properties
        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("6503574b-8bab-4da8-a19d-23a9bcffe01e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        public Grant[] Grants { get; set; }

        #region Allors
        [Id("E094E1DD-A3B0-4B6A-B2FA-B00E98BDC0D6")]
        #endregion
        [Size(256)]
        public Guid SecurityStamp { get; set; }

        #region inherited methods

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}

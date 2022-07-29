// <copyright file="Currency.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("fd397adf-40b4-4ef8-b449-dd5a24273df3")]
    #endregion
    [Plural("Currencies")]
    public partial class Currency : Enumeration, Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public string Name { get; set; }

        public LocalisedText[] LocalisedNames { get; set; }

        public bool IsActive { get; set; }

        #endregion

        #region Allors
        [Id("294a4bdc-f03a-47a2-a649-419e6b9021a3")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string IsoCode { get; set; }

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

// <copyright file="TemplateType.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("BDABB545-3B39-4F91-9D01-A589A5DA670E")]
    #endregion
    public partial class TemplateType : Enumeration, Deletable
    {
        #region inherited properties
        public Guid UniqueId { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Name { get; set; }

        public LocalisedText[] LocalisedNames { get; set; }

        public bool IsActive { get; set; }

        #endregion

        #region inherited methods
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

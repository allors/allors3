// <copyright file="Template.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("93F8B97B-2D9A-42FC-A823-7BDCC5A92203")]
    #endregion
    public partial class Template : UniquelyIdentifiable, Deletable, Object
    {
        #region inherited properties
        public Guid UniqueId { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("64DD490F-2B13-4D63-94A4-6BCE96FA14C2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        public TemplateType TemplateType { get; set; }

        #region Allors
        [Id("93C9C5F2-7D0B-475D-80B8-7CAC7B11CCDA")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Required]
        public Media Media { get; set; }

        #region Allors
        [Id("3BC9EEAE-717F-4030-88ED-68057B14ACEC")]
        [Indexed]
        #endregion
        [Required]
        public string Arguments { get; set; }

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

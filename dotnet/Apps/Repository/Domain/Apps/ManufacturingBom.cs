// <copyright file="ManufacturingBom.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("68a0c645-4671-4dda-87a5-53395934a9fc")]
    #endregion
    public partial class ManufacturingBom : PartBillOfMaterial
    {
        #region inherited properties
        public Part Part { get; set; }

        public string Instruction { get; set; }

        public int QuantityUsed { get; set; }

        public Part ComponentPart { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        #endregion

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

// <copyright file="PartRevision.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("22f87630-11dd-480e-a721-9836af7685b1")]
    #endregion
    public partial class PartRevision : Period, Object
    {
        #region inherited properties
        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("6d1b4cec-abff-46db-a446-0f8889426b28")]
        #endregion
        [Size(-1)]

        public string Reason { get; set; }

        #region Allors
        [Id("84561abd-08bc-4d28-b25c-22787d8bd7f0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Part SupersededByPart { get; set; }

        #region Allors
        [Id("8a064340-def3-4d9f-89d6-3325b8a41f4d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Part Part { get; set; }

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

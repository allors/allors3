// <copyright file="PartyRelationship.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("CE633852-D115-468E-A52F-22A777E27198")]
    #endregion
    [Relationship]
    [Workspace(Default)]
    public partial class Employment : Period, Deletable
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ThroughDate { get; set; }

        #endregion

        #region Allors
        [Id("93B8F2E1-9902-4C0B-BFAC-74629C494346")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public Person Employee { get; set; }

        #region Allors
        [Id("75FCF8E7-1DB4-466B-9428-5A9E45467D15")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public Organisation Employer { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        public void Delete() { }
        #endregion
    }
}

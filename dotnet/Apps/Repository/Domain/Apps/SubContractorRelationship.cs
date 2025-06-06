// <copyright file="SubContractorRelationship.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("d60cc44a-6491-4982-9b2d-99891e382a21")]
    #endregion
    public partial class SubContractorRelationship : PartyRelationship
    {
        #region inherited properties

        public Party[] Parties { get; set; }

        public string DisplayPartyRelationship { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public Agreement[] Agreements { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("567a8c58-2584-4dc7-96c8-13fea5b51cf9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public InternalOrganisation Contractor { get; set; }

        #region Allors
        [Id("d95ecb34-dfe4-42df-bc9f-1ad4af72abaa")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Organisation SubContractor { get; set; }

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

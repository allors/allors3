// <copyright file="SupplierRelationship.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("2b162153-f74d-4f97-b97c-48f04749b216")]
    #endregion
    public partial class SupplierRelationship : PartyRelationship, Period, Deletable, Object
    {
        #region inherited properties

        public Party[] Parties { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public Agreement[] Agreements { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("1546c9f0-84ce-4795-bcea-634d6a78e867")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace]
        public Organisation Supplier { get; set; }

        #region Allors
        [Id("70914837-D4C3-472B-A6DA-E8EE42D36E99")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace]
        public InternalOrganisation InternalOrganisation { get; set; }

        #region Allors
        [Id("7C5FDA1C-CE16-45C5-80DB-D89E2E9FB273")]
        #endregion
        [Required]
        [Workspace]
        public bool NeedsApproval { get; set; }

        #region Allors
        [Id("EC58B25E-D84A-402D-873B-A48E1E59365D")]
        #endregion
        [Workspace]
        public decimal ApprovalThresholdLevel1 { get; set; }

        #region Allors
        [Id("3ABDE14F-EEC1-4B45-9846-7896ABC27FBB")]
        #endregion
        [Workspace]
        public decimal ApprovalThresholdLevel2 { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }
        #endregion
    }
}

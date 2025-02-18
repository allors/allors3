// <copyright file="VatRegime.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("69db99bc-97f7-4e2e-903c-74afb55992af")]
    #endregion
    public partial class VatRegime : Enumeration, Versioned, Deletable
    {
        #region inherited properties
        public LocalisedText[] LocalisedNames { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        #endregion

        #region Versioning
        #region Allors
        [Id("9c67ee80-4119-4af7-a997-1ed14b4e340b")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public VatRegimeVersion CurrentVersion { get; set; }

        #region Allors
        [Id("ec4eb2ac-ed0d-4649-aa8c-72d51bc9a032")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public VatRegimeVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("65eb1cc4-46b6-435f-b5a1-4ca359680fdd")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        public Country[] Countries { get; set; }

        #region Allors
        [Id("8c66f441-be9a-468f-86f7-19fb2cebb51b")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        public VatRate[] VatRates { get; set; }

        #region Allors
        [Id("b2464c0e-668d-4906-9f66-ec57ab436052")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public VatSystem VatSystem { get; set; }

        #region Allors
        [Id("36b9d86d-4e2e-4ff5-b167-8ea6c81dd6cc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public VatBox[] VatBoxes { get; set; }

        #region Allors
        [Id("00A91056-1F2D-462F-8A81-6DA277AD86E1")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public VatClause VatClause { get; set; }

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

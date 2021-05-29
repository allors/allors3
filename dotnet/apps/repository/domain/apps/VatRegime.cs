// <copyright file="VatRegime.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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
    public partial class VatRegime : Enumeration, Versioned
    {
        #region inherited properties
        public LocalisedText[] LocalisedNames { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public Permission[] DeniedPermissions { get; set; }

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
        [Workspace]
        public VatRegimeVersion CurrentVersion { get; set; }

        #region Allors
        [Id("ec4eb2ac-ed0d-4649-aa8c-72d51bc9a032")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace]
        public VatRegimeVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("71cb2a90-e65e-4a98-a2dd-cb806d7ed0e7")]
        #endregion
        [Workspace]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public Country Country { get; set; }

        #region Allors
        [Id("2071cc28-c8bf-43dc-a5e5-ec5735756dfa")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public VatRate ObsoleteVatRate { get; set; }

        #region Allors
        [Id("8c66f441-be9a-468f-86f7-19fb2cebb51b")]
        #endregion
        [Workspace]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        public VatRate[] VatRates { get; set; }

        #region Allors
        [Id("b2464c0e-668d-4906-9f66-ec57ab436052")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public VatSystem VatSystem { get; set; }

        #region Allors
        [Id("36b9d86d-4e2e-4ff5-b167-8ea6c81dd6cc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace]
        public VatBox[] VatBoxes { get; set; }

        #region Allors
        [Id("00A91056-1F2D-462F-8A81-6DA277AD86E1")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public VatClause VatClause { get; set; }

        #region Allors
        [Id("a037f9f0-1aff-4ad0-8ee9-36ae4609d398")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public OrganisationGlAccount GeneralLedgerAccount { get; set; }

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

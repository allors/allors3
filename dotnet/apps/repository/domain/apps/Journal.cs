// <copyright file="Journal.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("d3446420-6d2a-4d18-a6eb-0405da9f7cc5")]
    #endregion
    public partial class Journal : Object, Versioned
    {
        #region inherited properties

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Versioning
        #region Allors
        [Id("ef255fc2-2791-48d3-a80b-0acb8ff816f8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public JournalVersion CurrentVersion { get; set; }

        #region Allors
        [Id("e120f16a-40c6-4c80-ba2e-8430f175a5e7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public JournalVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("37493cfc-e817-4b89-b7cb-7d29f69cf41e")]
        #endregion
        [Required]
        public string Name { get; set; }

        #region Allors
        [Id("1ec79ec4-60a8-4fdc-b11e-8c25697cd457")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public JournalType JournalType { get; set; }

        #region Allors
        [Id("01abf1e4-c2f8-4d04-8046-f5ac5428ff11")]
        #endregion
        [Required]
        public bool UseAsDefault { get; set; }

        #region Allors
        [Id("1915b6f2-aada-457e-8723-8f196a1e2fde")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        public InternalOrganisation InternalOrganisation { get; set; }

        #region Allors
        [Id("9b7c3687-b268-4c2b-8b04-c04a0c55d79f")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        public AccountingTransaction[] AccountingTransactions { get; set; }

        #region Allors
        [Id("04f786b4-66be-4616-9966-ac026384c0d3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public OrganisationGlAccount UnassignedAccount { get; set; }

        #region Allors
        [Id("3a52aa7c-fa01-4845-866c-976e48ea2179")]
        #endregion
        [Required]
        public bool BlockUnpaidTransactions { get; set; }

        #region Allors
        [Id("4f1b0471-67f9-4fa1-9b69-b5d9cbeda5e7")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Required]
        public OrganisationGlAccount ContraAccount { get; set; }

        #region Allors
        [Id("9491207d-1e98-48e2-bbfc-de1b7563ce1d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public Currency Currency { get; set; }

        #region Allors
        [Id("dbdca15b-5337-44f1-b490-c69cb36df9c3")]
        #endregion
        [Required]
        public bool CloseWhenInBalance { get; set; }

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

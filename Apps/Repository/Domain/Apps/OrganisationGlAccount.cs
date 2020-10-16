// <copyright file="OrganisationGlAccount.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("59f3100c-da48-4b4c-a302-1a75e37216a6")]
    #endregion
    public partial class OrganisationGlAccount : Period, Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        #endregion

        #region Allors
        [Id("8e20ce74-a772-45c8-a76a-a8ca0d4d7ebd")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Product Product { get; set; }

        #region Allors
        [Id("4D01DDD1-32B6-44A2-A42B-1CFEE1E6F42A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public InternalOrganisation InternalOrganisation { get; set; }

        #region Allors
        [Id("9337f791-56aa-4086-b661-2043cf02c662")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public OrganisationGlAccount SubsidiaryOf { get; set; }

        #region Allors
        [Id("9af20c76-200c-4aed-8154-99fd88907a15")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party Party { get; set; }

        #region Allors
        [Id("a1608d47-9fa7-4dc4-9736-c59f28221842")]
        #endregion
        [Derived]
        [Required]
        [Workspace(Default)]
        public bool HasBankStatementTransactions { get; set; }

        #region Allors
        [Id("c0de2fbb-9e70-4094-8279-fb46734e920e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ProductCategory ProductCategory { get; set; }

        #region Allors
        [Id("f1d3e642-2844-4c5a-a053-4dcfce461902")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public GeneralLedgerAccount GeneralLedgerAccount { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion
    }
}

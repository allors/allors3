// <copyright file="GeneralLedgerAccount.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("1a0e396b-69bd-4e77-a602-3d7f7938fd74")]
    #endregion
    public partial class GeneralLedgerAccount : UniquelyIdentifiable, Object
    {
        #region inherited properties
        public Guid UniqueId { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("0144834d-c5a9-42e7-bf22-af46ff95ee5f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Product DefaultCostUnit { get; set; }

        #region Allors
        [Id("01c49e6f-087a-494d-902d-12811442470e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public CostCenter DefaultCostCenter { get; set; }

        #region Allors
        [Id("08bb53f7-9b27-4079-bb9b-d8ff96f89b42")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string Description { get; set; }

        #region Allors
        [Id("27ba2d5b-9e0b-4b20-9b34-f007a0f2e2f2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public GeneralLedgerAccountType GeneralLedgerAccountType { get; set; }

        #region Allors
        [Id("2e6545f8-5fcf-4129-99f6-1f41280cd02d")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool CashAccount { get; set; }

        #region Allors
        [Id("3fc28997-124c-4e16-9c4d-128314e6395c")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool CostCenterAccount { get; set; }

        #region Allors
        [Id("4877e61b-443f-4bef-820f-5c93f8d42b8a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public DebitCreditConstant Side { get; set; }

        #region Allors
        [Id("5f797e0d-05aa-4dfb-a826-157ac6cdb0a9")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool BalanceSheetAccount { get; set; }

        #region Allors
        [Id("7f2e28ea-124a-45fa-9ed3-e3c2b0bb1822")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ReconciliationAccount { get; set; }

        #region Allors
        [Id("8616e916-a3e2-4cfe-84a4-778fd4b50d87")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("9b679f99-d678-4ec0-8ab1-e02eaabe6658")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool CostCenterRequired { get; set; }

        #region Allors
        [Id("a3aa445f-2aae-41be-8024-7b4a7e0a76ed")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool CostUnitRequired { get; set; }

        #region Allors
        [Id("aa569c0a-597d-4b75-a527-25c6ef339547")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public GeneralLedgerAccountGroup GeneralLedgerAccountGroup { get; set; }

        #region Allors
        [Id("beda5c75-e1a0-493a-85ec-a943214cec8d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public CostCenter[] CostCentersAllowed { get; set; }

        #region Allors
        [Id("bfe446ee-f9ff-462f-bb45-9bf52d61daa4")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool CostUnitAccount { get; set; }

        #region Allors
        [Id("cedccf34-0386-4be3-aa77-6ec0a9032c15")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string AccountNumber { get; set; }

        #region Allors
        [Id("d2078f49-9745-48e5-bdd2-7d7738f25d4e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public Product[] CostUnitsAllowed { get; set; }

        #region Allors
        [Id("e433abed-8f41-4a23-8e5b-e597bb6a14d2")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool Protected { get; set; }

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

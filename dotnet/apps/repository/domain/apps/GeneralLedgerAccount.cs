// <copyright file="GeneralLedgerAccount.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
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
        [Id("0f57c2f7-eee2-4bf1-a72d-056990634f05")]
        #endregion
        [Required]
        [Workspace]
        public string ReferenceCode { get; set; }

        #region Allors
        [Id("c0d736c2-f9a4-4647-bb5a-3a088f56a56a")]
        #endregion
        [Required]
        [Workspace]
        public string SortCode { get; set; }

        #region Allors
        [Id("cedccf34-0386-4be3-aa77-6ec0a9032c15")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace]
        public string ReferenceNumber { get; set; }

        #region Allors
        [Id("8616e916-a3e2-4cfe-84a4-778fd4b50d87")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("08bb53f7-9b27-4079-bb9b-d8ff96f89b42")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string Description { get; set; }

        #region Allors
        [Id("f558432e-f572-4a89-ad18-c71d3436c52a")]
        #endregion
        [Size(256)]
        [Workspace]
        public string SearchCode { get; set; }

        #region Allors
        [Id("27ba2d5b-9e0b-4b20-9b34-f007a0f2e2f2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public GeneralLedgerAccountType GeneralLedgerAccountType { get; set; }

        #region Allors
        [Id("aa569c0a-597d-4b75-a527-25c6ef339547")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public GeneralLedgerAccountClassification GeneralLedgerAccountClassification { get; set; }

        #region Allors
        [Id("229e89f0-4447-4d51-b14c-dd13b0d5a47f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public GeneralLedgerAccount CounterPartAccount { get; set; }

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
        public BalanceSide BalanceSide { get; set; }

        #region Allors
        [Id("9ab7c686-d53b-440b-8bcb-8f23e3db8b7b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace]
        public BalanceType BalanceType { get; set; }

        #region Allors
        [Id("7f2e28ea-124a-45fa-9ed3-e3c2b0bb1822")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ReconciliationAccount { get; set; }

        #region Allors
        [Id("9b679f99-d678-4ec0-8ab1-e02eaabe6658")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool CostCenterRequired { get; set; }

        #region Allors
        [Id("01c49e6f-087a-494d-902d-12811442470e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public CostCenter DefaultCostCenter { get; set; }

        #region Allors
        [Id("beda5c75-e1a0-493a-85ec-a943214cec8d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public CostCenter[] AssignedCostCentersAllowed { get; set; }

        #region Allors
        [Id("ac43581a-1e12-415d-87e6-a96658c54569")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public CostCenter[] DerivedCostCentersAllowed { get; set; }

        #region Allors
        [Id("a3aa445f-2aae-41be-8024-7b4a7e0a76ed")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool CostUnitRequired { get; set; }

        #region Allors
        [Id("0144834d-c5a9-42e7-bf22-af46ff95ee5f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Product DefaultCostUnit { get; set; }

        #region Allors
        [Id("d2078f49-9745-48e5-bdd2-7d7738f25d4e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public Product[] AssignedCostUnitsAllowed { get; set; }

        #region Allors
        [Id("b8e82b04-f4bf-4d2f-acb9-ceb7b3b11a12")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public Product[] DerivedCostUnitsAllowed { get; set; }

        #region Allors
        [Id("bfe446ee-f9ff-462f-bb45-9bf52d61daa4")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool CostUnitAccount { get; set; }

        #region Allors
        [Id("e433abed-8f41-4a23-8e5b-e597bb6a14d2")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool Blocked { get; set; }

        #region Allors
        [Id("639f1b51-f5ea-464f-b394-8d70c92f3e17")]
        #endregion
        [Required]
        [Workspace]
        public bool Compressed { get; set; }

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

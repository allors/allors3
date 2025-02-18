
// <copyright file="GeneralLedgerAccount.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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
    public partial class GeneralLedgerAccount : UniquelyIdentifiable, Object, ExternalWithPrimaryKey, Deletable, IDisplayName
    {
        #region inherited properties
        public Guid UniqueId { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string ExternalPrimaryKey { get; set; }

        public string DisplayName { get; set; }
        #endregion

        #region Allors
        [Id("0f57c2f7-eee2-4bf1-a72d-056990634f05")]
        #endregion
        [Workspace(Default)]
        public string ReferenceCode { get; set; }

        #region Allors
        [Id("c0d736c2-f9a4-4647-bb5a-3a088f56a56a")]
        #endregion
        [Workspace(Default)]
        public string SortCode { get; set; }

        #region Allors
        [Id("cedccf34-0386-4be3-aa77-6ec0a9032c15")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
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
        [Workspace(Default)]
        public string SearchCode { get; set; }

        #region Allors
        [Id("27ba2d5b-9e0b-4b20-9b34-f007a0f2e2f2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccountType GeneralLedgerAccountType { get; set; }

        #region Allors
        [Id("aa569c0a-597d-4b75-a527-25c6ef339547")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public GeneralLedgerAccountClassification GeneralLedgerAccountClassification { get; set; }

        #region Allors
        [Id("229e89f0-4447-4d51-b14c-dd13b0d5a47f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public GeneralLedgerAccount CounterPartAccount { get; set; }

        #region Allors
        [Id("b6ad3beb-e9cf-492e-b996-969d42c308ea")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public GeneralLedgerAccount Parent { get; set; }

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
        [Workspace(Default)]
        public BalanceSide BalanceSide { get; set; }

        #region Allors
        [Id("9ab7c686-d53b-440b-8bcb-8f23e3db8b7b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
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
        public CostUnit DefaultCostUnit { get; set; }

        #region Allors
        [Id("d2078f49-9745-48e5-bdd2-7d7738f25d4e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public CostUnit[] AssignedCostUnitsAllowed { get; set; }

        #region Allors
        [Id("b8e82b04-f4bf-4d2f-acb9-ceb7b3b11a12")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public CostUnit[] DerivedCostUnitsAllowed { get; set; }

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
        [Workspace(Default)]
        public bool Compressed { get; set; }

        #region Allors
        [Id("02ccb73a-62f2-438d-9839-b5b0de62802d")]
        #endregion
        [Workspace(Default)]
        public int RgsLevel { get; set; }

        #region Allors
        [Id("266b3a2a-48f3-409b-95ad-570661ae833d")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool IsRgsExcluded { get; set; }

        #region Allors
        [Id("4e7d7e63-1221-47bf-8cb3-a09fe1189d9e")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool IsRgsBase{ get; set; }

        #region Allors
        [Id("2f6a3db9-9860-4a0f-a909-efecf8d653cb")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool IsRgsExtended { get; set; }

        #region Allors
        [Id("3461443f-77da-427c-8eff-6d3031f6ac83")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool IsRgsUseWithEZ{ get; set; }

        #region Allors
        [Id("1af4092a-e09a-4e23-ad69-2598eb323ed9")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool IsRgsUseWithZzp { get; set; }

        #region Allors
        [Id("dd717afe-259c-4cff-ad6e-94ecd127a89f")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool IsRgsUseWithWoco { get; set; }

        #region Allors
        [Id("21c0feea-8912-49ad-b986-31267d44242a")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ExcludeRgsBB { get; set; }

        #region Allors
        [Id("91b43350-5032-422a-971b-000a2a2d149b")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ExcludeRgsAgro { get; set; }

        #region Allors
        [Id("73987542-9efe-4ef6-b5c5-5a257dcfd623")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ExcludeRgsWKR { get; set; }

        #region Allors
        [Id("d79f31a5-32ed-47d9-b9d9-99553a015381")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ExcludeRgsEZVOF { get; set; }

        #region Allors
        [Id("fbd8c25f-39f3-4ebc-a36c-6d599a75e48d")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ExcludeRgsBV { get; set; }

        #region Allors
        [Id("5a0efef2-a7ea-49b1-bc16-f6a4304edd20")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ExcludeRgsWoco { get; set; }

        #region Allors
        [Id("ef10ad7c-fd50-4e47-8d98-b122629f73c4")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ExcludeRgsBank { get; set; }

        #region Allors
        [Id("228cc6dc-dd15-46e5-94fd-5cf44864d49a")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ExcludeRgsOZW { get; set; }

        #region Allors
        [Id("5dc266df-5de3-446d-9152-133738adef25")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ExcludeRgsAfrekSyst { get; set; }

        #region Allors
        [Id("d31494bf-749a-4129-9047-b4915c81c40f")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ExcludeRgsNivo5 { get; set; }

        #region Allors
        [Id("21092dff-29d1-4746-984f-42748a5a435c")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ExcludeRgsUitbr5 { get; set; }

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

// <copyright file="AccountingTransaction.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("785a36a9-4710-4f3f-bd26-dbaff5353535")]
    #endregion
    public partial class AccountingTransaction : Object, Deletable
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("4e93aa10-beae-4eb5-af92-c27a006380ae")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace]
        public AccountingTransactionType AccountingTransactionType { get; set; }

        #region Allors
        [Id("79dc0f13-0caf-4acc-b4aa-fc6f5ed37129")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace]
        public string AccountingTransactionTypeName { get; set; }

        #region Allors
        [Id("a7fb7e5a-287a-41a1-b6b9-bd56601732f3")]
        [Required]
        [Derived]
        #endregion
        [Workspace]
        public string TransactionNumber { get; set; }

        #region Allors
        [Id("be061dda-bb8f-4bc1-b386-dc0c05dc6eaf")]
        #endregion
        [Required]
        [Workspace]
        public DateTime EntryDate { get; set; }

        #region Allors
        [Id("EF969E4C-ADD5-4A3D-A718-857CC99BBACA")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace]
        public InternalOrganisation InternalOrganisation { get; set; }

        #region Allors
        [Id("327fc2cb-9589-4e9d-b9e6-7429cbe14746")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Party FromParty { get; set; }

        #region Allors
        [Id("8f08f0b2-ad80-4486-8ed4-e63ca7b399cd")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace]
        public string FromPartyDisplayName { get; set; }

        #region Allors
        [Id("681312d3-63cd-45a2-883c-4a907d379f52")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Party ToParty { get; set; }

        #region Allors
        [Id("286c0ba0-9056-494c-9f9c-cfe7a7399956")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace]
        public string ToPartyDisplayName { get; set; }

        #region Allors
        [Id("83ae8e4e-c4cd-4f27-b5fd-b468e4603295")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public FixedAsset FixedAsset { get; set; }

        #region Allors
        [Id("c6270d9a-2ac0-46bb-acab-1feada800847")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace]
        public string FixedAssetDisplayName { get; set; }

        #region Allors
        [Id("9b376e18-7cf8-43f7-ac89-ef4b32a1c8fd")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Invoice Invoice { get; set; }

        #region Allors
        [Id("2969baaf-20c2-4bde-8c5c-f57efd1c2858")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace]
        public string InvoiceNumber { get; set; }

        #region Allors
        [Id("d2f6a94f-f2f5-4947-bbd4-b345df778378")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace]
        public Payment Payment { get; set; }

        #region Allors
        [Id("12d6d0a5-a418-4133-b503-3deb6691d7ae")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace]
        public InventoryItemTransaction InventoryItemTransaction { get; set; }

        #region Allors
        [Id("b88aa403-ab8b-4363-bfb2-35853ea9f7e0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Shipment Shipment { get; set; }

        #region Allors
        [Id("d673b160-dfbc-4c93-b2f0-23099ed5de7b")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace]
        public string ShipmentNumber { get; set; }

        #region Allors
        [Id("8352b682-9628-4423-8745-400688a79774")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public WorkEffort WorkEffort { get; set; }

        #region Allors
        [Id("17d62455-ccd2-406e-9fe8-f59709d80bba")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace]
        public string WorkEffortNumber { get; set; }

        #region Allors
        [Id("4e4cb94c-424c-4824-ad44-5bb1c7312a52")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        public AccountingTransactionDetail[] AccountingTransactionDetails { get; set; }

        #region Allors
        [Id("657f2688-4af0-4580-add2-c8a30b32e016")]
        #endregion
        [Size(-1)]
        [Workspace]
        public string Description { get; set; }

        #region Allors
        [Id("77910a3f-3547-4d6b-92e0-f1fc136e22da")]
        #endregion
        [Workspace]
        public DateTime TransactionDate { get; set; }

        #region Allors
        [Id("2c2fd25f-2070-4594-8dc5-e9dcabbb1656")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public AccountingPeriod AccountingPeriod { get; set; }

        #region Allors
        [Id("a29cb739-8d2f-4e7d-a652-af8d2e190658")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal DerivedTotalAmount { get; set; }

        #region Allors
        [Id("b68801be-f9be-4df7-b34d-609f1397dc07")]
        #endregion
        [Required]
        [Workspace]
        public bool Exported { get; set; }

        #region inherited methods

        public void OnBuild()
        {
        }

        public void OnPostBuild()
        {
        }

        public void OnInit()
        {
        }

        public void OnPostDerive()
        {
        }

        public void Delete() { }

        #endregion
    }
}

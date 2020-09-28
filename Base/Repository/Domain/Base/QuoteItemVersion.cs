// <copyright file="QuoteItemVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;

    #region Allors
    [Id("6D38838C-CA7A-4ACC-B240-E4A1F3AE2DC9")]
    #endregion
    public partial class QuoteItemVersion : Version
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public User LastModifiedBy { get; set; }

        #endregion

        #region Allors
        [Id("EB4A6E73-333A-4BBD-BE8A-C7DCCFCC7A8A")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace]
        public QuoteItemState QuoteItemState { get; set; }

        #region Allors
        [Id("5A4AFCB5-B067-424D-95D7-B8B77AB9D125")]
        #endregion
        [Workspace]
        [Size(-1)]
        public string InternalComment { get; set; }

        #region Allors
        [Id("2FF532EE-F6C4-4DE9-9F6A-53EBB0747D51")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Party Authorizer { get; set; }

        #region Allors
        [Id("36035A65-0806-4B96-9E57-5AC0176DA4C2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Deliverable Deliverable { get; set; }

        #region Allors
        [Id("95CF9D84-E1FE-40DC-AE7C-8CA2DDC6687C")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Product Product { get; set; }

        #region Allors
        [Id("A24339CD-58D1-4849-AB1C-CA38543B5580")]
        #endregion
        [Workspace]
        public DateTime EstimatedDeliveryDate { get; set; }

        #region Allors
        [Id("4D54E4C2-E612-484C-B4A4-EAC04D96BD5A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Workspace]
        public DateTime RequiredByDate { get; set; }

        #region Allors
        [Id("9E8A5EBF-AE58-4F04-B29C-2A813713C52E")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public UnitOfMeasure UnitOfMeasure { get; set; }

        #region Allors
        [Id("9BD1DB0E-5215-4764-8C1D-C88B98917D5B")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public ProductFeature ProductFeature { get; set; }

        #region Allors
        [Id("14e62d23-d178-4445-81f2-f984447080d8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public SerialisedItem SerialisedItem { get; set; }

        #region Allors
        [Id("985322DB-5906-44FE-AD94-A4243AD99ADC")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal UnitPrice { get; set; }

        #region Allors
        [Id("FCA1614D-45CD-4295-9911-B9464EA9A4C4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Skill Skill { get; set; }

        #region Allors
        [Id("E4C53A94-52F4-4A0D-8204-B315EECB16E2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public WorkEffort WorkEffort { get; set; }

        #region Allors
        [Id("932A2BF0-AA86-431B-BC77-818E1EC5A837")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        public QuoteTerm[] QuoteTerms { get; set; }

        #region Allors
        [Id("A8810D49-AAA6-43A8-99F8-DC7E6B30D83E")]
        #endregion
        [Workspace]
        public decimal Quantity { get; set; }

        #region Allors
        [Id("24B8A678-F28D-4041-9514-490CDC1FDE7D")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public RequestItem RequestItem { get; set; }

        #region Allors
        [Id("2a181149-58cd-4b82-86eb-b2f28e439481")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace]
        public IrpfRegime IrpfRegime { get; set; }

        #region Allors
        [Id("81b88b1b-3718-48ed-bace-174b65aa83bc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public IrpfRegime AssignedIrpfRegime { get; set; }

        #region Allors
        [Id("17f50756-7c9f-404c-974b-4af5b30a65d1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace]
        public IrpfRate IrpfRate { get; set; }

        #region Allors
        [Id("647c3cdb-88af-4446-9d05-cf9ba72ab08c")]
        #endregion
        [Precision(19)]
        [Scale(5)]
        [Workspace]
        public decimal UnitIrpf { get; set; }

        #region Allors
        [Id("cd920eb3-b705-4304-b210-96d8d7beecd5")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal TotalIrpf { get; set; }

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

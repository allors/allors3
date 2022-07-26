// <copyright file="InternalOrganisationAccountingSetting.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("9a26bc0a-87ba-4567-8558-43afea82076b")]
    #endregion
    [Plural("InternalOrganisationAccountingSettingses")]
    public partial class InternalOrganisationAccountingSettings : Object
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("9877ddf0-ddd7-40d0-a81e-b3102f2252af")]
        #endregion
        [Workspace(Default)]
        public int FiscalYearStartMonth { get; set; }

        #region Allors
        [Id("61dfb7f8-2c22-4808-8b38-17303e57c3d6")]
        #endregion
        [Workspace(Default)]
        public int FiscalYearStartDay { get; set; }

        #region Allors
        [Id("4cfd9d7c-476a-4244-87eb-801041f7e092")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Counter SubAccountCounter { get; set; }

        #region Allors
        [Id("07643669-ae77-4034-8459-c2e95470fba9")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public AccountingPeriod ActualAccountingPeriod { get; set; }

        #region Allors
        [Id("6566c209-15e4-4d35-8e0a-8c29ef2ee3ee")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal MaximumAllowedPaymentDifference { get; set; }

        #region Allors
        [Id("483e4bae-6ae1-44e3-9104-bf63c3e83b33")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public CostCenterSplitMethod CostCenterSplitMethod { get; set; }

        #region Allors
        [Id("a0a0ecb7-37d0-4ead-b66f-283ba9e6ac7c")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public Organisation TaxAuthorityAccount { get; set; }

        #region Allors
        [Id("653b3939-15fd-492b-84a7-dd57321ead1e")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount AccountsPayable { get; set; }

        #region Allors
        [Id("1061f402-8ef6-40bc-82dc-1a8ea38e2add")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount AccountsReceivable { get; set; }

        #region Allors
        [Id("b42714fd-aab6-4179-b798-ac46357dba50")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount SalesPaymentDifferences { get; set; }

        #region Allors
        [Id("33998581-8a35-45bc-a833-ce0bfe8f29ec")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount PurchasePaymentDifferences { get; set; }

        #region Allors
        [Id("24f7fabb-a87e-48be-ba8c-054be3f97014")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount CalculationDifferences { get; set; }

        #region Allors
        [Id("cf2950de-3f62-4a54-8ca7-117f95a8795f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount ExhangeRateDifferences { get; set; }

        #region Allors
        [Id("97d14708-613b-409b-8b68-51c160dfcea4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount OpeningBalance { get; set; }

        #region Allors
        [Id("ee3d6e6b-2278-4ce8-b14e-41bef1182378")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount Inventory { get; set; }

        #region Allors
        [Id("524bf214-0937-4d95-ac4d-35676817ba37")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount DeferredExpense { get; set; }

        #region Allors
        [Id("f2930449-7cf9-45fe-b421-df2b54d08ae7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount DeferredRevenue { get; set; }

        #region Allors
        [Id("cf34e1f5-4b8f-4a63-8eb8-52a8ffdb2f64")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount AccruedRevenue { get; set; }

        #region Allors
        [Id("3b70b1a6-47e3-49a4-bb14-e77d442733df")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount AccruedExpense { get; set; }

        #region Allors
        [Id("e28a17cb-8d7f-4134-912a-ad1f92c1b304")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount Equity { get; set; }

        #region Allors
        [Id("7cf58a64-b08d-4655-ac0a-5410a39a1131")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount RetainedEarnings { get; set; }

        #region Allors
        [Id("dedaab4a-2309-4afb-ad46-530336a2adc3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount NetIncome { get; set; }

        #region Allors
        [Id("35f7a894-704e-48ec-93e9-1a531fcb2a07")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public InternalOrganisationInvoiceSettings[] SettingsForInvoiceItemType { get; set; }

        #region Allors
        [Id("3b8ebd9c-59d7-4389-80b6-5ccdd582197f")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public InternalOrganisationInventorySettings[] SettingsForInventoryVariance { get; set; }

        #region Allors
        [Id("3f624382-3398-4562-9483-e46946988632")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public InternalOrganisationVatRegimeSettings[] SettingsForVatRegime{ get; set; }

        #region Allors
        [Id("4a95ad2c-63ba-481c-9698-5d2094df6ed4")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public RgsFilter RgsFilter { get; set; }

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

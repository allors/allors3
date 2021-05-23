// <copyright file="InternalOrganisationAccountingSetting.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("9a26bc0a-87ba-4567-8558-43afea82076b")]
    #endregion
    [Plural("InternalOrganisationAccountingSettingses")]
    public partial class InternalOrganisationAccountingSettings : Object
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("9877ddf0-ddd7-40d0-a81e-b3102f2252af")]
        #endregion
        [Workspace]
        public int FiscalYearStartMonth { get; set; }

        #region Allors
        [Id("61dfb7f8-2c22-4808-8b38-17303e57c3d6")]
        #endregion
        [Workspace]
        public int FiscalYearStartDay { get; set; }

        #region Allors
        [Id("4cfd9d7c-476a-4244-87eb-801041f7e092")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Counter SubAccountCounter { get; set; }

        #region Allors
        [Id("07643669-ae77-4034-8459-c2e95470fba9")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        [Workspace]
        public AccountingPeriod ActualAccountingPeriod { get; set; }

        #region Allors
        [Id("6566c209-15e4-4d35-8e0a-8c29ef2ee3ee")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal MaximumAllowedPaymentDifference { get; set; }

        #region Allors
        [Id("483e4bae-6ae1-44e3-9104-bf63c3e83b33")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public CostCenterSplitMethod CostCenterSplitMethod { get; set; }

        #region Allors
        [Id("b42714fd-aab6-4179-b798-ac46357dba50")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace]
        public GeneralLedgerAccount SalesPaymentDifferencesAccount { get; set; }

        #region Allors
        [Id("0ddaf8c4-d83a-4a41-a2bb-aade7ff1bc4e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public GeneralLedgerAccount SalesPaymentDiscountDifferencesAccount { get; set; }

        #region Allors
        [Id("33998581-8a35-45bc-a833-ce0bfe8f29ec")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public GeneralLedgerAccount PurchasePaymentDifferencesAccount { get; set; }

        #region Allors
        [Id("55573252-d587-4832-bc6f-7b663fc0903f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public GeneralLedgerAccount PurchasePaymentDiscountDifferencesAccount { get; set; }

        #region Allors
        [Id("4c1bee9b-96b2-4415-ad89-a53d9caf60c1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public GeneralLedgerAccount RetainedEarningsAccount { get; set; }

        #region Allors
        [Id("97d14708-613b-409b-8b68-51c160dfcea4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public GeneralLedgerAccount SuspenceAccount { get; set; }

        #region Allors
        [Id("679187d6-eb6f-4eaa-9fcb-a635de336759")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public GeneralLedgerAccount NetIncomeAccount { get; set; }

        #region Allors
        [Id("24f7fabb-a87e-48be-ba8c-054be3f97014")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public GeneralLedgerAccount CalculationDifferencesAccount { get; set; }

        #region Allors
        [Id("4a95ad2c-63ba-481c-9698-5d2094df6ed4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace]
        public RgsFilter[] RgsFilters { get; set; }

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

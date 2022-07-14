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
        [Id("b42714fd-aab6-4179-b798-ac46357dba50")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount SalesPaymentDifferencesAccount { get; set; }

        #region Allors
        [Id("33998581-8a35-45bc-a833-ce0bfe8f29ec")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount PurchasePaymentDifferencesAccount { get; set; }

        #region Allors
        [Id("24f7fabb-a87e-48be-ba8c-054be3f97014")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount CalculationDifferencesAccount { get; set; }

        #region Allors
        [Id("cf2950de-3f62-4a54-8ca7-117f95a8795f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount ExhangeRateDifferencesAccount { get; set; }

        #region Allors
        [Id("97d14708-613b-409b-8b68-51c160dfcea4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount OpeningBalanceSuspenceAccount { get; set; }

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

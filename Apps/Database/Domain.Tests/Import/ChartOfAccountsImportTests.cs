// <copyright file="ChartOfAccountsImportTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class ChartOfAccountsImportTests : DomainTest, IClassFixture<Fixture>
    {
        public ChartOfAccountsImportTests(Fixture fixture) : base(fixture) { }

        // TODO: Import
        // [Fact]
        // public void GivenGeneralLedgerAccountXml_WhenImported_ThenGeneralLedgerAccountIsCreated()
        // {
        //    var filepath = string.Format("domain\\import\\minimaal genormaliseerd rekeningstelsel 1.xml");

        // new ChartOfAccountsImport(this.DatabaseTransaction).ImportChartOfAccounts(filepath);

        // Assert.Single(new GeneralLedgerAccounts(this.DatabaseTransaction).Extent().Count);
        //    Assert.Single(this.DatabaseTransaction.Extent<GeneralLedgerAccount>().Count);
        //    Assert.Single(this.DatabaseTransaction.Extent<ChartOfAccounts>().Count);
        //    Assert.Single(this.DatabaseTransaction.Extent<GeneralLedgerAccountClassification>().Count);
        //    Assert.Single(this.DatabaseTransaction.Extent<GeneralLedgerAccountType>().Count);
        //    Assert.Equal(0, this.DatabaseTransaction.Extent<CostCenter>().Count);

        // var chartOfAccounts = (ChartOfAccounts)this.DatabaseTransaction.Extent<ChartOfAccounts>().First;
        //    Assert.Equal("Minimum Algemeen Rekeningenstelsel", chartOfAccounts.Name);
        //    Assert.Single(chartOfAccounts.GeneralLedgerAccounts.Count);

        // var generalLedgerAccountClassification = (GeneralLedgerAccountClassification)this.DatabaseTransaction.Extent<GeneralLedgerAccountClassification>().First;
        //    Assert.Equal("Kapitaal", generalLedgerAccountClassification.Description);
        //    Assert.False(generalLedgerAccountClassification.ExistParent);

        // var generalLedgerAccountType = (GeneralLedgerAccountType)this.DatabaseTransaction.Extent<GeneralLedgerAccountType>().First;
        //    Assert.Equal("Eigen vermogen, voorzieningen voor risico's en kosten en schulden op langer dan een jaar", generalLedgerAccountType.Description);

        // var generalLedgerAccount = (GeneralLedgerAccount)this.DatabaseTransaction.Extent<GeneralLedgerAccount>().First;
        //    Assert.Equal("100000", generalLedgerAccount.AccountNumber);
        //    Assert.True(generalLedgerAccount.BalanceSheetAccount);
        //    Assert.False(generalLedgerAccount.CashAccount);
        //    Assert.Equal(chartOfAccounts, generalLedgerAccount.ChartOfAccountsWhereGeneralLedgerAccount);
        //    Assert.False(generalLedgerAccount.CostCenterAccount);
        //    Assert.False(generalLedgerAccount.CostCenterRequired);
        //    Assert.False(generalLedgerAccount.ExistCostCentersAllowed);
        //    Assert.False(generalLedgerAccount.CostUnitAccount);
        //    Assert.False(generalLedgerAccount.CostUnitRequired);
        //    Assert.False(generalLedgerAccount.ExistCostUnitsAllowed);
        //    Assert.Equal(new BalanceSides(this.DatabaseTransaction).Credit, generalLedgerAccount.Side);
        //    Assert.False(generalLedgerAccount.ExistDefaultCostCenter);
        //    Assert.False(generalLedgerAccount.ExistDefaultCostUnit);
        //    Assert.IsNullOrEmpty(generalLedgerAccount.Description);
        //    Assert.Equal(generalLedgerAccountClassification, generalLedgerAccount.GeneralLedgerAccountClassification);
        //    Assert.Equal(generalLedgerAccountType, generalLedgerAccount.GeneralLedgerAccountType);
        //    Assert.Equal("Geplaats kapitaal", generalLedgerAccount.Name);
        //    Assert.False(generalLedgerAccount.Protected);
        //    Assert.False(generalLedgerAccount.ReconciliationAccount);
        // }

        // [Fact]
        // public void GivenGeneralLedgerAccountXmlWithGroupHierarchy_WhenImported_ThenGeneralLedgerAccountClassificationsAreCreated()
        // {
        //    var filepath = string.Format("domain\\import\\minimaal genormaliseerd rekeningstelsel 2.xml");

        // new ChartOfAccountsImport(this.DatabaseTransaction).ImportChartOfAccounts(filepath);

        // Assert.Single(new GeneralLedgerAccounts(this.DatabaseTransaction).Extent().Count);
        //    Assert.Single(this.DatabaseTransaction.Extent<GeneralLedgerAccount>().Count);
        //    Assert.Single(this.DatabaseTransaction.Extent<ChartOfAccounts>().Count);
        //    Assert.Equal(3, this.DatabaseTransaction.Extent<GeneralLedgerAccountClassification>().Count);
        //    Assert.Single(this.DatabaseTransaction.Extent<GeneralLedgerAccountType>().Count);
        //    Assert.Equal(0, this.DatabaseTransaction.Extent<CostCenter>().Count);

        // var chartOfAccounts = (ChartOfAccounts)this.DatabaseTransaction.Extent<ChartOfAccounts>().First;
        //    Assert.Equal("Minimum Algemeen Rekeningenstelsel", chartOfAccounts.Name);
        //    Assert.Single(chartOfAccounts.GeneralLedgerAccounts.Count);

        // var generalLedgerAccountClassifications = this.DatabaseTransaction.Extent<GeneralLedgerAccountClassification>();
        //    generalLedgerAccountClassifications.Filter.AddEquals(GeneralLedgerAccountClassifications.Meta.Description, "Verworpen uitgaven");

        // var generalLedgerAccountClassification = (GeneralLedgerAccountClassification)generalLedgerAccountClassifications.First;
        //    Assert.True(generalLedgerAccountClassification.ExistParent);

        // var parent = generalLedgerAccountClassification.Parent;
        //    Assert.Equal("Overige kosten", parent.Description);
        //    Assert.True(parent.ExistParent);

        // var grandParent = parent.Parent;
        //    Assert.Equal("Diensten en diverse goederen", grandParent.Description);
        //    Assert.False(grandParent.ExistParent);

        // var generalLedgerAccountType = (GeneralLedgerAccountType)this.DatabaseTransaction.Extent<GeneralLedgerAccountType>().First;
        //    Assert.Equal("Kosten", generalLedgerAccountType.Description);

        // var generalLedgerAccount = (GeneralLedgerAccount)this.DatabaseTransaction.Extent<GeneralLedgerAccount>().First;
        //    Assert.Equal("617910", generalLedgerAccount.AccountNumber);
        //    Assert.False(generalLedgerAccount.BalanceSheetAccount);
        //    Assert.False(generalLedgerAccount.CashAccount);
        //    Assert.Equal(chartOfAccounts, generalLedgerAccount.ChartOfAccountsWhereGeneralLedgerAccount);
        //    Assert.False(generalLedgerAccount.CostCenterAccount);
        //    Assert.False(generalLedgerAccount.CostCenterRequired);
        //    Assert.False(generalLedgerAccount.ExistCostCentersAllowed);
        //    Assert.False(generalLedgerAccount.CostUnitAccount);
        //    Assert.False(generalLedgerAccount.CostUnitRequired);
        //    Assert.False(generalLedgerAccount.ExistCostUnitsAllowed);
        //    Assert.Equal(new BalanceSides(this.DatabaseTransaction).Debit, generalLedgerAccount.Side);
        //    Assert.False(generalLedgerAccount.ExistDefaultCostCenter);
        //    Assert.False(generalLedgerAccount.ExistDefaultCostUnit);
        //    Assert.IsNullOrEmpty(generalLedgerAccount.Description);
        //    Assert.Equal(generalLedgerAccountClassification, generalLedgerAccount.GeneralLedgerAccountClassification);
        //    Assert.Equal(generalLedgerAccountType, generalLedgerAccount.GeneralLedgerAccountType);
        //    Assert.Equal("Verworpen uitgaven niet-aftrekbare belastingen", generalLedgerAccount.Name);
        //    Assert.False(generalLedgerAccount.Protected);
        //    Assert.False(generalLedgerAccount.ReconciliationAccount);
        // }

        // [Fact]
        // public void GivenGeneralLedgerAccountXmlWithCostCenter_WhenImported_ThenCostCenterIsCreated()
        // {
        //    var filepath = string.Format("domain\\import\\minimaal genormaliseerd rekeningstelsel 3.xml");

        // new ChartOfAccountsImport(this.DatabaseTransaction).ImportChartOfAccounts(filepath);

        // Assert.Single(new GeneralLedgerAccounts(this.DatabaseTransaction).Extent().Count);
        //    Assert.Single(this.DatabaseTransaction.Extent<GeneralLedgerAccount>().Count);
        //    Assert.Single(this.DatabaseTransaction.Extent<ChartOfAccounts>().Count);
        //    Assert.Single(this.DatabaseTransaction.Extent<GeneralLedgerAccountClassification>().Count);
        //    Assert.Single(this.DatabaseTransaction.Extent<GeneralLedgerAccountType>().Count);
        //    Assert.Single(this.DatabaseTransaction.Extent<CostCenter>().Count);

        // var chartOfAccounts = (ChartOfAccounts)this.DatabaseTransaction.Extent<ChartOfAccounts>().First;
        //    Assert.Equal("Minimum Algemeen Rekeningenstelsel", chartOfAccounts.Name);
        //    Assert.Single(chartOfAccounts.GeneralLedgerAccounts.Count);

        // var generalLedgerAccountClassification = (GeneralLedgerAccountClassification)this.DatabaseTransaction.Extent<GeneralLedgerAccountClassification>().First;
        //    Assert.Equal("Kapitaal", generalLedgerAccountClassification.Description);
        //    Assert.False(generalLedgerAccountClassification.ExistParent);

        // var generalLedgerAccountType = (GeneralLedgerAccountType)this.DatabaseTransaction.Extent<GeneralLedgerAccountType>().First;
        //    Assert.Equal("Eigen vermogen, voorzieningen voor risico's en kosten en schulden op langer dan een jaar", generalLedgerAccountType.Description);

        // var costCenter = (CostCenter)this.DatabaseTransaction.Extent<CostCenter>().First;
        //    Assert.Equal("Misc", costCenter.Name);

        // var generalLedgerAccount = (GeneralLedgerAccount)this.DatabaseTransaction.Extent<GeneralLedgerAccount>().First;
        //    Assert.Equal("100000", generalLedgerAccount.AccountNumber);
        //    Assert.True(generalLedgerAccount.BalanceSheetAccount);
        //    Assert.False(generalLedgerAccount.CashAccount);
        //    Assert.Equal(chartOfAccounts, generalLedgerAccount.ChartOfAccountsWhereGeneralLedgerAccount);
        //    Assert.True(generalLedgerAccount.CostCenterAccount);
        //    Assert.False(generalLedgerAccount.CostCenterRequired);
        //    Assert.Equal(costCenter, generalLedgerAccount.CostCentersAllowed.First);
        //    Assert.False(generalLedgerAccount.CostUnitAccount);
        //    Assert.False(generalLedgerAccount.CostUnitRequired);
        //    Assert.False(generalLedgerAccount.ExistCostUnitsAllowed);
        //    Assert.Equal(new BalanceSides(this.DatabaseTransaction).Credit, generalLedgerAccount.Side);
        //    Assert.Equal(costCenter, generalLedgerAccount.DefaultCostCenter);
        //    Assert.False(generalLedgerAccount.ExistDefaultCostUnit);
        //    Assert.IsNullOrEmpty(generalLedgerAccount.Description);
        //    Assert.Equal(generalLedgerAccountClassification, generalLedgerAccount.GeneralLedgerAccountClassification);
        //    Assert.Equal(generalLedgerAccountType, generalLedgerAccount.GeneralLedgerAccountType);
        //    Assert.Equal("Geplaats kapitaal", generalLedgerAccount.Name);
        //    Assert.False(generalLedgerAccount.Protected);
        //    Assert.False(generalLedgerAccount.ReconciliationAccount);
        // }
    }
}

// <copyright file="CustomerShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.customershipment.create;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.TimeEntryTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.organisation.overview;
    using libs.workspace.angular.apps.src.lib.objects.salesinvoice.list;
    using libs.workspace.angular.apps.src.lib.objects.salesinvoice.overview;
    using libs.workspace.angular.apps.src.lib.objects.workeffort.list;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Sales")]
    public class TimeEntryCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly WorkEffortListComponent workEffortListPage;

        public TimeEntryCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.workEffortListPage = this.Sidenav.NavigateToWorkEfforts();
        }

        [Fact]
        public void Create()
        {
            //var before = new TimeEntries(this.Transaction).Extent().ToArray();

            //var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");

            //var expected = new DisbursementBuilder(this.Transaction).WithDefaults().Build();

            //this.Transaction.Derive();

            //var expectedAmount = expected.Amount;
            //var expectedEffectiveDate = expected.EffectiveDate;

            //this.workEffortListPage.Table.DefaultAction(salesInvoice);
            //var disbursement = new SalesInvoiceOverviewComponent(this.workEffortListPage.Driver, this.M).PaymentOverviewPanel.Click().CreateDisbursement();

            //disbursement.Amount.Set(expected.Amount.ToString());
            //disbursement.EffectiveDate.Set(expected.EffectiveDate);

            //this.Transaction.Rollback();
            //disbursement.SAVE.Click();

            //this.Driver.WaitForAngular();
            //this.Transaction.Rollback();

            //var after = new TimeEntries(this.Transaction).Extent().ToArray();

            //Assert.Equal(after.Length, before.Length + 1);

            //var actual = after.Except(before).First();

            //Assert.Equal(expectedAmount, actual.Amount);
            //Assert.Equal(expectedEffectiveDate.Date, actual.EffectiveDate.Date);
        }
    }
}

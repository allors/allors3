// <copyright file="EmailAddressEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;
using libs.workspace.angular.apps.src.lib.objects.emailaddress.edit;
using libs.workspace.angular.apps.src.lib.objects.salesinvoice.list;
using libs.workspace.angular.apps.src.lib.objects.person.overview;

namespace Tests.SalesInvoiceTests
{
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.salesinvoice.overview;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class RepeatingSalesInvoiceCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly SalesInvoiceListComponent salesInvoiceListPage;

        public RepeatingSalesInvoiceCreateTest(Fixture fixture) : base(fixture)
        {
            this.Login();
            this.salesInvoiceListPage = this.Sidenav.NavigateToSalesInvoices();
        }

        [Fact]
        public void Create()
        {
            var before = new RepeatingSalesInvoices(this.Transaction).Extent().ToArray();

            var expectedSalesInvoice = new SalesInvoices(this.Transaction).Extent().FirstOrDefault();
            var expected = new RepeatingSalesInvoiceBuilder(this.Transaction).WithDefaults(expectedSalesInvoice).Build();

            this.Transaction.Derive();

            var expectedFrequency = expected.Frequency;
            var expectedDayOfWeek = expected.DayOfWeek;
            var expectedNextExecutionDate = expected.NextExecutionDate;

            this.salesInvoiceListPage.Table.DefaultAction(expectedSalesInvoice);
            var salesInvoiceDetails = new SalesInvoiceOverviewComponent(this.salesInvoiceListPage.Driver, this.M);
            var repeatingSalesInvoiceItemDetail = salesInvoiceDetails.RepeatingsalesinvoiceOverviewPanel.Click().CreateRepeatingSalesInvoice();

            repeatingSalesInvoiceItemDetail
                .Frequency.Select(expected.Frequency)
                .DayOfWeek.Select(expected.DayOfWeek)
                .NextExecutionDate.Set(expected.NextExecutionDate);

            this.Transaction.Rollback();
            repeatingSalesInvoiceItemDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new RepeatingSalesInvoices(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var repeatingSalesInvoice = after.Except(before).First();

            Assert.Equal(expectedFrequency, repeatingSalesInvoice.Frequency);
            Assert.Equal(expectedDayOfWeek, repeatingSalesInvoice.DayOfWeek);
            Assert.Equal(expectedNextExecutionDate, repeatingSalesInvoice.NextExecutionDate);
        }
    }
}

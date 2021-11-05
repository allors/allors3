// <copyright file="CustomerShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.customershipment.create;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.WorkEffortAssignmentRateTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.organisation.overview;
    using libs.workspace.angular.apps.src.lib.objects.salesinvoice.list;
    using libs.workspace.angular.apps.src.lib.objects.salesinvoice.overview;
    using libs.workspace.angular.apps.src.lib.objects.workeffort.list;
    using libs.workspace.angular.apps.src.lib.objects.worktask.overview;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Sales")]
    public class WorkEffortAssignmentRateCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly WorkEffortListComponent workEffortListPage;

        public WorkEffortAssignmentRateCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.workEffortListPage = this.Sidenav.NavigateToWorkEfforts();
        }

        [Fact]
        public void Create()
        {
            var before = new WorkEffortAssignmentRates(this.Transaction).Extent().ToArray();
            var workEffort = new WorkEfforts(this.Transaction).Extent().First();

            var expected = new WorkEffortAssignmentRateBuilder(this.Transaction).WithDefaults().Build();

            this.Transaction.Derive();

            var expectedRateType = expected.RateType;
            var expectedRate = expected.Rate;

            this.workEffortListPage.Table.DefaultAction(workEffort);
            var workTaskComponent = new WorkTaskOverviewComponent(this.workEffortListPage.Driver, this.M).WorkeffortassignmentrateOverviewPanel.Click().CreateWorkEffortAssignmentRate();

            workTaskComponent
                .RateType.Select(expected.RateType)
                .Rate.Set(expected.Rate.ToString());

            this.Transaction.Rollback();
            workTaskComponent.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new WorkEffortAssignmentRates(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedRateType, actual.RateType);
            Assert.Equal(expectedRate, actual.Rate);
        }
    }
}

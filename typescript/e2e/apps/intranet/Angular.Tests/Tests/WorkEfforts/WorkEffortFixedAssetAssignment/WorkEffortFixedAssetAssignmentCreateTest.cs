// <copyright file="CustomerShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.customershipment.create;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.WorkEffortFixedAssetAssignmentTests
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
    public class WorkEffortFixedAssetAssignmentCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly WorkEffortListComponent workEffortListPage;

        public WorkEffortFixedAssetAssignmentCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.workEffortListPage = this.Sidenav.NavigateToWorkEfforts();
        }

        [Fact]
        public void Create()
        {
            var before = new WorkEffortFixedAssetAssignments(this.Transaction).Extent().ToArray();
            var workEffort = new WorkEfforts(this.Transaction).Extent().First();

            var expected = new WorkEffortFixedAssetAssignmentBuilder(this.Transaction).WithDefaults(workEffort.Customer, workEffort).Build();

            this.Transaction.Derive();

            var expectedFromDate = expected.FromDate;
            var expectedAsset = expected.FixedAsset;
            var expectedComment = expected.Comment;

            this.workEffortListPage.Table.DefaultAction(workEffort);
            var workEffortFixedAssestAssignmentComponent = new WorkTaskOverviewComponent(this.workEffortListPage.Driver, this.M).WorkeffortfixedassetassignmentOverviewPanel.Click().CreateWorkEffortFixedAssetAssignment();

            workEffortFixedAssestAssignmentComponent
                .FromDate.Set(expected.FromDate)
                .Comment.Set(expected.Comment)
                .WorkEffortFixedAssetAssignmentFixedAsset_1.Select(((SerialisedItem)expected.FixedAsset).DisplayName());

            this.Transaction.Rollback();
            workEffortFixedAssestAssignmentComponent.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new WorkEffortFixedAssetAssignments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedFromDate, actual.FromDate);
            Assert.Equal(expectedAsset, actual.FixedAsset);
            Assert.Equal(expectedComment, actual.Comment);
        }
    }
}

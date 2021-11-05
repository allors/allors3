// <copyright file="CustomerShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.customershipment.create;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.WorkEffortInvoiceItemAssignmentTests
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
    public class WorkEffortInvoiceItemAssignmentCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly WorkEffortListComponent workEffortListPage;

        public WorkEffortInvoiceItemAssignmentCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.workEffortListPage = this.Sidenav.NavigateToWorkEfforts();
        }

        [Fact]
        public void CreateWithNonSerialisedItem()
        {
            var before = new WorkEffortInvoiceItemAssignments(this.Transaction).Extent().ToArray();
            var workEffort = new WorkEfforts(this.Transaction).Extent().First();
           
            var item = new WorkEffortInvoiceItemBuilder(this.Transaction).WithDefaults().Build();
            var expected = new WorkEffortInvoiceItemAssignmentBuilder(this.Transaction)
                .WithWorkEffortInvoiceItem(item)
                .WithAssignment(workEffort)
                .Build();

            this.Transaction.Derive();

            var expectedItem = expected.WorkEffortInvoiceItem;

            this.workEffortListPage.Table.DefaultAction(workEffort);
            var workEffortFixedAssestAssignmentComponent = new WorkTaskOverviewComponent(this.workEffortListPage.Driver, this.M).WorkeffortinvoiceitemeassignmentOverviewPanel.Click().CreateWorkEffortInvoiceItemAssignment();

            workEffortFixedAssestAssignmentComponent
                .WorkEffortInvoiceItemInvoiceItemType_1.Select(expected.WorkEffortInvoiceItem.InvoiceItemType)
                .Amount.Set(expected.WorkEffortInvoiceItem.Amount.ToString());

            this.Transaction.Rollback();
            workEffortFixedAssestAssignmentComponent.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new WorkEffortInvoiceItemAssignments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedItem, actual.WorkEffortInvoiceItem);
        }
    }
}

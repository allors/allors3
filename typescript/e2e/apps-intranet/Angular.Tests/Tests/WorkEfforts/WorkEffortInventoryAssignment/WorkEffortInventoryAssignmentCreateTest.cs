// <copyright file="CustomerShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.customershipment.create;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.WorkEffortInventoryAssignmentTests
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
    public class WorkEffortInventoryAssignmentCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly WorkEffortListComponent workEffortListPage;

        public WorkEffortInventoryAssignmentCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.workEffortListPage = this.Sidenav.NavigateToWorkEfforts();
        }

        [Fact]
        public void CreateWithNonSerialisedItem()
        {
            var before = new WorkEffortInventoryAssignments(this.Transaction).Extent().ToArray();
            var workEffort = new WorkEfforts(this.Transaction).Extent().First();
            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");

            var expected = new WorkEffortInventoryAssignmentBuilder(this.Transaction).WithDefaults(internalOrganisation, workEffort).Build();

            this.Transaction.Derive();

            var expectedQuantity = expected.Quantity;
            var expectedItem = expected.InventoryItem;

            this.workEffortListPage.Table.DefaultAction(workEffort);
            var workEffortFixedAssestAssignmentComponent = new WorkTaskOverviewComponent(this.workEffortListPage.Driver, this.M).WorkeffortinventoryassignmentOverviewPanel.Click().CreateWorkEffortInventoryAssignment();

            workEffortFixedAssestAssignmentComponent
                .InventoryItem.Select(expected.InventoryItem)
                .Quantity.Set(expected.Quantity.ToString());

            this.Transaction.Rollback();
            workEffortFixedAssestAssignmentComponent.SAVECLOSE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new WorkEffortInventoryAssignments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedItem, actual.InventoryItem);
            Assert.Equal(expectedQuantity, actual.Quantity);
        }
    }
}

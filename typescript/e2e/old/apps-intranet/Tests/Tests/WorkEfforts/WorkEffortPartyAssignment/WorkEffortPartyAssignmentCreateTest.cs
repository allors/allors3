// <copyright file="CustomerShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.customershipment.create;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.WorkEffortsPartyAssignmentTests
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
    [Trait("Category", "WorkEfforts")]
    public class WorkEffortPartyAssignmentCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly WorkEffortListComponent workEffortListPage;

        public WorkEffortPartyAssignmentCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.workEffortListPage = this.Sidenav.NavigateToWorkEfforts();
        }

        [Fact]
        public void Create()
        {
            var before = new WorkEffortPartyAssignments(this.Transaction).Extent().ToArray();
            var workEffort = new WorkEfforts(this.Transaction).Extent().First();

            var expected = new WorkEffortPartyAssignmentBuilder(this.Transaction)
                .WithParty(workEffort.ExecutedBy.ActiveEmployees.First())
                .WithAssignment(workEffort) 
                .Build();

            this.Transaction.Derive();

            var expectedParty = expected.Party;

            this.workEffortListPage.Table.DefaultAction(workEffort);
            var workTaskComponent = new WorkTaskOverviewComponent(this.workEffortListPage.Driver, this.M).WorkeffortpartyassignmentOverviewPanel.Click().CreateWorkEffortPartyAssignment();

            workTaskComponent
                .Party.Select(expected.Party);

            this.Transaction.Rollback();
            workTaskComponent.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new WorkEffortPartyAssignments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedParty, actual.Party);
        }
    }
}

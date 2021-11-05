// <copyright file="CustomerShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.customershipment.create;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.WorkTaskTests
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
    public class WorkTaskCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly WorkEffortListComponent workEffortListPage;

        public WorkTaskCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.workEffortListPage = this.Sidenav.NavigateToWorkEfforts();
        }

        [Fact]
        public void CreateWithNonSerialisedItem()
        {
            var before = new WorkTasks(this.Transaction).Extent().ToArray();
            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");

            var expected = new WorkTaskBuilder(this.Transaction).WithScheduledInternalWork(internalOrganisation).Build();

            this.Transaction.Derive();

            var expectedName = expected.Name;
            var expectedCustomer = expected.Customer;
            var expectedScheduledStart = expected.ScheduledStart;

            var workTaskCreate = this.workEffortListPage.CreateWorkTask();
            
            workTaskCreate
                .Name.Set(expected.Name)
                .Customer.Select(expected.Customer.DisplayName())
                .ScheduledStart.Set(expected.ScheduledStart);

            this.Transaction.Rollback();
            workTaskCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new WorkTasks(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedName, actual.Name);
            Assert.Equal(expectedCustomer, actual.Customer);
            Assert.Equal(expectedScheduledStart.Date, actual.ScheduledStart.Date);
        }
    }
}

// <copyright file="PersonEmploymentEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.employment.edit;
using libs.workspace.angular.apps.src.lib.objects.person.list;
using libs.workspace.angular.apps.src.lib.objects.person.overview;

namespace Tests.PartyRelationshipTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class PersonEmploymentEditTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent people;

        private readonly Employment editPartyRelationship;

        private readonly Person employee;

        public PersonEmploymentEditTest(Fixture fixture)
            : base(fixture)
        {
            var allors = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            this.employee = new PersonBuilder(this.Transaction).WithLastName("employee").Build();

            // Delete all existing for the new one to be in the first page of the list.
            foreach (PartyRelationship partyRelationship in allors.PartyRelationshipsWhereParty)
            {
                partyRelationship.Delete();
            }

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.editPartyRelationship = new EmploymentBuilder(this.Transaction)
                .WithEmployee(this.employee)
                .WithEmployer(allors)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Login();
            this.people = this.Sidenav.NavigateToPeople();
        }

        [Fact]
        public void Create()
        {
            var before = new Employments(this.Transaction).Extent().ToArray();

            var employer = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");

            this.people.Table.DefaultAction(this.employee);
            var employmentEditComponent = new PersonOverviewComponent(this.people.Driver, this.M).PartyrelationshipOverviewPanel.Click().CreateEmployment();

            employmentEditComponent.FromDate
                .Set(DateTimeFactory.CreateDate(2018, 12, 22))
                .ThroughDate.Set(DateTimeFactory.CreateDate(2018, 12, 22).AddYears(1))
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Employments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var partyRelationship = after.Except(before).First();

            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, partyRelationship.FromDate.Date.ToUniversalTime().Date);
            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).AddYears(1).Date, partyRelationship.ThroughDate.Value.Date.ToUniversalTime().Date);
            Assert.Equal(employer, partyRelationship.Employer);
            Assert.Equal(this.employee, partyRelationship.Employee);
        }

        [Fact]
        public void Edit()
        {
            var before = new Employments(this.Transaction).Extent().ToArray();

            var employer = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");

            this.people.Table.DefaultAction(this.employee);
            var personOverviewPage = new PersonOverviewComponent(this.people.Driver, this.M);

            var partyRelationshipOverview = personOverviewPage.PartyrelationshipOverviewPanel.Click();
            partyRelationshipOverview.Table.DefaultAction(this.editPartyRelationship);

            var employmentEditComponent = new EmploymentEditComponent(this.Driver, this.M);
            employmentEditComponent.FromDate.Set(DateTimeFactory.CreateDate(2018, 12, 22))
                .ThroughDate.Set(DateTimeFactory.CreateDate(2018, 12, 22).AddYears(1))
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Employments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, this.editPartyRelationship.FromDate.Date.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).AddYears(1).Date, this.editPartyRelationship.ThroughDate.Value.Date.ToUniversalTime().Date);
            Assert.Equal(employer, this.editPartyRelationship.Employer);
            Assert.Equal(this.employee, this.editPartyRelationship.Employee);
        }
    }
}

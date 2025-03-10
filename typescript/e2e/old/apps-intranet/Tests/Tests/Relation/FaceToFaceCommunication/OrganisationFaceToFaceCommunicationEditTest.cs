// <copyright file="OrganisationFaceToFaceCommunicationEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.facetofacecommunication.edit;
using libs.workspace.angular.apps.src.lib.objects.organisation.list;
using libs.workspace.angular.apps.src.lib.objects.organisation.overview;

namespace Tests.FaceToFaceCommunicationTests
{
    using System.Linq;
    using Allors;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class OrganisationFaceToFaceCommunicationEditTest : Test, IClassFixture<Fixture>
    {
        private readonly OrganisationListComponent organisationListPage;

        public OrganisationFaceToFaceCommunicationEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.organisationListPage = this.Sidenav.NavigateToOrganisations();
        }

        [Fact]
        public void Edit()
        {
            var allors = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var firstEmployee = allors.ActiveEmployees.First();

            var secondEmployee = allors.CreateEmployee("letmein", this.Transaction.Faker()); //second employee
            this.Transaction.Derive();

            var organisation = allors.ActiveCustomers.First(v => v.GetType().Name == typeof(Organisation).Name);
            var contact = organisation.CurrentContacts.FirstOrDefault();

            var editCommunicationEvent = new FaceToFaceCommunicationBuilder(this.Transaction)
                .WithSubject("dummy")
                .WithFromParty(organisation.CurrentContacts.FirstOrDefault())
                .WithToParty(firstEmployee)
                .WithLocation("old location")
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var before = new FaceToFaceCommunications(this.Transaction).Extent().ToArray();

            this.organisationListPage.Table.DefaultAction(organisation);
            var organisationOverview = new OrganisationOverviewComponent(this.organisationListPage.Driver, this.M);

            var communicationEventOverview = organisationOverview.CommunicationeventOverviewPanel.Click();
            communicationEventOverview.Table.DefaultAction(editCommunicationEvent);

            var faceToFaceCommunicationEdit = new FaceToFaceCommunicationEditComponent(organisationOverview.Driver, this.M);
            faceToFaceCommunicationEdit
                .CommunicationEventState.Select(new CommunicationEventStates(this.Transaction).Completed)
                .EventPurposes.Toggle(new CommunicationEventPurposes(this.Transaction).Conference)
                .Location.Set("new location")
                .Subject.Set("new subject")
                .FromParty.Select(secondEmployee)
                .ToParty.Select(contact)
                .ScheduledStart.Set(DateTimeFactory.CreateDate(2018, 12, 24))
                .ScheduledEnd.Set(DateTimeFactory.CreateDate(2018, 12, 24))
                .ActualStart.Set(DateTimeFactory.CreateDate(2018, 12, 24))
                .ActualEnd.Set(DateTimeFactory.CreateDate(2018, 12, 24))
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new FaceToFaceCommunications(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(new CommunicationEventStates(this.Transaction).Completed, editCommunicationEvent.CommunicationEventState);
            Assert.Contains(new CommunicationEventPurposes(this.Transaction).Conference, editCommunicationEvent.EventPurposes);
            Assert.Equal(secondEmployee, editCommunicationEvent.FromParty);
            Assert.Equal(contact, editCommunicationEvent.ToParty);
            Assert.Equal("new location", editCommunicationEvent.Location);
            Assert.Equal("new subject", editCommunicationEvent.Subject);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 24).Date, editCommunicationEvent.ScheduledStart);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 24).Date, editCommunicationEvent.ScheduledEnd.Value.Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 24).Date, editCommunicationEvent.ActualStart.Value.Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 24).Date, editCommunicationEvent.ActualEnd.Value.Date);
        }
    }
}

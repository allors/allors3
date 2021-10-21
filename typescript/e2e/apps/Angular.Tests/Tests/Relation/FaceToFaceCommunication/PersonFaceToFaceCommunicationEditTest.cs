// <copyright file="PersonFaceToFaceCommunicationEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.facetofacecommunication.edit;
using libs.workspace.angular.apps.src.lib.objects.person.list;
using libs.workspace.angular.apps.src.lib.objects.person.overview;

namespace Tests.FaceToFaceCommunicationTests
{
    using System.Linq;
    using Allors;
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class PersonFaceToFaceCommunicationEditTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent personListPage;

        public PersonFaceToFaceCommunicationEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.personListPage = this.Sidenav.NavigateToPeople();
        }

        [Fact]
        public void Edit()
        {
            var faker = this.Transaction.Faker();
            var subject = faker.Lorem.Sentence();
            var location = faker.Address.FullAddress();

            var person = new People(this.Transaction).Extent().FirstOrDefault();

            var allors = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var firstEmployee = allors.ActiveEmployees.First();
            var secondEmployee = allors.ActiveEmployees.Last();

            var editCommunicationEvent = new FaceToFaceCommunicationBuilder(this.Transaction)
                .WithSubject(subject)
                .WithFromParty(person)
                .WithToParty(firstEmployee)
                .WithLocation(location)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var before = new FaceToFaceCommunications(this.Transaction).Extent().ToArray();

            this.personListPage.Table.DefaultAction(person);
            var personOverview = new PersonOverviewComponent(this.personListPage.Driver, this.M);

            var communicationEventOverview = personOverview.CommunicationeventOverviewPanel.Click();
            communicationEventOverview.Table.DefaultAction(editCommunicationEvent);

            var faceToFaceCommunicationEditComponent = new FaceToFaceCommunicationEditComponent(this.Driver, this.M);

            var scheduleStartDate = DateTimeFactory.CreateDate(2018, 12, 24);
            var scheduleEndDate = DateTimeFactory.CreateDate(2018, 12, 24);
            var actualStartDate = DateTimeFactory.CreateDate(2018, 12, 24);
            var actualEndDate = DateTimeFactory.CreateDate(2018, 12, 24);

            faceToFaceCommunicationEditComponent
                .CommunicationEventState.Select(new CommunicationEventStates(this.Transaction).Completed)
                .EventPurposes.Toggle(new CommunicationEventPurposes(this.Transaction).Conference)
                .Subject.Set(subject)
                .Location.Set(location)
                .FromParty.Select(secondEmployee)
                .ToParty.Select(person)
                .ScheduledStart.Set(scheduleStartDate)
                .ScheduledEnd.Set(scheduleEndDate)
                .ActualStart.Set(actualStartDate)
                .ActualEnd.Set(actualEndDate)
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new FaceToFaceCommunications(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(new CommunicationEventStates(this.Transaction).Completed, editCommunicationEvent.CommunicationEventState);
            Assert.Contains(new CommunicationEventPurposes(this.Transaction).Conference, editCommunicationEvent.EventPurposes);
            Assert.Equal(secondEmployee, editCommunicationEvent.FromParty);
            Assert.Equal(person, editCommunicationEvent.ToParty);
            Assert.Equal(subject, editCommunicationEvent.Subject);
            Assert.Equal(location, editCommunicationEvent.Location);
            Assert.Equal(scheduleStartDate.Date, editCommunicationEvent.ScheduledStart.Value.Date);
            Assert.Equal(scheduleEndDate.Date, editCommunicationEvent.ScheduledEnd.Value.Date);
            Assert.Equal(actualStartDate.Date, editCommunicationEvent.ActualStart.Value.Date);
            Assert.Equal(actualEndDate.Date, editCommunicationEvent.ActualEnd.Value.Date);
        }
    }
}

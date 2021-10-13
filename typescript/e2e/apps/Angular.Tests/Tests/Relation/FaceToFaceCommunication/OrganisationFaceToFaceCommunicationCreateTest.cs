// <copyright file="OrganisationFaceToFaceCommunicationCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.organisation.list;
using libs.workspace.angular.apps.src.lib.objects.organisation.overview;

namespace Tests.FaceToFaceCommunicationTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class OrganisationFaceToFaceCommunicationCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly OrganisationListComponent organisationListPage;

        public OrganisationFaceToFaceCommunicationCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.organisationListPage = this.Sidenav.NavigateToOrganisations();
        }

        [Fact]
        public void Create()
        {
            var allors = new Organisations(this.Session).FindBy(M.Organisation.Name, "Allors BVBA");
            var employee = allors.ActiveEmployees.FirstOrDefault();

            var before = new FaceToFaceCommunications(this.Session).Extent().ToArray();

            var organisation = allors.ActiveCustomers.First(v => v.GetType().Name == typeof(Organisation).Name);
            var contact = organisation.CurrentContacts.FirstOrDefault();

            this.organisationListPage.Table.DefaultAction(organisation);
            var faceToFaceCommunicationEdit = new OrganisationOverviewComponent(this.organisationListPage.Driver, this.M).CommunicationeventOverviewPanel.Click().CreateFaceToFaceCommunication();

            faceToFaceCommunicationEdit
                .CommunicationEventState.Select(new CommunicationEventStates(this.Session).Completed)
                .EventPurposes.Toggle(new CommunicationEventPurposes(this.Session).Appointment)
                .Location.Set("location")
                .Subject.Set("subject")
                .FromParty.Select(employee)
                .ToParty.Select(contact)
                .ScheduledStart.Set(DateTimeFactory.CreateDate(2018, 12, 22))
                .ScheduledEnd.Set(DateTimeFactory.CreateDate(2018, 12, 22))
                .ActualStart.Set(DateTimeFactory.CreateDate(2018, 12, 23))
                .ActualEnd.Set(DateTimeFactory.CreateDate(2018, 12, 23))
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Session.Rollback();

            var after = new FaceToFaceCommunications(this.Session).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var communicationEvent = after.Except(before).First();

            Assert.Equal(new CommunicationEventStates(this.Session).Completed, communicationEvent.CommunicationEventState);
            Assert.Contains(new CommunicationEventPurposes(this.Session).Appointment, communicationEvent.EventPurposes);
            Assert.Equal(employee, communicationEvent.FromParty);
            Assert.Equal(contact, communicationEvent.ToParty);
            Assert.Equal("location", communicationEvent.Location);
            Assert.Equal("subject", communicationEvent.Subject);
            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, communicationEvent.ScheduledStart.Value.ToUniversalTime().Date);
            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, communicationEvent.ScheduledEnd.Value.Date.ToUniversalTime().Date);
            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 23).Date, communicationEvent.ActualStart.Value.Date.ToUniversalTime().Date);
            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 23).Date, communicationEvent.ActualEnd.Value.Date.ToUniversalTime().Date);
        }
    }
}

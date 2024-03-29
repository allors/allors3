// <copyright file="OrganisationEmailCommunicationCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.organisation.list;
using libs.workspace.angular.apps.src.lib.objects.organisation.overview;

namespace Tests.EmailCommunicationTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class OrganisationEmailCommunicationCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly OrganisationListComponent organisationListPage;

        public OrganisationEmailCommunicationCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.organisationListPage = this.Sidenav.NavigateToOrganisations();
        }

        [Fact]
        public void Create()
        {
            var allors = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var employee = allors.ActiveEmployees.First();

            var organisation = allors.ActiveCustomers.First(v => v.GetType().Name == typeof(Organisation).Name);
            var contact = organisation.CurrentContacts.FirstOrDefault();

            var employeeEmailAddress = employee.PersonalEmailAddress;
            var personEmailAddress = organisation.CurrentContacts.First().PersonalEmailAddress;

            var before = new EmailCommunications(this.Transaction).Extent().ToArray();

            this.organisationListPage.Table.DefaultAction(organisation);
            var organisationOverview = new OrganisationOverviewComponent(this.organisationListPage.Driver, this.M);
            var communicationEventOverview = organisationOverview.CommunicationeventOverviewPanel.Click();
            var emailCommunicationCreate = communicationEventOverview.CreateEmailCommunication();

            emailCommunicationCreate
                .CommunicationEventState.Select(new CommunicationEventStates(this.Transaction).Completed)
                .EventPurposes.Toggle(new CommunicationEventPurposes(this.Transaction).Appointment)
                .FromParty.Select(employee)
                .FromEmail.Select(employeeEmailAddress)
                .ToParty.Select(contact)
                .ToEmail.Select(personEmailAddress)
                .SubjectTemplate.Set("subject")
                .BodyTemplate.Set("body")
                .ScheduledStart.Set(DateTimeFactory.CreateDate(2018, 12, 22))
                .ScheduledEnd.Set(DateTimeFactory.CreateDate(2018, 12, 22))
                .ActualStart.Set(DateTimeFactory.CreateDate(2018, 12, 23))
                .ActualEnd.Set(DateTimeFactory.CreateDate(2018, 12, 23))
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new EmailCommunications(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var communicationEvent = after.Except(before).First();

            Assert.Equal(new CommunicationEventStates(this.Transaction).Completed, communicationEvent.CommunicationEventState);
            Assert.Contains(new CommunicationEventPurposes(this.Transaction).Appointment, communicationEvent.EventPurposes);
            Assert.Equal(employee, communicationEvent.FromParty);
            Assert.Equal(employeeEmailAddress, communicationEvent.FromEmail);
            Assert.Equal(contact, communicationEvent.ToParty);
            Assert.Equal(personEmailAddress, communicationEvent.ToEmail);
            Assert.Equal("subject", communicationEvent.EmailTemplate.SubjectTemplate);
            Assert.Equal("body", communicationEvent.EmailTemplate.BodyTemplate);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, communicationEvent.ScheduledStart.Value.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, communicationEvent.ScheduledEnd.Value.Date.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 23).Date, communicationEvent.ActualStart.Value.Date.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 23).Date, communicationEvent.ActualEnd.Value.Date.ToUniversalTime().Date);
        }
    }
}

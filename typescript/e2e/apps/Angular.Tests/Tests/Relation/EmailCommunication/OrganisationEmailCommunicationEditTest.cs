// <copyright file="OrganisationEmailCommunicationEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.emailcommunication.edit;
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
    public class OrganisationEmailCommunicationEditTest : Test, IClassFixture<Fixture>
    {
        private readonly OrganisationListComponent organisationListPage;

        public OrganisationEmailCommunicationEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.organisationListPage = this.Sidenav.NavigateToOrganisations();
        }

        [Fact]
        public void Edit()
        {
            var allors = new Organisations(this.Session).FindBy(M.Organisation.Name, "Allors BVBA");
            var employee = allors.ActiveEmployees.First();

            var organisation = allors.ActiveCustomers.First(v => v.GetType().Name == typeof(Organisation).Name);
            var contact = organisation.CurrentContacts.FirstOrDefault();

            var employeeEmailAddress = employee.PersonalEmailAddress;
            var personEmailAddress = organisation.CurrentContacts.First().PersonalEmailAddress;

            var editCommunicationEvent = new EmailCommunicationBuilder(this.Session)
                .WithSubject("dummy")
                .WithFromParty(employee)
                .WithFromEmail(employeeEmailAddress)
                .WithToParty(organisation.CurrentContacts.FirstOrDefault())
                .WithToEmail(personEmailAddress)
                .WithEmailTemplate(new EmailTemplateBuilder(this.Session).Build())
                .Build();

            this.Session.Derive();
            this.Session.Commit();

            var before = new EmailCommunications(this.Session).Extent().ToArray();

            this.organisationListPage.Table.DefaultAction(organisation);
            var organisationOverview = new OrganisationOverviewComponent(this.organisationListPage.Driver, this.M);

            var communicationEventOverview = organisationOverview.CommunicationeventOverviewPanel.Click();
            communicationEventOverview.Table.DefaultAction(editCommunicationEvent);
            
            var emailCommunicationEdit = new EmailCommunicationEditComponent(organisationOverview.Driver, this.M);
            emailCommunicationEdit
                .CommunicationEventState.Select(new CommunicationEventStates(this.Session).Completed)
                .EventPurposes.Toggle(new CommunicationEventPurposes(this.Session).Inquiry)
                .FromParty.Select(contact)
                .FromEmail.Select(personEmailAddress)
                .ToParty.Select(employee)
                .ToEmail.Select(employeeEmailAddress)
                .SubjectTemplate.Set("new subject")
                .BodyTemplate.Set("new body")
                .ScheduledStart.Set(DateTimeFactory.CreateDate(2018, 12, 24))
                .ScheduledEnd.Set(DateTimeFactory.CreateDate(2018, 12, 24))
                .ActualStart.Set(DateTimeFactory.CreateDate(2018, 12, 24))
                .ActualEnd.Set(DateTimeFactory.CreateDate(2018, 12, 24))
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Session.Rollback();

            var after = new EmailCommunications(this.Session).Extent().ToArray();

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(new CommunicationEventStates(this.Session).Completed, editCommunicationEvent.CommunicationEventState);
            Assert.Contains(new CommunicationEventPurposes(this.Session).Inquiry, editCommunicationEvent.EventPurposes);
            Assert.Equal(contact, editCommunicationEvent.FromParty);
            Assert.Equal(personEmailAddress, editCommunicationEvent.FromEmail);
            Assert.Equal(employee, editCommunicationEvent.ToParty);
            Assert.Equal(employeeEmailAddress, editCommunicationEvent.ToEmail);
            Assert.Equal("new subject", editCommunicationEvent.EmailTemplate.SubjectTemplate);
            Assert.Equal("new body", editCommunicationEvent.EmailTemplate.BodyTemplate);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 24).Date, editCommunicationEvent.ScheduledStart);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 24).Date, editCommunicationEvent.ScheduledEnd.Value.Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 24).Date, editCommunicationEvent.ActualStart.Value.Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 24).Date, editCommunicationEvent.ActualEnd.Value.Date);
        }
    }
}

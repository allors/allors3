// <copyright file="OrganisationPhoneCommunicationEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.organisation.list;
using libs.workspace.angular.apps.src.lib.objects.organisation.overview;
using libs.workspace.angular.apps.src.lib.objects.phonecommunication.edit;

namespace Tests.PhoneCommunicationTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class OrganisationPhoneCommunicationEditTest : Test, IClassFixture<Fixture>
    {
        private readonly OrganisationListComponent organisations;

        private readonly PartyContactMechanism organisationPhoneNumber;

        private readonly PhoneCommunication editCommunicationEvent;

        public OrganisationPhoneCommunicationEditTest(Fixture fixture)
            : base(fixture)
        {
            var allors = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var firstEmployee = allors.ActiveEmployees.First();
            var organisation = allors.ActiveCustomers.FirstOrDefault();

            this.editCommunicationEvent = new PhoneCommunicationBuilder(this.Transaction)
                .WithSubject("dummy")
                .WithLeftVoiceMail(true)
                .WithFromParty(firstEmployee)
                .WithToParty(organisation.CurrentContacts.FirstOrDefault())
                .WithPhoneNumber(organisation.GeneralPhoneNumber)
                .Build();

            this.organisationPhoneNumber = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(new TelecommunicationsNumberBuilder(this.Transaction)
                    .WithCountryCode("+1")
                    .WithAreaCode("111")
                    .WithContactNumber("222")
                    .Build())
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).SalesOffice)
                .WithUseAsDefault(false)
                .Build();

            organisation.AddPartyContactMechanism(this.organisationPhoneNumber);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Login();
            this.organisations = this.Sidenav.NavigateToOrganisations();
        }

        [Fact]
        public void Edit()
        {
            var allors = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var firstEmployee = allors.ActiveEmployees.First();
            var organisation = allors.ActiveCustomers.FirstOrDefault();

            var before = new PhoneCommunications(this.Transaction).Extent().ToArray();

            this.organisations.Table.DefaultAction(allors);
            var personOverview = new OrganisationOverviewComponent(this.organisations.Driver, this.M);

            var communicationEventOverview = personOverview.CommunicationeventOverviewPanel.Click();
            communicationEventOverview.Table.DefaultAction(this.editCommunicationEvent);

            var phoneCommunicationEdit = new PhoneCommunicationEditComponent(personOverview.Driver, this.M);
            phoneCommunicationEdit
                .LeftVoiceMail.Set(false)
                .CommunicationEventState.Select(new CommunicationEventStates(this.Transaction).Completed)
                .EventPurposes.Toggle(new CommunicationEventPurposes(this.Transaction).Inquiry)
                .FromParty.Select(organisation)
                .ToParty.Select(firstEmployee)
                .FromPhoneNumber.Select(this.organisationPhoneNumber.ContactMechanism)
                .Subject.Set("new subject")
                .ScheduledStart.Set(DateTimeFactory.CreateDate(2018, 12, 23))
                .ScheduledEnd.Set(DateTimeFactory.CreateDate(2018, 12, 23))
                .ActualStart.Set(DateTimeFactory.CreateDate(2018, 12, 24))
                .ActualEnd.Set(DateTimeFactory.CreateDate(2018, 12, 25))
                .Comment.Set("new comment")
            .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PhoneCommunications(this.Transaction).Extent().ToArray();
            Assert.Equal(after.Length, before.Length);

            Assert.False(this.editCommunicationEvent.LeftVoiceMail);
            Assert.Equal(new CommunicationEventStates(this.Transaction).Completed, this.editCommunicationEvent.CommunicationEventState);
            Assert.Contains(new CommunicationEventPurposes(this.Transaction).Inquiry, this.editCommunicationEvent.EventPurposes);
            Assert.Equal(organisation, this.editCommunicationEvent.FromParty);
            Assert.Equal(firstEmployee, this.editCommunicationEvent.ToParty);
            Assert.Equal(this.organisationPhoneNumber.ContactMechanism, this.editCommunicationEvent.PhoneNumber);
            Assert.Equal("new subject", this.editCommunicationEvent.Subject);
            Assert.Equal("new comment", this.editCommunicationEvent.Comment);
        }
    }
}

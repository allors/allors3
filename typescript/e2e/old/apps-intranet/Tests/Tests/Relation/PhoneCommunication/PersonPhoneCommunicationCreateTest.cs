// <copyright file="PersonPhoneCommunicationCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.person.list;
using libs.workspace.angular.apps.src.lib.objects.person.overview;

namespace Tests.PhoneCommunicationTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class PersonPhoneCommunicationCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent people;

        private readonly PartyContactMechanism anotherPhoneNumber;

        private readonly PhoneCommunication editCommunicationEvent;

        public PersonPhoneCommunicationCreateTest(Fixture fixture)
            : base(fixture)
        {
            var person = new People(this.Transaction).Extent().FirstOrDefault();

            var allors = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var firstEmployee = allors.ActiveEmployees.First();

            this.editCommunicationEvent = new PhoneCommunicationBuilder(this.Transaction)
                .WithSubject("dummy")
                .WithLeftVoiceMail(true)
                .WithFromParty(person)
                .WithToParty(firstEmployee)
                .WithPhoneNumber(person.GeneralPhoneNumber)
                .Build();

            this.anotherPhoneNumber = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(new TelecommunicationsNumberBuilder(this.Transaction).WithCountryCode("+1").WithAreaCode("111").WithContactNumber("222").Build())
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).SalesOffice)
                .WithUseAsDefault(false)
                .Build();

            person.AddPartyContactMechanism(this.anotherPhoneNumber);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Login();
            this.people = this.Sidenav.NavigateToPeople();
        }

        [Fact]
        public void Create()
        {
            var allors = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var employee = allors.ActiveEmployees.FirstOrDefault();

            var before = new PhoneCommunications(this.Transaction).Extent().ToArray();

            var person = new People(this.Transaction).Extent().FirstOrDefault();

            this.people.Table.DefaultAction(person);
            var communicationEventOverview = new PersonOverviewComponent(this.people.Driver, this.M).CommunicationeventOverviewPanel.Click();

            communicationEventOverview
                .CreatePhoneCommunication()
                .LeftVoiceMail.Set(true)
                .CommunicationEventState.Select(new CommunicationEventStates(this.Transaction).Completed)
                .EventPurposes.Toggle(new CommunicationEventPurposes(this.Transaction).Inquiry)
                .Subject.Set("subject")
                .FromParty.Select(person)
                .ToParty.Select(employee)
                .FromPhoneNumber.Select(person.GeneralPhoneNumber)
                .ScheduledStart.Set(DateTimeFactory.CreateDate(2018, 12, 22))
                .ScheduledEnd.Set(DateTimeFactory.CreateDate(2018, 12, 22))
                .ActualStart.Set(DateTimeFactory.CreateDate(2018, 12, 23))
                .ActualEnd.Set(DateTimeFactory.CreateDate(2018, 12, 23))
                .Comment.Set("comment")
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PhoneCommunications(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var communicationEvent = after.Except(before).First();

            Assert.True(communicationEvent.LeftVoiceMail);
            Assert.Equal(new CommunicationEventStates(this.Transaction).Completed, communicationEvent.CommunicationEventState);
            Assert.Contains(new CommunicationEventPurposes(this.Transaction).Inquiry, communicationEvent.EventPurposes);
            Assert.Equal(person, communicationEvent.FromParty);
            Assert.Equal(employee, communicationEvent.ToParty);
            Assert.Equal(person.GeneralPhoneNumber, communicationEvent.PhoneNumber);
            Assert.Equal("subject", communicationEvent.Subject);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, communicationEvent.ScheduledStart.Value.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, communicationEvent.ScheduledEnd.Value.Date.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 23).Date, communicationEvent.ActualStart.Value.Date.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 23).Date, communicationEvent.ActualEnd.Value.Date.ToUniversalTime().Date);
            Assert.Equal("comment", communicationEvent.Comment);
        }
    }
}

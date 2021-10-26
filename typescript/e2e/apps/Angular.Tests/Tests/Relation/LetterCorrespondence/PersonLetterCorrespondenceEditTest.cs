// <copyright file="PersonLetterCorrespondenceEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.lettercorrespondence.edit;
using libs.workspace.angular.apps.src.lib.objects.person.list;
using libs.workspace.angular.apps.src.lib.objects.person.overview;

namespace Tests.LetterCorrespondenceTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class PersonLetterCorrespondenceEditTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent personListPage;

        public PersonLetterCorrespondenceEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.personListPage = this.Sidenav.NavigateToPeople();
        }

        [Fact]
        public void Edit()
        {
            var person = new People(this.Transaction).Extent().FirstOrDefault();

            var allors = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var employee = allors.ActiveEmployees.First();

            var address = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("city")
                .WithPostalCode("1111")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
                .Build();

            person.AddPartyContactMechanism(new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(address).Build());

            var employeeAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("home sweet home")
                .WithLocality("suncity")
                .WithPostalCode("0000")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
                .Build();

            employee.AddPartyContactMechanism(new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(employeeAddress).Build());

            var editCommunicationEvent = new LetterCorrespondenceBuilder(this.Transaction)
                .WithSubject("dummy")
                .WithFromParty(employee)
                .WithToParty(person)
                .WithPostalAddress(address)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var before = new LetterCorrespondences(this.Transaction).Extent().ToArray();

            var postalAddress = (PostalAddress)person.PartyContactMechanisms.First(v => v.ContactMechanism.GetType().Name == typeof(PostalAddress).Name).ContactMechanism;

            this.personListPage.Table.DefaultAction(person);
            var personOverview = new PersonOverviewComponent(this.personListPage.Driver, this.M);

            var communicationEventOverview = personOverview.CommunicationeventOverviewPanel.Click();
            communicationEventOverview.Table.DefaultAction(editCommunicationEvent);

            var letterCorrespondenceEditComponent = new LetterCorrespondenceEditComponent(this.Driver, this.M);
            letterCorrespondenceEditComponent
                .CommunicationEventState.Select(new CommunicationEventStates(this.Transaction).InProgress)
                .EventPurposes.Toggle(new CommunicationEventPurposes(this.Transaction).Appointment)
                .FromParty.Select(person)
                .ToParty.Select(employee)
                .FromPostalAddress.Select(postalAddress)
                .Subject.Set("new subject")
                .ScheduledStart.Set(DateTimeFactory.CreateDate(2018, 12, 23))
                .ScheduledEnd.Set(DateTimeFactory.CreateDate(2018, 12, 23))
                .ActualStart.Set(DateTimeFactory.CreateDate(2018, 12, 24))
                .ActualEnd.Set(DateTimeFactory.CreateDate(2018, 12, 24))
                .Comment.Set("new comment")
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new LetterCorrespondences(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(new CommunicationEventStates(this.Transaction).Completed, editCommunicationEvent.CommunicationEventState);
            Assert.Contains(new CommunicationEventPurposes(this.Transaction).Appointment, editCommunicationEvent.EventPurposes);
            Assert.Equal(person, editCommunicationEvent.FromParty);
            Assert.Equal(employee, editCommunicationEvent.ToParty);
            Assert.Equal(postalAddress, editCommunicationEvent.PostalAddress);
            Assert.Equal("new subject", editCommunicationEvent.Subject);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 23).Date, editCommunicationEvent.ScheduledStart.Value.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 23).Date, editCommunicationEvent.ScheduledEnd.Value.Date.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 24).Date, editCommunicationEvent.ActualStart.Value.Date.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 24).Date, editCommunicationEvent.ActualEnd.Value.Date.ToUniversalTime().Date);
            Assert.Equal("new comment", editCommunicationEvent.Comment);
        }
    }
}

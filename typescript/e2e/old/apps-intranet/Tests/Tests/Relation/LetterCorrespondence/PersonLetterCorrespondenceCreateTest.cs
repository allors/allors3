// <copyright file="PersonLetterCorrespondenceCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

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
    public class PersonLetterCorrespondenceCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent personListPage;

        public PersonLetterCorrespondenceCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.personListPage = this.Sidenav.NavigateToPeople();
        }

        [Fact]
        public void Create()
        {
            var person = new People(this.Transaction).Extent().FirstOrDefault();

            var address = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("city")
                .WithPostalCode("1111")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
                .Build();

            person.AddPartyContactMechanism(new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(address).Build());

            var allors = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var employee = allors.ActiveEmployees.FirstOrDefault();

            var employeeAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("home sweet home")
                .WithLocality("suncity")
                .WithPostalCode("0000")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
                .Build();

            employee.AddPartyContactMechanism(new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(employeeAddress).Build());

            this.Transaction.Derive();
            this.Transaction.Commit();

            var before = new LetterCorrespondences(this.Transaction).Extent().ToArray();

            this.personListPage.Table.DefaultAction(person);
            var letterCorrespondenceEdit = new PersonOverviewComponent(this.personListPage.Driver, this.M).CommunicationeventOverviewPanel.Click().CreateLetterCorrespondence();

            letterCorrespondenceEdit
                .CommunicationEventState.Select(new CommunicationEventStates(this.Transaction).Completed)
                .EventPurposes.Toggle(new CommunicationEventPurposes(this.Transaction).Appointment)
                .FromParty.Select(employee)
                .ToParty.Select(person)
                .FromPostalAddress.Select(employeeAddress)
                .Subject.Set("subject")
                .ScheduledStart.Set(DateTimeFactory.CreateDate(2018, 12, 22))
                .ScheduledEnd.Set(DateTimeFactory.CreateDate(2018, 12, 22))
                .ActualStart.Set(DateTimeFactory.CreateDate(2018, 12, 23))
                .ActualEnd.Set(DateTimeFactory.CreateDate(2018, 12, 23))
                .Comment.Set("comment")
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new LetterCorrespondences(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var communicationEvent = after.Except(before).First();

            Assert.Equal(new CommunicationEventStates(this.Transaction).Completed, communicationEvent.CommunicationEventState);
            Assert.Contains(new CommunicationEventPurposes(this.Transaction).Appointment, communicationEvent.EventPurposes);
            Assert.Equal(employeeAddress, communicationEvent.PostalAddress);
            Assert.Equal(employee, communicationEvent.FromParty);
            Assert.Equal(person, communicationEvent.ToParty);
            Assert.Equal("subject", communicationEvent.Subject);
            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, communicationEvent.ScheduledStart.Value.ToUniversalTime().Date);
            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, communicationEvent.ScheduledEnd.Value.Date.ToUniversalTime().Date);
            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 23).Date, communicationEvent.ActualStart.Value.Date.ToUniversalTime().Date);
            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 23).Date, communicationEvent.ActualEnd.Value.Date.ToUniversalTime().Date);
            Assert.Equal("comment", communicationEvent.Comment);
        }
    }
}

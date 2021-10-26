// <copyright file="OrganisationLetterCorrespondenceEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.lettercorrespondence.edit;
using libs.workspace.angular.apps.src.lib.objects.organisation.list;
using libs.workspace.angular.apps.src.lib.objects.organisation.overview;

namespace Tests.LetterCorrespondenceTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class OrganisationLetterCorrespondenceEditTest : Test, IClassFixture<Fixture>
    {
        private readonly OrganisationListComponent organisationListPage;

        public OrganisationLetterCorrespondenceEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.organisationListPage = this.Sidenav.NavigateToOrganisations();
        }

        [Fact]
        public void Edit()
        {
            var allors = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var employee = allors.ActiveEmployees.First();

            var organisation = allors.ActiveCustomers.First(v => v.GetType().Name == typeof(Organisation).Name);

            var organisationAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("city")
                .WithPostalCode("1111")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
                .Build();

            organisation.AddPartyContactMechanism(new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(organisationAddress).Build());

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
                .WithToParty(organisation)
                .WithPostalAddress(organisationAddress)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var before = new LetterCorrespondences(this.Transaction).Extent().ToArray();

            this.organisationListPage.Table.DefaultAction(organisation);
            var organisationOverviewPage = new OrganisationOverviewComponent(this.organisationListPage.Driver, this.M);

            var communicationEventOverview = organisationOverviewPage.CommunicationeventOverviewPanel.Click();
            communicationEventOverview.Table.DefaultAction(editCommunicationEvent);

            var letterCorrespondenceEdit = new LetterCorrespondenceEditComponent(organisationOverviewPage.Driver, this.M);
            letterCorrespondenceEdit
                .CommunicationEventState.Select(new CommunicationEventStates(this.Transaction).InProgress)
                .EventPurposes.Toggle(new CommunicationEventPurposes(this.Transaction).Appointment)
                .FromParty.Select(organisation)
                .ToParty.Select(employee)
                .FromPostalAddress.Select(organisationAddress)
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
            Assert.Equal(organisation, editCommunicationEvent.FromParty);
            Assert.Equal(employee, editCommunicationEvent.ToParty);
            Assert.Equal(organisationAddress, editCommunicationEvent.PostalAddress);
            Assert.Equal("new subject", editCommunicationEvent.Subject);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 23).Date, editCommunicationEvent.ScheduledStart.Value.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 23).Date, editCommunicationEvent.ScheduledEnd.Value.Date.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 24).Date, editCommunicationEvent.ActualStart.Value.Date.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 24).Date, editCommunicationEvent.ActualEnd.Value.Date.ToUniversalTime().Date);
            Assert.Equal("new comment", editCommunicationEvent.Comment);
        }
    }
}

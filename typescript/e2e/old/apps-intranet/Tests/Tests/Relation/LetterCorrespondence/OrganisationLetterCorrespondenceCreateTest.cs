// <copyright file="OrganisationLetterCorrespondenceCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

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
    public class OrganisationLetterCorrespondenceCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly OrganisationListComponent organisationListPage;

        public OrganisationLetterCorrespondenceCreateTest(Fixture fixture)
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

            var organisationAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("city")
                .WithPostalCode("1111")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
                .Build();

            organisation.AddPartyContactMechanism(new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(organisationAddress).Build());

            this.Transaction.Derive();
            this.Transaction.Commit();

            var before = new LetterCorrespondences(this.Transaction).Extent().ToArray();

            this.organisationListPage.Table.DefaultAction(organisation);
            var letterCorrespondenceEdit = new OrganisationOverviewComponent(this.organisationListPage.Driver, this.M).CommunicationeventOverviewPanel.Click().CreateLetterCorrespondence();

            letterCorrespondenceEdit
                .CommunicationEventState.Select(new CommunicationEventStates(this.Transaction).Completed)
                .EventPurposes.Toggle(new CommunicationEventPurposes(this.Transaction).Appointment)
                .FromParty.Select(organisation)
                .ToParty.Select(employee)
                .FromPostalAddress.Select(organisationAddress)
                .Subject.Set("subject");
            letterCorrespondenceEdit.ScheduledStart.Set(DateTimeFactory.CreateDate(2018, 12, 22));
            letterCorrespondenceEdit.ScheduledEnd.Set(DateTimeFactory.CreateDate(2018, 12, 22));
            letterCorrespondenceEdit.ActualStart.Set(DateTimeFactory.CreateDate(2018, 12, 23));
            letterCorrespondenceEdit.ActualEnd.Set(DateTimeFactory.CreateDate(2018, 12, 23));
            letterCorrespondenceEdit.Comment.Set("comment");
                            letterCorrespondenceEdit.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new LetterCorrespondences(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var communicationEvent = after.Except(before).First();

            Assert.Equal(new CommunicationEventStates(this.Transaction).Completed, communicationEvent.CommunicationEventState);
            Assert.Contains(new CommunicationEventPurposes(this.Transaction).Appointment, communicationEvent.EventPurposes);
            Assert.Equal(organisationAddress, communicationEvent.PostalAddress);
            Assert.Equal(organisation, communicationEvent.FromParty);
            Assert.Equal(employee, communicationEvent.ToParty);
            Assert.Equal("subject", communicationEvent.Subject);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, communicationEvent.ScheduledStart.Value.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, communicationEvent.ScheduledEnd.Value.Date.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 23).Date, communicationEvent.ActualStart.Value.Date.ToUniversalTime().Date);
            Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 23).Date, communicationEvent.ActualEnd.Value.Date.ToUniversalTime().Date);
            Assert.Equal("comment", communicationEvent.Comment);
        }
    }
}

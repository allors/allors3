// <copyright file="PersonOrganisationContactRelationshipEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.organisationcontactrelationship.edit;
using libs.workspace.angular.apps.src.lib.objects.person.list;
using libs.workspace.angular.apps.src.lib.objects.person.overview;

namespace Tests.PartyRelationshipTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class PersonOrganisationContactRelationshipEditTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent people;

        private readonly OrganisationContactRelationship editPartyRelationship;

        private readonly Organisation organisation;

        private readonly Person contact;

        public PersonOrganisationContactRelationshipEditTest(Fixture fixture)
            : base(fixture)
        {
            this.organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").Build();
            this.contact = new PersonBuilder(this.Transaction).WithLastName("contact").Build();

            this.editPartyRelationship = new OrganisationContactRelationshipBuilder(this.Transaction)
                .WithContactKind(new OrganisationContactKinds(this.Transaction).GeneralContact)
                .WithContact(this.contact)
                .WithOrganisation(this.organisation)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Login();
            this.people = this.Sidenav.NavigateToPeople();
        }

        [Fact]
        public void Create()
        {
            var before = new OrganisationContactRelationships(this.Transaction).Extent().ToArray();

            this.people.Table.DefaultAction(this.contact);
            var organisationContactRelationshipEdit = new PersonOverviewComponent(this.people.Driver, this.M).PartyrelationshipOverviewPanel.Click().CreateOrganisationContactRelationship();

            organisationContactRelationshipEdit.FromDate.Set(DateTimeFactory.CreateDate(2018, 12, 22))
                .ThroughDate.Set(DateTimeFactory.CreateDate(2018, 12, 22).AddYears(1))
                .ContactKinds.Toggle(new OrganisationContactKinds(this.Transaction).SalesContact)
                .Organisation.Select(this.organisation)
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new OrganisationContactRelationships(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var partyRelationship = after.Except(before).First();

            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, partyRelationship.FromDate.Date.ToUniversalTime().Date);
            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).AddYears(1).Date, partyRelationship.ThroughDate.Value.Date.ToUniversalTime().Date);
            Assert.Equal(2, partyRelationship.ContactKinds.Count());
            Assert.Contains(new OrganisationContactKinds(this.Transaction).GeneralContact, partyRelationship.ContactKinds);
            Assert.Contains(new OrganisationContactKinds(this.Transaction).SalesContact, partyRelationship.ContactKinds);
            Assert.Equal(this.organisation, partyRelationship.Organisation);
            Assert.Equal(this.contact, partyRelationship.Contact);
        }

        [Fact]
        public void Edit()
        {
            var before = new OrganisationContactRelationships(this.Transaction).Extent().ToArray();

            this.people.Table.DefaultAction(this.contact);
            var personOverviewPage = new PersonOverviewComponent(this.people.Driver, this.M);

            var partyRelationshipOverview = personOverviewPage.PartyrelationshipOverviewPanel.Click();
            partyRelationshipOverview.Table.DefaultAction(this.editPartyRelationship);

            var organisationContactRelationshipEditComponent = new OrganisationContactRelationshipEditComponent(this.Driver, this.M);
            organisationContactRelationshipEditComponent.FromDate.Set(DateTimeFactory.CreateDate(2018, 12, 22))
                .ThroughDate.Set(DateTimeFactory.CreateDate(2018, 12, 22).AddYears(1))
                .ContactKinds.Toggle(new OrganisationContactKinds(this.Transaction).GeneralContact)
                .ContactKinds.Toggle(new OrganisationContactKinds(this.Transaction).SalesContact)
                .ContactKinds.Toggle(new OrganisationContactKinds(this.Transaction).SupplierContact)
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new OrganisationContactRelationships(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length);

            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, this.editPartyRelationship.FromDate.Date.ToUniversalTime().Date);
            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).AddYears(1).Date, this.editPartyRelationship.ThroughDate.Value.Date.ToUniversalTime().Date);
            Assert.Equal(2, this.editPartyRelationship.ContactKinds.Count());
            Assert.Contains(new OrganisationContactKinds(this.Transaction).SalesContact, this.editPartyRelationship.ContactKinds);
            Assert.Contains(new OrganisationContactKinds(this.Transaction).SupplierContact, this.editPartyRelationship.ContactKinds);
            Assert.Equal(this.organisation, this.editPartyRelationship.Organisation);
            Assert.Equal(this.contact, this.editPartyRelationship.Contact);
        }
    }
}

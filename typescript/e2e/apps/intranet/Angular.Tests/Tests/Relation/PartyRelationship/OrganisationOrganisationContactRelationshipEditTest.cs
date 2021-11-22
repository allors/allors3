// <copyright file="OrganisationOrganisationContactRelationshipEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.organisation.list;
using libs.workspace.angular.apps.src.lib.objects.organisation.overview;
using libs.workspace.angular.apps.src.lib.objects.organisationcontactrelationship.edit;

namespace Tests.PartyRelationshipTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class OrganisationOrganisationContactRelationshipEditTest : Test, IClassFixture<Fixture>
    {
        private readonly OrganisationListComponent organisations;

        private readonly OrganisationContactRelationship editPartyRelationship;

        private readonly Organisation organisation;

        private readonly Person contact;

        public OrganisationOrganisationContactRelationshipEditTest(Fixture fixture)
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
            this.organisations = this.Sidenav.NavigateToOrganisations();
        }

        [Fact]
        public void Create()
        {
            var before = new OrganisationContactRelationships(this.Transaction).Extent().ToArray();

            this.organisations.Table.DefaultAction(this.organisation);
            var partyRelationshipEdit = new OrganisationOverviewComponent(this.organisations.Driver, this.M).PartyrelationshipOverviewPanel.Click().CreateOrganisationContactRelationship();

            partyRelationshipEdit
                .FromDate.Set(DateTimeFactory.CreateDate(2018, 12, 22))
                .ThroughDate.Set(DateTimeFactory.CreateDate(2018, 12, 22).AddYears(1))
                .ContactKinds.Toggle(new OrganisationContactKinds(this.Transaction).SalesContact)
                .Contact.Select(this.contact.DisplayName())
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

            this.organisations.Table.DefaultAction(this.organisation);
            var organisationOverview = new OrganisationOverviewComponent(this.organisations.Driver, this.M);

            var partyRelationshipOverview = organisationOverview.PartyrelationshipOverviewPanel.Click();
            partyRelationshipOverview.Table.DefaultAction(this.editPartyRelationship);

            var partyRelationshipEdit = new OrganisationContactRelationshipEditComponent(organisationOverview.Driver, this.M);
            partyRelationshipEdit
                .FromDate.Set(DateTimeFactory.CreateDate(2018, 12, 22))
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

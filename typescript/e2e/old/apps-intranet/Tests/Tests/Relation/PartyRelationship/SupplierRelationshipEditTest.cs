// <copyright file="SupplierRelationshipEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.organisation.list;
using libs.workspace.angular.apps.src.lib.objects.organisation.overview;
using libs.workspace.angular.apps.src.lib.objects.supplierrelationship.edit;

namespace Tests.PartyRelationshipTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class SupplierRelationshipEditTest : Test, IClassFixture<Fixture>
    {
        private readonly OrganisationListComponent organisations;

        private readonly PartyRelationship editPartyRelationship;

        public SupplierRelationshipEditTest(Fixture fixture)
            : base(fixture)
        {
            var allors = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();

            // Delete all existing for the new one to be in the first page of the list.
            foreach (PartyRelationship partyRelationship in allors.PartyRelationshipsWhereParty)
            {
                partyRelationship.Delete();
            }

            this.editPartyRelationship = new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier)
                .WithInternalOrganisation(allors)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Login();
            this.organisations = this.Sidenav.NavigateToOrganisations();
        }

        [Fact]
        public void Create()
        {
            var before = new PartyRelationships(this.Transaction).Extent().ToArray();

            var extent = new Organisations(this.Transaction).Extent();
            var internalOrganisation = extent.First(v => v.DisplayName().Equals("Allors BVBA"));

            this.organisations.Table.DefaultAction(internalOrganisation);
            var partyRelationshipEdit = new OrganisationOverviewComponent(this.organisations.Driver, this.M).PartyrelationshipOverviewPanel.Click().CreateCustomerRelationship();

            partyRelationshipEdit
                .FromDate.Set(DateTimeFactory.CreateDate(2018, 12, 22))
                .ThroughDate.Set(DateTimeFactory.CreateDate(2018, 12, 22).AddYears(1))
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PartyRelationships(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var partyRelationship = after.Except(before).First();

            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, partyRelationship.FromDate.Date.ToUniversalTime().Date);
            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).AddYears(1).Date, partyRelationship.ThroughDate.Value.Date.ToUniversalTime().Date);
        }

        [Fact]
        public void Edit()
        {
            var before = new PartyRelationships(this.Transaction).Extent().ToArray();

            var extent = new Organisations(this.Transaction).Extent();
            var internalOrganisation = extent.First(v => v.DisplayName().Equals("Allors BVBA"));

            this.organisations.Table.DefaultAction(internalOrganisation);
            var organisationOverviewPage = new OrganisationOverviewComponent(this.organisations.Driver, this.M);

            var partyRelationshipOverview = organisationOverviewPage.PartyrelationshipOverviewPanel.Click();
            partyRelationshipOverview.Table.DefaultAction(this.editPartyRelationship);

            var partyRelationshipEdit = new SupplierRelationshipEditComponent(organisationOverviewPage.Driver, this.M);
            partyRelationshipEdit
                .FromDate.Set(DateTimeFactory.CreateDate(2018, 12, 22))
                .ThroughDate.Set(DateTimeFactory.CreateDate(2018, 12, 22).AddYears(1))
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PartyRelationships(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length);

            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).Date, this.editPartyRelationship.FromDate.Date.ToUniversalTime().Date);
            // Assert.Equal(DateTimeFactory.CreateDate(2018, 12, 22).AddYears(1).Date, this.editPartyRelationship.ThroughDate.Value.Date.ToUniversalTime().Date);
        }
    }
}

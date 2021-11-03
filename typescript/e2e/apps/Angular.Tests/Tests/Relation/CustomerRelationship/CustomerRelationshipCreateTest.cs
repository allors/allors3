// <copyright file="CustomerShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.customershipment.create;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.CustomerRelationshipTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.catalogue.list;
    using libs.workspace.angular.apps.src.lib.objects.organisation.list;
    using libs.workspace.angular.apps.src.lib.objects.organisation.overview;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Product")]
    public class CustomerRelationshipCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly OrganisationListComponent organisationListPage;

        public CustomerRelationshipCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.organisationListPage = this.Sidenav.NavigateToOrganisations();
        }

        [Fact]
        public void Create()
        {
            var organisation = new Organisations(this.Transaction).Extent().First();
            var before = new CustomerRelationships(this.Transaction).Extent().ToArray();

            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var expected = new CustomerRelationshipBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();

            this.Transaction.Derive();

            var expectedFromDate = expected.FromDate;

            this.organisationListPage.Table.DefaultAction(organisation);
            var customerRelationship = new OrganisationOverviewComponent(this.organisationListPage.Driver, this.M).PartyrelationshipOverviewPanel.Click().CreateCustomerRelationship();

            customerRelationship.FromDate.Set(expected.FromDate);

            this.Transaction.Rollback();
            customerRelationship.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new CustomerRelationships(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedFromDate, actual.FromDate);
        }
    }
}

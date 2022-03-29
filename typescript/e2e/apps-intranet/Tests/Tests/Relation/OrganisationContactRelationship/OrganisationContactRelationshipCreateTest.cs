// <copyright file="OrganisationFaceToFaceCommunicationCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.organisation.list;
using libs.workspace.angular.apps.src.lib.objects.organisation.overview;

namespace Tests.FaceToFaceCommunicationTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class OrganisationContactRelationshipCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly OrganisationListComponent organisationListPage;

        public OrganisationContactRelationshipCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.organisationListPage = this.Sidenav.NavigateToOrganisations();
        }

        [Fact]
        public void Create()
        {
            var before = new OrganisationContactRelationships(this.Transaction).Extent().ToArray();
            
            var organisation = new Organisations(this.Transaction).Extent().First(v => v.CurrentContacts.Any());

            var expected = new OrganisationContactRelationshipBuilder(this.Transaction).WithDefaults(organisation).Build();

            this.Transaction.Derive();

            var expectedFromDate = expected.FromDate;
            var expectedContact = expected.Contact;

            this.organisationListPage.Table.DefaultAction(organisation);
            var organisationContactRelationship = new OrganisationOverviewComponent(this.organisationListPage.Driver, this.M).PartyrelationshipOverviewPanel.Click().CreateOrganisationContactRelationship();

            organisationContactRelationship
                .FromDate.Set(expected.FromDate)
                .Contact.Select(expected.Contact.DisplayName());

            this.Transaction.Rollback();
            organisationContactRelationship.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new OrganisationContactRelationships(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var organisationContactRelationshipCreated = after.Except(before).First();

            Assert.Equal(expectedFromDate.Date, organisationContactRelationshipCreated.FromDate.Date);
            Assert.Equal(expectedContact, organisationContactRelationshipCreated.Contact);
        }
    }
}

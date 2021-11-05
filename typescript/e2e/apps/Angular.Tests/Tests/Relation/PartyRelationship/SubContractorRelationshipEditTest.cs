// <copyright file="InternalOrganisationEmploymentEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.employment.edit;
using libs.workspace.angular.apps.src.lib.objects.organisation.list;
using libs.workspace.angular.apps.src.lib.objects.organisation.overview;

namespace Tests.PartyRelationshipTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class SubContractorRelationshipEditTest : Test, IClassFixture<Fixture>
    {
        private readonly OrganisationListComponent organisationListPage;

        public SubContractorRelationshipEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.organisationListPage = this.Sidenav.NavigateToOrganisations();
        }

        [Fact]
        public void Create()
        {
            var before = new SubContractorRelationships(this.Transaction).Extent().ToArray();

            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var organisation = new Organisations(this.Transaction).Extent().Except(new[] { internalOrganisation }).First();

            var expected = new SubContractorRelationshipBuilder(this.Transaction).WithDefaults(internalOrganisation, organisation).Build();

            this.Transaction.Derive();

            var expectedFromDate = expected.FromDate;
            var expectedThroughDate = expected.ThroughDate.Value;

            this.organisationListPage.Table.DefaultAction(organisation);
            var subContractorRelationshipEdit = new OrganisationOverviewComponent(this.organisationListPage.Driver, this.M).PartyrelationshipOverviewPanel.Click().CreateSubContractorRelationship();

            subContractorRelationshipEdit
                .FromDate.Set(expected.FromDate)
                .ThroughDate.Set(expected.ThroughDate.Value);

            this.Transaction.Rollback();
            subContractorRelationshipEdit.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SubContractorRelationships(this.Transaction).Extent().ToArray();

            Assert.Equal(before.Length + 1, after.Length);

            var subContractorRelationship = after.Except(before).First();

            Assert.Equal(expectedFromDate.Date, subContractorRelationship.FromDate.Date);
            Assert.Equal(expectedThroughDate.Date, subContractorRelationship.ThroughDate.Value.Date);
        }
    }
}

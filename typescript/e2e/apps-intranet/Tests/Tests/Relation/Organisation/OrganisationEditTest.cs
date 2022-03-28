// <copyright file="OrganisationEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.organisation.list;
using libs.workspace.angular.apps.src.lib.objects.organisation.overview;

namespace Tests.OrganisationTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class OrganisationEditTest : Test, IClassFixture<Fixture>
    {
        private readonly OrganisationListComponent organisationListPage;

        public OrganisationEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.organisationListPage = this.Sidenav.NavigateToOrganisations();
        }

        [Fact]
        public void Edit()
        {
            var customOrganisationClassification = new CustomOrganisationClassificationBuilder(this.Transaction).WithName("Gold").Build();
            var industryClassification = new IndustryClassificationBuilder(this.Transaction).WithName("Retail").Build();
            var legalForm = new LegalForms(this.Transaction).FindBy(M.LegalForm.Description, "BE - BVBA / SPRL");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var before = new Organisations(this.Transaction).Extent().ToArray();

            var organisation = before.Last();
            var id = organisation.Id;

            this.organisationListPage.Table.DefaultAction(organisation);
            var organisationOverview = new OrganisationOverviewComponent(this.organisationListPage.Driver, this.M);
            var organisationOverviewDetail = organisationOverview.OrganisationOverviewDetail.Click();
            organisationOverviewDetail
                .Name.Set("new organisation")
                .TaxNumber.Set("BE 123 456 789 01")
                .LegalForm.Select(legalForm)
                .Locale.Select(this.Transaction.GetSingleton().AdditionalLocales.FirstOrDefault())
                .IndustryClassifications.Toggle(industryClassification)
                .CustomClassifications.Toggle(customOrganisationClassification)
                .IsManufacturer.Set(true)
                .Comment.Set("comment")
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Organisations(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length);

            organisation = after.First(v => v.Id.Equals(id));
        }
    }
}

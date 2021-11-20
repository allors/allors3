// <copyright file="EmailAddressEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;
using libs.workspace.angular.apps.src.lib.objects.emailaddress.edit;
using libs.workspace.angular.apps.src.lib.objects.requestforquote.list;
using libs.workspace.angular.apps.src.lib.objects.requestforquote.overview;

namespace Tests.RequestForQuoteTests
{
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class RequestForQuoteEditTest : Test, IClassFixture<Fixture>
    {
        private readonly RequestForQuoteListComponent requestForQuoteListPage;

        public RequestForQuoteEditTest(Fixture fixture) : base(fixture)
        {
            this.Login();
            this.requestForQuoteListPage = this.Sidenav.NavigateToRequestsForQuote();
        }

        [Fact]
        public void Edit()
        {
            var before = new Requests(this.Transaction).Extent().ToArray();

            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithInternalOrganisationDefaults().Build();

            var expected = new RequestForQuoteBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();

            this.Transaction.Derive();

            var expectedRequestDate = expected.RequestDate;
            var expectedDescription = expected.Description;

            var request = before.First();
            var id = request.Id;

            this.requestForQuoteListPage.Table.DefaultAction(request);
            var requestForQuoteDetails = new RequestForQuoteOverviewComponent(this.requestForQuoteListPage.Driver, this.M);
            var requestForQuoteOverviewDetail = requestForQuoteDetails.RequestforquoteOverviewDetail.Click();

            requestForQuoteOverviewDetail
                .RequestDate.Set(expected.RequestDate)
                .Description.Set(expected.Description);

            this.Transaction.Rollback();
            requestForQuoteOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Requests(this.Transaction).Extent().ToArray();

            var good = after.First(v => v.Id.Equals(id));

            Assert.Equal(after.Length, before.Length);
            Assert.Equal(expectedRequestDate.Date, good.RequestDate.Date);
            Assert.Equal(expectedDescription, good.Description);
        }
    }
}

// <copyright file="EmailAddressEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;
using libs.workspace.angular.apps.src.lib.objects.emailaddress.edit;
using libs.workspace.angular.apps.src.lib.objects.requestforquote.list;
using libs.workspace.angular.apps.src.lib.objects.person.overview;

namespace Tests.RequestForQuoteTests
{
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class RequestForQuoteCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly RequestForQuoteListComponent requestForQuoteListPage;

        public RequestForQuoteCreateTest(Fixture fixture) : base(fixture)
        {
            this.Login();
            this.requestForQuoteListPage = this.Sidenav.NavigateToRequestsForQuote();
        }

        [Fact]
        public void Create()
        {
            var before = new Requests(this.Transaction).Extent().ToArray();

            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var expected = new RequestForQuoteBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();

            this.Transaction.Derive();

            var expectedRequestDate = expected.RequestDate;
            var expectedDescription = expected.Description;

            var nonUnifiedGoodCreate = this.requestForQuoteListPage.CreateRequestForQuote();

            nonUnifiedGoodCreate
                .RequestDate.Set(expected.RequestDate)
                .Description.Set(expected.Description);

            this.Transaction.Rollback();
            nonUnifiedGoodCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Requests(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var request = after.Except(before).First();

            Assert.Equal(expectedRequestDate.Date, request.RequestDate.Date);
            Assert.Equal(expectedDescription, request.Description);
        }
    }
}

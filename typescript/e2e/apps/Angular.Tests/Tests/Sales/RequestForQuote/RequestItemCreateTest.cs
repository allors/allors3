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
    public class RequestItemCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly RequestForQuoteListComponent requestForQuoteListPage;

        public RequestItemCreateTest(Fixture fixture) : base(fixture)
        {
            this.Login();
            this.requestForQuoteListPage = this.Sidenav.NavigateToRequestsForQuote();
        }

        [Fact]
        public void Create()
        {
            var before = new RequestItems(this.Transaction).Extent().ToArray();

            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var expectedRequest = new Requests(this.Transaction).Extent().FirstOrDefault(v => v is RequestForQuote);

            var unifiedGood = internalOrganisation.ProductCategoriesWhereInternalOrganisation.SelectMany(v => v.AllProducts).OfType<UnifiedGood>().FirstOrDefault(v => v.ExistSerialisedItems);
            
            var expected = new RequestItemBuilder(this.Transaction)
                .WithDefaults(internalOrganisation, unifiedGood)
                .Build();

            this.Transaction.Derive();

            var expectedQuantity = expected.Quantity;
            var expectedUnitOfMeasure = expected.UnitOfMeasure;
            var expectedAsignedUnitPrice = expected.AssignedUnitPrice;
            var expectedComment = expected.Comment;

            this.requestForQuoteListPage.Table.DefaultAction(expectedRequest);
            var goodDetails = new RequestForQuoteOverviewComponent(this.requestForQuoteListPage.Driver, this.M);
            var requestItemDetail = goodDetails.RequestitemOverviewPanel.Click().CreateRequestItem();

            requestItemDetail
                .Product.Select(expected.Product.Name)
                .Quantity.Set(expected.Quantity.ToString())
                .UnitOfMeasure.Select(expected.UnitOfMeasure)
                .AssignedUnitPrice.Set(expected.AssignedUnitPrice.ToString())
                .Comment.Set(expected.Comment);

            this.Transaction.Rollback();
            requestItemDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new RequestItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var requestItem = after.Except(before).First();

            Assert.Equal(expectedQuantity, requestItem.Quantity);
            Assert.Equal(expectedUnitOfMeasure, requestItem.UnitOfMeasure);
            Assert.Equal(expectedAsignedUnitPrice, requestItem.AssignedUnitPrice);
            Assert.Equal(expectedComment, requestItem.Comment);
        }
    }
}

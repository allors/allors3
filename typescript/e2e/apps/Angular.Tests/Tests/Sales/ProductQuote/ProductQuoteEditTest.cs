// <copyright file="PartyContactMechanismEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Tests.PartyContactMachanismTests
{
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.productquote.list;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class ProductQuoteEditTest : Test, IClassFixture<Fixture>
    {
        private readonly ProductQuoteListComponent productQuotes;

        public ProductQuoteEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.productQuotes = this.Sidenav.NavigateToProductQuotes();
        }

        [Fact]
        public void Edit()
        {
            var before = new ProductQuotes(this.Transaction).Extent().ToArray();

            var organisation = new Organisations(this.Transaction).Extent().FirstOrDefault(x => x.ActiveCustomers.Count() > 0); // x => x.PartyContactMechanisms.Count() > 0

            var expected = new ProductQuoteBuilder(this.Transaction)
                .WithDefaults(organisation)
                .Build();


            this.Transaction.Derive();

            var expectedFullfillContactMechanism = expected.FullfillContactMechanism;
            var expectedReceiver = expected.Receiver;

            var productQuoteDetail = productQuotes.CreateProductQuote();

            productQuoteDetail
                .Receiver.Select(expectedReceiver.DisplayName())
                .FullfillContactMechanism.Select(expectedFullfillContactMechanism);

            this.Transaction.Rollback();
            productQuoteDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new ProductQuotes(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var productQuote = after.Except(before).First();

            Assert.Equal(expectedReceiver, productQuote.Receiver);
            Assert.Equal(expectedFullfillContactMechanism, productQuote.FullfillContactMechanism);
        }
    }
}

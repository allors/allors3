// <copyright file="QuoteItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Allors.Database.Domain.TestPopulation;
    using Xunit;

    public class QuoteItemTests : DomainTest, IClassFixture<Fixture>
    {
        public QuoteItemTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenSerialisedItem_WhenDerived_ThenSerialisedItemAvailabilityIsChanged()
        {
            var quote = new ProductQuoteBuilder(this.Session).WithSerializedDefaults(this.InternalOrganisation).Build();

            this.Session.Derive();

            var serialisedItem = quote.QuoteItems.First().SerialisedItem;

            Assert.True(serialisedItem.OnQuote);
        }
    }

    [Trait("Category", "Security")]
    public class QuoteItemDeniedPermissonDerivationSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public QuoteItemDeniedPermissonDerivationSecurityTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.QuoteItem.ObjectType, this.M.QuoteItem.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedQuoteItemStateCreatedDeriveDeletePermission()
        {
            var purchaseQuote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session)
                .WithAssignedUnitPrice(1)
                .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Session).Build())
                .Build();

            purchaseQuote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, quoteItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangedQuoteItemStateCreatedWithNonDeletableQuoteDeriveDeletePermission()
        {
            var purchaseQuote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session)
                .WithAssignedUnitPrice(1)
                .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Session).Build())
                .Build();

            purchaseQuote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            purchaseQuote.Send();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, quoteItem.DeniedPermissions);
        }
    }
}

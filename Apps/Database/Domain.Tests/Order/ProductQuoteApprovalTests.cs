// <copyright file="ProductQuoteApprovalTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using TestPopulation;
    using Xunit;

    public class ProductQuoteApprovalDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteApprovalDerivationTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void ChangedProductQuoteDeriveTitle()
        {
            var quote = new ProductQuoteBuilder(this.Session).WithIssuer(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var approval = new ProductQuoteApprovalBuilder(this.Session).WithProductQuote(quote).Build();
            this.Session.Derive(false);

            Assert.Equal(approval.Title, "Approval of " + quote.WorkItemDescription);
        }

        [Fact]
        public void ChangedProductQuoteDeriveWorkItem()
        {
            var quote = new ProductQuoteBuilder(this.Session).WithIssuer(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var approval = new ProductQuoteApprovalBuilder(this.Session).WithProductQuote(quote).Build();
            this.Session.Derive(false);

            Assert.Equal(approval.WorkItem, quote);
        }

        [Fact]
        public void ChangedProductQuoteDeriveDateClosed()
        {
            var quote = new ProductQuoteBuilder(this.Session).WithIssuer(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var approval = new ProductQuoteApprovalBuilder(this.Session).WithProductQuote(quote).Build();
            this.Session.Derive(false);

            Assert.True(approval.ExistDateClosed);
        }

        [Fact]
        public void ChangedProductQuoteQuoteStateDeriveDateClosed()
        {
            var quote = this.InternalOrganisation.CreateB2BProductQuoteWithSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            quote.QuoteState = new QuoteStates(this.Session).AwaitingApproval;
            this.Session.Derive(false);

            Assert.False(quote.ProductQuoteApprovalsWhereProductQuote.First().ExistDateClosed);
        }
    }

    public class ProductQuoteApprovalParticipantsDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteApprovalParticipantsDerivationTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void ChangedProductQuoteDeriveEmptyParticipants()
        {
            var quote = new ProductQuoteBuilder(this.Session).WithIssuer(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var approval = new ProductQuoteApprovalBuilder(this.Session).WithProductQuote(quote).Build();
            this.Session.Derive(false);

            Assert.Empty(approval.Participants);
        }

        [Fact]
        public void ChangedProductQuoteQuoteStateDeriveParticipants()
        {
            var quote = this.InternalOrganisation.CreateB2BProductQuoteWithSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            quote.QuoteState = new QuoteStates(this.Session).AwaitingApproval;
            this.Session.Derive(false);

            Assert.NotEmpty(quote.ProductQuoteApprovalsWhereProductQuote.First().Participants);
        }
    }
}

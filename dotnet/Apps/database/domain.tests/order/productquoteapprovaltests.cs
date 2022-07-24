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

    public class ProductQuoteApprovalRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteApprovalRuleTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void ChangedProductQuoteDeriveTitle()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).WithIssuer(this.InternalOrganisation).Build();
            this.Derive();

            var approval = new ProductQuoteApprovalBuilder(this.Transaction).WithProductQuote(quote).Build();
            this.Derive();

            Assert.Equal(approval.Title, "Approval of " + quote.WorkItemDescription);
        }

        [Fact]
        public void ChangedProductQuoteDeriveWorkItem()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).WithIssuer(this.InternalOrganisation).Build();
            this.Derive();

            var approval = new ProductQuoteApprovalBuilder(this.Transaction).WithProductQuote(quote).Build();
            this.Derive();

            Assert.Equal(approval.WorkItem, quote);
        }

        [Fact]
        public void ChangedProductQuoteDeriveDateClosed()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).WithIssuer(this.InternalOrganisation).Build();
            this.Derive();

            var approval = new ProductQuoteApprovalBuilder(this.Transaction).WithProductQuote(quote).Build();
            this.Derive();

            Assert.True(approval.ExistDateClosed);
        }

        [Fact]
        public void ChangedProductQuoteQuoteStateDeriveDateClosed()
        {
            var quote = this.InternalOrganisation.CreateB2BProductQuoteWithSerialisedItem(this.Transaction.Faker());
            this.Derive();

            quote.QuoteState = new QuoteStates(this.Transaction).AwaitingApproval;
            this.Derive();

            Assert.False(quote.ProductQuoteApprovalsWhereProductQuote.First().ExistDateClosed);
        }
    }

    public class ProductQuoteApprovalParticipantsRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteApprovalParticipantsRuleTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void ChangedProductQuoteDeriveEmptyParticipants()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).WithIssuer(this.InternalOrganisation).Build();
            this.Derive();

            var approval = new ProductQuoteApprovalBuilder(this.Transaction).WithProductQuote(quote).Build();
            this.Derive();

            Assert.Empty(approval.Participants);
        }

        [Fact]
        public void ChangedProductQuoteQuoteStateDeriveParticipants()
        {
            var quote = this.InternalOrganisation.CreateB2BProductQuoteWithSerialisedItem(this.Transaction.Faker());
            this.Derive();

            quote.QuoteState = new QuoteStates(this.Transaction).AwaitingApproval;
            this.Derive();

            Assert.NotEmpty(quote.ProductQuoteApprovalsWhereProductQuote.First().Participants);
        }
    }
}

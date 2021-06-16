// <copyright file="QuoteTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Resources;
    using TestPopulation;
    using Xunit;

    public class ProductQuoteTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenProductQuote_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var party = new PersonBuilder(this.Transaction).WithLastName("party").Build();

            this.Transaction.Commit();

            var builder = new ProductQuoteBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithReceiver(party);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithFullfillContactMechanism(new WebAddressBuilder(this.Transaction).WithElectronicAddressString("test").Build());
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenProposal_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var party = new PersonBuilder(this.Transaction).WithLastName("party").Build();

            this.Transaction.Commit();

            var builder = new ProposalBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithReceiver(party);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithFullfillContactMechanism(new WebAddressBuilder(this.Transaction).WithElectronicAddressString("test").Build());
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenStatementOfWork_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var party = new PersonBuilder(this.Transaction).WithLastName("party").Build();

            this.Transaction.Commit();

            var builder = new StatementOfWorkBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithReceiver(party);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithFullfillContactMechanism(new WebAddressBuilder(this.Transaction).WithElectronicAddressString("test").Build());
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenProductQuote_WhenDeriving_ThenTotalPriceIsDerivedFromItems()
        {
            var party = new PersonBuilder(this.Transaction).WithLastName("party").Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            var quote = new ProductQuoteBuilder(this.Transaction)
                .WithReceiver(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Transaction).WithElectronicAddressString("test").Build())
                .Build();

            var item1 = new QuoteItemBuilder(this.Transaction).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(1000).Build();
            var item2 = new QuoteItemBuilder(this.Transaction).WithProduct(good).WithQuantity(3).WithAssignedUnitPrice(100).Build();

            quote.AddQuoteItem(item1);
            quote.AddQuoteItem(item2);

            this.Transaction.Derive();

            Assert.Equal(1300, quote.TotalExVat);
        }

        [Fact]
        public void GivenIssuerWithoutQuoteNumberPrefix_WhenDeriving_ThenSortableQuoteNumberIsSet()
        {
            this.InternalOrganisation.RemoveQuoteNumberPrefix();
            this.Transaction.Derive();

            var party = new PersonBuilder(this.Transaction).WithLastName("party").Build();

            var quote = new ProductQuoteBuilder(this.Transaction)
                .WithReceiver(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Transaction).WithElectronicAddressString("test").Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(int.Parse(quote.QuoteNumber), quote.SortableQuoteNumber);
        }

        [Fact]
        public void GivenIssuerWithQuoteNumberPrefix_WhenDeriving_ThenSortableQuoteNumberIsSet()
        {
            this.InternalOrganisation.QuoteSequence = new QuoteSequences(this.Transaction).EnforcedSequence;
            this.InternalOrganisation.QuoteNumberPrefix = "prefix-";
            this.Transaction.Derive();

            var party = new PersonBuilder(this.Transaction).WithLastName("party").Build();

            var quote = new ProductQuoteBuilder(this.Transaction)
                .WithReceiver(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Transaction).WithElectronicAddressString("test").Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(int.Parse(quote.QuoteNumber.Split('-')[1]), quote.SortableQuoteNumber);
        }

        [Fact]
        public void GivenIssuerWithParametrizedQuoteNumberPrefix_WhenDeriving_ThenSortableQuoteNumberIsSet()
        {
            this.InternalOrganisation.QuoteSequence = new QuoteSequences(this.Transaction).EnforcedSequence;
            this.InternalOrganisation.QuoteNumberPrefix = "prefix-{year}-";
            this.Transaction.Derive();

            var party = new PersonBuilder(this.Transaction).WithLastName("party").Build();

            var quote = new ProductQuoteBuilder(this.Transaction)
                .WithReceiver(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Transaction).WithElectronicAddressString("test").Build())
                .WithIssueDate(this.Transaction.Now().Date)
                .Build();

            this.Transaction.Derive();

            var number = int.Parse(quote.QuoteNumber.Split('-').Last()).ToString("000000");
            Assert.Equal(int.Parse(string.Concat(this.Transaction.Now().Date.Year.ToString(), number)), quote.SortableQuoteNumber);
        }
    }

    public class ProductQuoteRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedIssuerThrowValidationError()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            quote.Issuer = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();

            Assert.Contains(ErrorMessages.InternalOrganisationChanged, this.Derive().Errors[0].Message);
        }

        [Fact]
        public void ChangedIssuerDeriveWorkItemDescription()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            var expected = $"ProductQuote: {quote.QuoteNumber} [{quote.Issuer?.PartyName}]";
            Assert.Equal(expected, quote.WorkItemDescription);
        }
    }

    public class ProductQuoteAwaitingApprovalRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteAwaitingApprovalRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedQuoteStateCreateApprovalTask()
        {
            var quote = this.InternalOrganisation.CreateB2BProductQuoteWithSerialisedItem(this.Transaction.Faker());
            this.Derive();

            quote.QuoteState = new QuoteStates(this.Transaction).AwaitingApproval;
            this.Derive();

            Assert.True(quote.ExistProductQuoteApprovalsWhereProductQuote);
        }
    }

    [Trait("Category", "Security")]
    public class ProductQuoteDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteDeniedPermissionRuleTests(Fixture fixture) : base(fixture)
        {
            this.deletePermission = new Permissions(this.Transaction).Get(this.M.ProductQuote, this.M.ProductQuote.Delete);
            this.setReadyPermission = new Permissions(this.Transaction).Get(this.M.ProductQuote, this.M.ProductQuote.SetReadyForProcessing);
        }

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;
        private readonly Permission setReadyPermission;

        [Fact]
        public void OnChangedProductQuoteValidQuoteItemsDeriveSetReadyPermissionAllowed()
        {
            var productQuote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction)
                    .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Transaction).Build())
                    .WithAssignedUnitPrice(1)
                    .Build();
            productQuote.AddQuoteItem(quoteItem);

            this.Derive();

            Assert.DoesNotContain(this.setReadyPermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductQuoteValidQuoteItemsDeriveSetReadyPermissionDenied()
        {
            var productQuote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction)
                    .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Transaction).Build())
                    .WithAssignedUnitPrice(1)
                    .Build();
            productQuote.AddQuoteItem(quoteItem);
            this.Derive();

            quoteItem.Cancel();
            this.Derive();

            Assert.Contains(this.setReadyPermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductQuoteStateNotCreatedDeriveSetReadyPermission()
        {
            var productQuote = this.InternalOrganisation.CreateB2BProductQuoteWithSerialisedItem(this.Transaction.Faker());
            this.Derive();

            productQuote.Send();
            this.Derive();

            Assert.True(productQuote.QuoteState.IsAwaitingAcceptance);
            Assert.Contains(this.setReadyPermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveDeletePermissionAllowed()
        {
            var productQuote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveDeletePermissionDenied()
        {
            var productQuote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            productQuote.QuoteState = new QuoteStates(this.Transaction).Accepted;
            this.Derive();

            Assert.Contains(this.deletePermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestDeriveDeletePermissionDenied()
        {
            var productQuote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            productQuote.Request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(this.deletePermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestDeriveDeletePermissionDeniedAllowed()
        {
            var productQuote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            productQuote.Request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            productQuote.RemoveRequest();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrderQuoteDeriveDeletePermissionDenied()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            salesOrder.Quote = quote;
            this.Derive();

            Assert.Contains(this.deletePermission, quote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrderQuoteDeriveDeletePermissionAllowed()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            salesOrder.Quote = quote;
            this.Derive();

            salesOrder.RemoveQuote();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, quote.DeniedPermissions);
        }
    }
}

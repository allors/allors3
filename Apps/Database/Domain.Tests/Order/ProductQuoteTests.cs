// <copyright file="QuoteTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Resources;
    using TestPopulation;
    using Xunit;

    public class ProductQuoteTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenProductQuote_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            this.Session.Commit();

            var builder = new ProductQuoteBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithReceiver(party);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build());
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenProposal_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            this.Session.Commit();

            var builder = new ProposalBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithReceiver(party);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build());
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenStatementOfWork_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            this.Session.Commit();

            var builder = new StatementOfWorkBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithReceiver(party);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build());
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenProductQuote_WhenDeriving_ThenTotalPriceIsDerivedFromItems()
        {
            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            var good = new Goods(this.Session).FindBy(this.M.Good.Name, "good1");

            var quote = new ProductQuoteBuilder(this.Session)
                .WithReceiver(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build())
                .Build();

            var item1 = new QuoteItemBuilder(this.Session).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(1000).Build();
            var item2 = new QuoteItemBuilder(this.Session).WithProduct(good).WithQuantity(3).WithAssignedUnitPrice(100).Build();

            quote.AddQuoteItem(item1);
            quote.AddQuoteItem(item2);

            this.Session.Derive();

            Assert.Equal(1300, quote.TotalExVat);
        }

        [Fact]
        public void GivenIssuerWithoutQuoteNumberPrefix_WhenDeriving_ThenSortableQuoteNumberIsSet()
        {
            this.InternalOrganisation.RemoveQuoteNumberPrefix();
            this.Session.Derive();

            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            var quote = new ProductQuoteBuilder(this.Session)
                .WithReceiver(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build())
                .Build();

            this.Session.Derive();

            Assert.Equal(int.Parse(quote.QuoteNumber), quote.SortableQuoteNumber);
        }

        [Fact]
        public void GivenIssuerWithQuoteNumberPrefix_WhenDeriving_ThenSortableQuoteNumberIsSet()
        {
            this.InternalOrganisation.QuoteSequence = new QuoteSequences(this.Session).EnforcedSequence;
            this.InternalOrganisation.QuoteNumberPrefix = "prefix-";
            this.Session.Derive();

            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            var quote = new ProductQuoteBuilder(this.Session)
                .WithReceiver(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build())
                .Build();

            this.Session.Derive();

            Assert.Equal(int.Parse(quote.QuoteNumber.Split('-')[1]), quote.SortableQuoteNumber);
        }

        [Fact]
        public void GivenIssuerWithParametrizedQuoteNumberPrefix_WhenDeriving_ThenSortableQuoteNumberIsSet()
        {
            this.InternalOrganisation.QuoteSequence = new QuoteSequences(this.Session).EnforcedSequence;
            this.InternalOrganisation.QuoteNumberPrefix = "prefix-{year}-";
            this.Session.Derive();

            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            var quote = new ProductQuoteBuilder(this.Session)
                .WithReceiver(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build())
                .WithIssueDate(this.Session.Now().Date)
                .Build();

            this.Session.Derive();

            var number = int.Parse(quote.QuoteNumber.Split('-').Last()).ToString("000000");
            Assert.Equal(int.Parse(string.Concat(this.Session.Now().Date.Year.ToString(), number)), quote.SortableQuoteNumber);
        }
    }

    public class ProductQuoteDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedIssuerThrowValidationError()
        {
            var quote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            quote.Issuer = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();

            var expectedError = $"{quote} {this.M.ProductQuote.Issuer} {ErrorMessages.InternalOrganisationChanged}";
            Assert.Equal(expectedError, this.Session.Derive(false).Errors[0].Message);
        }

        [Fact]
        public void ChangedIssuerDeriveWorkItemDescription()
        {
            var quote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            var expected = $"ProductQuote: {quote.QuoteNumber} [{quote.Issuer?.PartyName}]";
            Assert.Equal(expected, quote.WorkItemDescription);
        }
    }

    public class ProductQuoteAwaitingApprovalDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteAwaitingApprovalDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedQuoteStateCreateApprovalTask()
        {
            var quote = this.InternalOrganisation.CreateB2BProductQuoteWithSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            quote.QuoteState = new QuoteStates(this.Session).AwaitingApproval;
            this.Session.Derive(false);

            Assert.True(quote.ExistProductQuoteApprovalsWhereProductQuote);
        }
    }

    [Trait("Category", "Security")]
    public class ProductQuoteDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteDeniedPermissionDerivationTests(Fixture fixture) : base(fixture)
        {
            this.deletePermission = new Permissions(this.Session).Get(this.M.ProductQuote.ObjectType, this.M.ProductQuote.Delete);
            this.setReadyPermission = new Permissions(this.Session).Get(this.M.ProductQuote.ObjectType, this.M.ProductQuote.SetReadyForProcessing);
        }

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;
        private readonly Permission setReadyPermission;

        [Fact]
        public void OnChangedProductQuoteQuoteItemStateCreatedDeriveSetReadyPermission()
        {
            var productQuote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session)
                    .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Session).Build())
                    .WithAssignedUnitPrice(1)
                    .Build();
            productQuote.AddQuoteItem(quoteItem);

            this.Session.Derive(false);

            Assert.DoesNotContain(this.setReadyPermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductQuoteWithoutQuoteItemStateCreatedDeriveSetReadyPermission()
        {
            var productQuote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Contains(this.setReadyPermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductQuoteStateNotCreatedDeriveSetReadyPermission()
        {
            var productQuote = this.InternalOrganisation.CreateB2BProductQuoteWithSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            productQuote.Send();
            this.Session.Derive(false);

            Assert.True(productQuote.QuoteState.IsAwaitingAcceptance);
            Assert.Contains(this.setReadyPermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductQuoteStateCreatedDeriveDeletePermission()
        {
            var productQuote = new ProductQuoteBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductQuoteStateCreatedWithRequestDeriveDeletePermission()
        {
            var request = new RequestForQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            var productQuote = new ProductQuoteBuilder(this.Session).WithRequest(request).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductQuoteStateApprovalDeriveDeletePermission()
        {
            var productQuote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            productQuote.Send();
            this.Session.Derive(false);

            productQuote.Accept();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductQuoteStateOrderedWithSalesOrderDeriveDeletePermission()
        {
            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            var good = new Goods(this.Session).FindBy(this.M.Good.Name, "good1");
             
            var quote = new ProductQuoteBuilder(this.Session)
                .WithReceiver(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build())
                .Build();

            var item1 = new QuoteItemBuilder(this.Session).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(1000).Build();
            var item2 = new QuoteItemBuilder(this.Session).WithProduct(good).WithQuantity(3).WithAssignedUnitPrice(100).Build();

            quote.AddQuoteItem(item1);
            quote.AddQuoteItem(item2);

            this.Session.Derive(false);

            quote.Send();
            this.Session.Derive(false);

            quote.Accept();
            this.Session.Derive(false);

            quote.Send();
            this.Session.Derive(false);

            var productQuoteOrder = quote.Order();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, quote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductQuoteWithRequestDeriveDeletePermission()
        {
            var quote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            quote.Request = new RequestForQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            var item = new QuoteItemBuilder(this.Session).Build();
            quote.AddQuoteItem(item);
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, quote.DeniedPermissions);
        }
    }
}

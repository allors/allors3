// <copyright file="ProposalDeniedPermissionRuleTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Allors.Database.Domain.TestPopulation;
    using Resources;
    using Xunit;

    public class ProposalRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ProposalRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedIssuerThrowValidationError()
        {
            var quote = new ProposalBuilder(this.Transaction).Build();
            this.Derive();

            quote.Issuer = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();

            Assert.Contains(ErrorMessages.InternalOrganisationChanged, this.Derive().Errors.ElementAt(0).Message);
        }
    }

    [Trait("Category", "Security")]
    public class ProposalDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ProposalDeniedPermissionRuleTests(Fixture fixture) : base(fixture)
        {
            this.deleteRevocation = new Revocations(this.Transaction).ProposalDeleteRevocation;
            this.setReadyForProcessingRevocation = new Revocations(this.Transaction).ProposalSetReadyForProcessingRevocation;
        }

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;
        private readonly Revocation setReadyForProcessingRevocation;

        [Fact]
        public void OnChangedProductQuoteValidQuoteItemsDeriveSetReadyPermissionAllowed()
        {
            var quote = new ProposalBuilder(this.Transaction).Build();
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction)
                    .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Transaction).Build())
                    .WithAssignedUnitPrice(1)
                    .Build();
            quote.AddQuoteItem(quoteItem);

            this.Derive();

            Assert.DoesNotContain(this.setReadyForProcessingRevocation, quote.Revocations);
        }

        [Fact]
        public void OnChangedProductQuoteValidQuoteItemsDeriveSetReadyPermissionDenied()
        {
            var quote = new ProposalBuilder(this.Transaction).Build();
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction)
                    .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Transaction).Build())
                    .WithAssignedUnitPrice(1)
                    .Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            quoteItem.Cancel();
            this.Derive();

            Assert.Contains(this.setReadyForProcessingRevocation, quote.Revocations);
        }

        [Fact]
        public void OnChangedProductQuoteStateNotCreatedDeriveSetReadyPermission()
        {
            var quote = this.InternalOrganisation.CreateB2BProductQuoteWithSerialisedItem(this.Transaction.Faker());
            this.Derive();

            quote.Send();
            this.Derive();

            Assert.True(quote.QuoteState.IsAwaitingAcceptance);

            var setReadyForProcessingPermission = new Permissions(this.Transaction).Get(this.M.ProductQuote, this.M.ProductQuote.SetReadyForProcessing);
            Assert.Contains(setReadyForProcessingPermission, quote.Revocations.SelectMany(v => v.DeniedPermissions));
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveDeletePermissionAllowed()
        {
            var quote = new ProposalBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, quote.Revocations);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveDeletePermissionDenied()
        {
            var quote = new ProposalBuilder(this.Transaction).Build();
            this.Derive();

            quote.QuoteState = new QuoteStates(this.Transaction).Accepted;
            this.Derive();

            Assert.Contains(this.deleteRevocation, quote.Revocations);
        }

        [Fact]
        public void OnChangedRequestDeriveDeletePermissionDenied()
        {
            var quote = new ProposalBuilder(this.Transaction).Build();
            this.Derive();

            quote.Request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(this.deleteRevocation, quote.Revocations);
        }

        [Fact]
        public void OnChangedRequestDeriveDeletePermissionDeniedAllowed()
        {
            var quote = new ProposalBuilder(this.Transaction).Build();
            this.Derive();

            quote.Request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            quote.RemoveRequest();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, quote.Revocations);
        }
    }
}

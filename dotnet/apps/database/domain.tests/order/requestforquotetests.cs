// <copyright file="RequestForQuoteTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Resources;
    using Xunit;

    public class RequestForQuoteRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestForQuoteRuleTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void ChangedRecipientDeriveValidationError()
        {
            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            request.Recipient = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InternalOrganisationChanged));
        }

        [Fact]
        public void ChangedRequestItemsDeriveRequestItemsSyncedRequest()
        {
            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            var requestItem = new RequestItemBuilder(this.Transaction).Build();
            request.AddRequestItem(requestItem);
            this.Derive();

            Assert.Equal(request, requestItem.SyncedRequest);
        }
    }

    [Trait("Category", "Security")]
    public class RequestForQuoteDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestForQuoteDeniedPermissionRuleTests(Fixture fixture) : base(fixture)
        {
            this.deleteRevocation = new Revocations(this.Transaction).RequestForQuoteDeleteRevocation;
            this.submitRevocation = new Revocations(this.Transaction).RequestForQuoteSubmitRevocation;
        }

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;
        private readonly Revocation submitRevocation;

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveDeletePermissionDenied()
        {
            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(this.deleteRevocation, request.Revocations);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveDeletePermissionAllowed()
        {
            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            request.RequestState = new RequestStates(this.Transaction).Submitted;
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, request.Revocations);
        }

        [Fact]
        public void OnChangedQuoteRequestDeriveDeletePermission()
        {
            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            quote.Request = request;
            this.Derive();

            Assert.Contains(this.deleteRevocation, request.Revocations);
        }

        [Fact]
        public void OnChangedRequestItemsRequestItemStateDeriveDeletePermission()
        {
            var request = new RequestForQuoteBuilder(this.Transaction)
                .WithRequestState(new RequestStates(this.Transaction).Submitted)
                .Build();
            this.Derive();

            var requestItem = new RequestItemBuilder(this.Transaction).Build();
            request.AddRequestItem(requestItem);
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, request.Revocations);

            requestItem.RequestItemState = new RequestItemStates(this.Transaction).Quoted;
            this.Derive();

            Assert.Contains(this.deleteRevocation, request.Revocations);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveSubmitPermissionDenied()
        {
            var requestForQuote = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(this.submitRevocation, requestForQuote.Revocations);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveSubmitPermissionAllowed()
        {
            var requestForQuote = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(this.submitRevocation, requestForQuote.Revocations);

            requestForQuote.Originator = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            this.Derive();

            Assert.DoesNotContain(this.submitRevocation, requestForQuote.Revocations);
        }
    }
}

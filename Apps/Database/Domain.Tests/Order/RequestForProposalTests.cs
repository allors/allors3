// <copyright file="OrderTermTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Resources;
    using Xunit;

    public class RequestForProposalDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestForProposalDerivationTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void ChangedRecipientDeriveValidationError()
        {
            var request = new RequestForProposalBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            request.Recipient = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();

            var expectedMessage = $"{request} { this.M.RequestForProposal.Recipient} { ErrorMessages.InternalOrganisationChanged}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedRequestItemsDeriveRequestItemsSyncedRequest()
        {
            var request = new RequestForProposalBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requestItem = new RequestItemBuilder(this.Transaction).Build();
            request.AddRequestItem(requestItem);
            this.Transaction.Derive(false);

            Assert.Equal(request, requestItem.SyncedRequest);
        }
    }

    [Trait("Category", "Security")]
    public class RequestForProposalDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestForProposalDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.RequestForProposal.ObjectType, this.M.RequestForProposal.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveDeletePermissionDenied()
        {
            var request = new RequestForProposalBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, request.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveDeletePermissionAllowed()
        {
            var request = new RequestForProposalBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            request.RequestState = new RequestStates(this.Transaction).Submitted;
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, request.DeniedPermissions);
        }

        [Fact]
        public void OnChangedQuoteRequestDeriveDeletePermission()
        {
            var request = new RequestForProposalBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quote.Request = request;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, request.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestItemsRequestItemStateDeriveDeletePermission()
        {
            var request = new RequestForProposalBuilder(this.Transaction)
                .WithRequestState(new RequestStates(this.Transaction).Submitted)
                .Build();
            this.Transaction.Derive(false);

            var requestItem = new RequestItemBuilder(this.Transaction).Build();
            request.AddRequestItem(requestItem);
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, request.DeniedPermissions);

            requestItem.RequestItemState = new RequestItemStates(this.Transaction).Quoted;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, request.DeniedPermissions);
        }
    }
}

// <copyright file="OrderTermTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Domain
{
    using Xunit;

    [Trait("Category", "Security")]
    public class RequestForProposalSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestForProposalSecurityTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.RequestForProposal.ObjectType, this.M.RequestForProposal.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedRequestForProposalStateCreatedDeriveDeletePermission()
        {
            var requestForProposal = new RequestForProposalBuilder(this.Session).Build();
            this.Session.Derive(false);

            requestForProposal.Submit();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, requestForProposal.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestForProposalStateDeriveDeletePermission()
        {
            var requestForProposal = new RequestForProposalBuilder(this.Session).WithEmailAddress("test@test.be").Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, requestForProposal.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestForProposalWithQuoteStateDeriveDeletePermission()
        {
            var requestForProposal = new RequestForProposalBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quote = new ProductQuoteBuilder(this.Session).WithRequest(requestForProposal).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, requestForProposal.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestForProposalStateCreatedWithDeletableRequestItemsDeriveDeletePermission()
        {
            var requestItem = new RequestItemBuilder(this.Session).Build();
            var requestForProposal = new RequestForQuoteBuilder(this.Session).WithRequestItem(requestItem).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, requestForProposal.DeniedPermissions);
        }
    }
}

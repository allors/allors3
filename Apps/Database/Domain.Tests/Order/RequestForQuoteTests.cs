// <copyright file="OrderTermTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Domain
{
    using Xunit;

    [Trait("Category", "Security")]
    public class RequestForQuoteSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestForQuoteSecurityTests(Fixture fixture) : base(fixture)
        {
            this.deletePermission = new Permissions(this.Session).Get(this.M.RequestForQuote.ObjectType, this.M.RequestForQuote.Delete);
            this.submitPermission = new Permissions(this.Session).Get(this.M.RequestForQuote.ObjectType, this.M.RequestForQuote.Submit);
        }
        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;
        private readonly Permission submitPermission;

        [Fact]
        public void OnChangedRequestForQuoteStateCreatedDeriveDeletePermission()
        {
            var requestForQuote = new RequestForQuoteBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, requestForQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestForQuoteStateCreatedWithEmailAddressDeriveDeletePermission()
        {
            var requestForQuote = new RequestForQuoteBuilder(this.Session).WithEmailAddress("test@test.com").Build();

            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, requestForQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestForQuoteStateCreatedWithQuoteWhereRequestDeriveDeletePermission()
        {
            var requestForQuote = new RequestForQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quote = new ProductQuoteBuilder(this.Session).WithRequest(requestForQuote).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, requestForQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestForQuoteStateCreatedWithDeletableRequestItemsDeriveDeletePermission()
        {
            var requestItem = new RequestItemBuilder(this.Session).Build();
            var requestForQuote = new RequestForQuoteBuilder(this.Session).WithRequestItem(requestItem).Build();
            this.Session.Derive(false);

            requestForQuote.Cancel();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, requestForQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestForQuoteStateCancelledDeriveDeletePermission()
        {
            var requestForQuote = new RequestForQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            requestForQuote.Cancel();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, requestForQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestForQuoteStateCreatedDeriveSubmitPermission()
        {
            var requestForQuote = new RequestForQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Contains(this.submitPermission, requestForQuote.DeniedPermissions);
        }
    }
}

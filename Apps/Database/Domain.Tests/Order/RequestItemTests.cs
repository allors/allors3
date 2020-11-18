// <copyright file="OrderTermTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    [Trait("Category", "Security")]
    public class RequestItemSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestItemSecurityTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.RequestItem.ObjectType, this.M.RequestItem.Delete);
        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedRequestItemStateCreatedDeriveDeletePermission()
        {
            var requestItem = new RequestItemBuilder(this.Session).Build();
            var requestForQuote = new RequestForQuoteBuilder(this.Session).WithRequestItem(requestItem).Build();
            this.Session.Derive(false);

            requestForQuote.Submit();
            this.Session.Derive(false);

            requestForQuote.CreateQuote();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, requestItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestItemStateSubmitDeriveDeletePermission()
        {
            var requestItem = new RequestItemBuilder(this.Session).Build();
            var requestForQuote = new RequestForQuoteBuilder(this.Session).WithRequestItem(requestItem).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, requestItem.DeniedPermissions);
        }
    }
}

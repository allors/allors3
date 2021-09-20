// <copyright file="DerivationNodesTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Allors;
    using Allors.Database.Domain;
    using Xunit;

    public class MarkTest : DomainTest, IClassFixture<Fixture>
    {
        public MarkTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void PostDerive()
        {
            var post = new PostBuilder(this.Transaction).Build();

            this.Transaction.Derive();

            Assert.Equal(1, post.Counter);

            post.Counter = 3;

            this.Transaction.Derive();

            Assert.Equal(5, post.Counter);
        }
    }
}

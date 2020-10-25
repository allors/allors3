// <copyright file="DerivationNodesTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Tests
{
    using Allors;
    using Allors.Domain;
    using Xunit;

    public class ValidationDomainDerivationTest : DomainTest, IClassFixture<Fixture>
    {
        public ValidationDomainDerivationTest(Fixture fixture) : base(fixture, false) { }

        [Fact]
        public void One2OneRoles()
        {
            var cc = new CCBuilder(this.Session)
                .Build();

            var bb = new BBBuilder(this.Session)
                .WithOne2One(cc)
                .Build();

            var aa = new AABuilder(this.Session)
                .WithOne2One(bb)
                .Build();

            this.Session.Derive();

            cc.Assigned = "x";

            this.Session.Derive();

            Assert.Equal("x", aa.Derived);
        }

        [Fact]
        public void Many2OneRoles()
        {
            var cc = new CCBuilder(this.Session)
                .Build();

            var bb = new BBBuilder(this.Session)
                .WithMany2One(cc)
                .Build();

            var aa = new AABuilder(this.Session)
                .WithMany2One(bb)
                .Build();

            this.Session.Derive();

            cc.Assigned = "x";

            this.Session.Derive();

            Assert.Equal("x", aa.Derived);
        }

        [Fact]
        public void One2ManyRoles()
        {
            var cc = new CCBuilder(this.Session)
                .Build();

            var bb = new BBBuilder(this.Session)
                .WithOne2Many(cc)
                .Build();

            var aa = new AABuilder(this.Session)
                .WithOne2Many(bb)
                .Build();

            this.Session.Derive();

            cc.Assigned = "x";

            this.Session.Derive();

            Assert.Equal("x", aa.Derived);
        }

        [Fact]
        public void Create()
        {
            var aa = new AABuilder(this.Session)
                .Build();

            this.Session.Derive();

            Assert.True(aa.IsCreated);
        }
    }
}

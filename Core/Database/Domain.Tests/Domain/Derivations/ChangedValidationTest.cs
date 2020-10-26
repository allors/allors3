// <copyright file="ChangedValidationDomainDerivationTest.cs" company="Allors bvba">
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

    public class ChangedValidationDomainDerivationTest : DomainTest, IClassFixture<Fixture>
    {
        public ChangedValidationDomainDerivationTest(Fixture fixture) : base(fixture, false) { }

        [Fact]
        public void One2One()
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
        public void Many2One()
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
        public void One2Many()
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
        public void Many2Many()
        {
            var cc = new CCBuilder(this.Session)
                .Build();

            var bb = new BBBuilder(this.Session)
                .WithMany2Many(cc)
                .Build();

            var aa = new AABuilder(this.Session)
                .WithMany2Many(bb)
                .Build();

            this.Session.Derive();

            cc.Assigned = "x";

            this.Session.Derive();

            Assert.Equal("x", aa.Derived);
        }
    }
}

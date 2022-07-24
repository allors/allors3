// <copyright file="ChangedValidationDomainDerivationTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class ChangedValidationDomainDerivationTest : DomainTest, IClassFixture<Fixture>
    {
        public ChangedValidationDomainDerivationTest(Fixture fixture) : base(fixture, false) { }

        [Fact]
        public void One2One()
        {
            var cc = new CCBuilder(this.Transaction)
                .Build();

            var bb = new BBBuilder(this.Transaction)
                .WithOne2One(cc)
                .Build();

            var aa = new AABuilder(this.Transaction)
                .WithOne2One(bb)
                .Build();

            this.Transaction.Derive();

            cc.Assigned = "x";

            this.Transaction.Derive();

            Assert.Equal("x", aa.Derived);
        }

        [Fact]
        public void Many2One()
        {
            var cc = new CCBuilder(this.Transaction)
                .Build();

            var bb = new BBBuilder(this.Transaction)
                .WithMany2One(cc)
                .Build();

            var aa = new AABuilder(this.Transaction)
                .WithMany2One(bb)
                .Build();

            this.Transaction.Derive();

            cc.Assigned = "x";

            this.Transaction.Derive();

            Assert.Equal("x", aa.Derived);
        }

        [Fact]
        public void One2Many()
        {
            var cc = new CCBuilder(this.Transaction)
                .Build();

            var bb = new BBBuilder(this.Transaction)
                .WithOne2Many(cc)
                .Build();

            var aa = new AABuilder(this.Transaction)
                .WithOne2Many(bb)
                .Build();

            this.Transaction.Derive();

            cc.Assigned = "x";

            this.Transaction.Derive();

            Assert.Equal("x", aa.Derived);
        }

        [Fact]
        public void Many2Many()
        {
            var cc = new CCBuilder(this.Transaction)
                .Build();

            var bb = new BBBuilder(this.Transaction)
                .WithMany2Many(cc)
                .Build();

            var aa = new AABuilder(this.Transaction)
                .WithMany2Many(bb)
                .Build();

            this.Transaction.Derive();

            cc.Assigned = "x";

            this.Transaction.Derive();

            Assert.Equal("x", aa.Derived);
        }

        [Fact]
        public void C1ChangedRole()
        {
            var c1 = new C1Builder(this.Transaction).Build();
            var c2 = new C2Builder(this.Transaction).Build();

            c1.ChangedRolePingC1 = true;
            c2.ChangedRolePingC1 = true;

            this.Transaction.Derive();

            Assert.True(c1.ChangedRolePongC1);
            Assert.Null(c2.ChangedRolePongC1);
        }

        [Fact]
        public void I1ChangedRole()
        {
            var c1 = new C1Builder(this.Transaction).Build();
            var c2 = new C2Builder(this.Transaction).Build();

            c1.ChangedRolePingI1 = true;
            c2.ChangedRolePingI1 = true;

            this.Transaction.Derive();

            Assert.True(c1.ChangedRolePongI1);
            Assert.Null(c2.ChangedRolePongI1);
        }

        [Fact]
        public void I12ChangedRole()
        {
            var c1 = new C1Builder(this.Transaction).Build();
            var c2 = new C2Builder(this.Transaction).Build();

            c1.ChangedRolePingI12 = true;
            c2.ChangedRolePingI12 = true;

            this.Transaction.Derive();

            Assert.True(c1.ChangedRolePongI12);
            Assert.True(c2.ChangedRolePongI12);
        }

        [Fact]
        public void S12ChangedRole()
        {
            var c1 = new C1Builder(this.Transaction).Build();
            var c2 = new C2Builder(this.Transaction).Build();

            c1.ChangedRolePingS12 = true;
            c2.ChangedRolePingS12 = true;

            this.Transaction.Derive();

            Assert.True(c1.ChangedRolePongS12);
            Assert.True(c2.ChangedRolePongS12);
        }
    }
}

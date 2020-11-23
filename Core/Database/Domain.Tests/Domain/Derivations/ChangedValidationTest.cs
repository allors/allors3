// <copyright file="ChangedValidationDomainDerivationTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Domain;
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

        [Fact]
        public void C1ChangedRole()
        {
            var derivation = new C1ChangedRoleDerivation(this.M);
            this.Session.Database.AddDerivation(derivation);

            var c1 = new C1Builder(this.Session).Build();
            var c2 = new C2Builder(this.Session).Build();

            c1.ChangedRolePing = true;
            c2.ChangedRolePing = true;

            this.Session.Derive();

            Assert.True(c1.ChangedRolePong);
            Assert.Null(c2.ChangedRolePong);
        }

        [Fact]
        public void I1ChangedRole()
        {
            var derivation = new I1ChangedRoleDerivation(this.M);
            this.Session.Database.AddDerivation(derivation);

            var c1 = new C1Builder(this.Session).Build();
            var c2 = new C2Builder(this.Session).Build();

            c1.ChangedRolePing = true;
            c2.ChangedRolePing = true;

            this.Session.Derive();

            Assert.True(c1.ChangedRolePong);
            Assert.Null(c2.ChangedRolePong);
        }

        [Fact]
        public void I12ChangedRole()
        {
            var derivation = new I12ChangedRoleDerivation(this.M);
            this.Session.Database.AddDerivation(derivation);

            var c1 = new C1Builder(this.Session).Build();
            var c2 = new C2Builder(this.Session).Build();

            c1.ChangedRolePing = true;
            c2.ChangedRolePing = true;

            this.Session.Derive();

            Assert.True(c1.ChangedRolePong);
            Assert.True(c2.ChangedRolePong);
        }
        
        [Fact]
        public void S12ChangedRole()
        {
            var derivation = new S12ChangedRoleDerivation(this.M);
            this.Session.Database.AddDerivation(derivation);

            var c1 = new C1Builder(this.Session).Build();
            var c2 = new C2Builder(this.Session).Build();

            c1.ChangedRolePing = true;
            c2.ChangedRolePing = true;

            this.Session.Derive();

            Assert.True(c1.ChangedRolePong);
            Assert.True(c2.ChangedRolePong);
        }
    }
}

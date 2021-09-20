// <copyright file="BuilderTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Database.Data;
    using Xunit;

    public class CloneTest : DomainTest, IClassFixture<Fixture>
    {
        public CloneTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void Shallow()
        {
            var c2A = new C2Builder(this.Transaction)
                .WithC2AllorsString("c2A")
                .Build();

            var c2B = new C2Builder(this.Transaction)
                .WithC2AllorsString("c2B")
                .Build();

            var c2C = new C2Builder(this.Transaction)
                .WithC2AllorsString("c2B")
                .Build();

            var c2D = new C2Builder(this.Transaction)
                .WithC2AllorsString("c2D")
                .Build();

            var c1A = new C1Builder(this.Transaction)
                .WithC1AllorsString("c1A")
                .WithC1C2One2One(c2A)
                .WithC1C2One2Many(c2B)
                .WithC1C2Many2One(c2C)
                .WithC1C2Many2Many(c2D)
                .Build();

            var cloned = c1A.Clone();

            Assert.NotEqual(c1A, cloned);
            Assert.Equal("c1A", cloned.C1AllorsString);
            Assert.Null(cloned.C1C2One2One);
            Assert.Empty(cloned.C1C2One2Manies);
            Assert.Equal(c2C, cloned.C1C2Many2One);
            Assert.Contains(c2D, cloned.C1C2Many2Manies);
        }

        [Fact]
        public void DeepLevelOne()
        {
            var c2A = new C2Builder(this.Transaction)
                .WithC2AllorsString("c2A")
                .Build();

            var c2B = new C2Builder(this.Transaction)
                .WithC2AllorsString("c2B")
                .Build();

            var c2C = new C2Builder(this.Transaction)
                .WithC2AllorsString("c2B")
                .Build();

            var c2D = new C2Builder(this.Transaction)
                .WithC2AllorsString("c2D")
                .Build();

            var c1A = new C1Builder(this.Transaction)
                .WithC1AllorsString("c1A")
                .WithC1C2One2One(c2A)
                .WithC1C2One2Many(c2B)
                .WithC1C2Many2One(c2C)
                .WithC1C2Many2Many(c2D)
                .Build();

            var cloned = c1A.Clone(this.M.C1.Nodes(
                v => v.C1C2One2One.Node(),
                v => v.C1C2One2Manies.Node(),
                v => v.C1C2Many2One.Node(),
                v => v.C1C2Many2Manies.Node()));

            Assert.NotEqual(c1A, cloned);
            Assert.Equal("c1A", cloned.C1AllorsString);
            Assert.NotNull(cloned.C1C2One2One);
            Assert.NotEmpty(cloned.C1C2One2Manies);
            Assert.NotNull(cloned.C1C2Many2One);
            Assert.NotEmpty(cloned.C1C2Many2Manies);

            Assert.NotEqual(c2A, cloned.C1C2One2One);
            Assert.DoesNotContain(c2B, cloned.C1C2One2Manies);
            Assert.NotEqual(c2C, cloned.C1C2Many2One);
            Assert.DoesNotContain(c2D, cloned.C1C2Many2Manies);
        }

        [Fact]
        public void DeepLevelTwo()
        {
            var c2C = new C2Builder(this.Transaction)
                .WithC2AllorsString("c2C")
                .Build();

            var c2B = new C2Builder(this.Transaction)
                .WithC2AllorsString("c2B")
                .Build();

            var c2A = new C2Builder(this.Transaction)
                .WithC2AllorsString("c2A")
                .WithC2C2One2One(c2B)
                .WithC2C2Many2One(c2C)
                .Build();

            var c1A = new C1Builder(this.Transaction)
                .WithC1AllorsString("c1A")
                .WithC1C2One2One(c2A)
                .Build();

            var deepClone = this.M.C1.Node(v => v.C1C2One2One.Node(w => w.C2.C2C2One2One.Node()));

            var cloned1A = c1A.Clone(deepClone);
            var cloned = cloned1A.C1C2One2One;

            Assert.NotNull(cloned.C2C2One2One);
            Assert.NotNull(cloned.C2C2Many2One);

            Assert.NotEqual(c2B, cloned.C2C2One2One);
            Assert.Equal(c2C, cloned.C2C2Many2One);
        }
    }
}

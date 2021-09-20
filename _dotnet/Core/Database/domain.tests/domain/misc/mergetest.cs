// <copyright file="MergeTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class MergeTest : DomainTest, IClassFixture<Fixture>
    {
        public MergeTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void NotMergeUnitRoleWhenExist()
        {
            var c1A = new C1Builder(this.Transaction)
                .WithC1AllorsString("c1A")
                .Build();

            var c1B = new C1Builder(this.Transaction)
                .WithC1AllorsString("c1B")
                .Build();

            c1B.Merge(c1A);

            Assert.Equal("c1A", c1A.C1AllorsString);
        }

        [Fact]
        public void MergeUnitRole()
        {
            var c1A = new C1Builder(this.Transaction)
                .Build();

            var c1B = new C1Builder(this.Transaction)
                .WithC1AllorsString("c1B")
                .Build();

            c1B.Merge(c1A);

            Assert.Equal("c1B", c1A.C1AllorsString);
        }

        [Fact]
        public void NotMergeOneToOneRoleWhenExist()
        {
            var c2A = new C2Builder(this.Transaction)
                .Build();

            var c1A = new C1Builder(this.Transaction)
                .WithC1C2One2One(c2A)
                .Build();

            var c2B = new C2Builder(this.Transaction)
                .Build();

            var c1B = new C1Builder(this.Transaction)
                .WithC1C2One2One(c2B)
                .Build();

            c1B.Merge(c1A);

            Assert.Equal(c2A, c1A.C1C2One2One);
        }

        [Fact]
        public void MergeOneToOne()
        {
            var c1A = new C1Builder(this.Transaction)
                .Build();

            var c2B = new C2Builder(this.Transaction)
                .Build();

            var c1B = new C1Builder(this.Transaction)
                .WithC1C2One2One(c2B)
                .Build();

            c1B.Merge(c1A);

            Assert.Equal(c2B, c1A.C1C2One2One);
        }

        [Fact]
        public void NotMergeManyToOneRoleWhenExist()
        {
            var c2A = new C2Builder(this.Transaction)
                .Build();

            var c1A = new C1Builder(this.Transaction)
                .WithC1C2Many2One(c2A)
                .Build();

            var c2B = new C2Builder(this.Transaction)
                .Build();

            var c1B = new C1Builder(this.Transaction)
                .WithC1C2Many2One(c2B)
                .Build();

            c1B.Merge(c1A);

            Assert.Equal(c2A, c1A.C1C2Many2One);
        }

        [Fact]
        public void MergeManyToOne()
        {
            var c1A = new C1Builder(this.Transaction)
                .Build();

            var c2B = new C2Builder(this.Transaction)
                .Build();

            var c1B = new C1Builder(this.Transaction)
                .WithC1C2Many2One(c2B)
                .Build();

            c1B.Merge(c1A);

            Assert.Equal(c2B, c1A.C1C2Many2One);
        }

        [Fact]
        public void MergeOneToMany()
        {
            var c1A = new C1Builder(this.Transaction)
                .WithC1C2One2Many(new C2Builder(this.Transaction).Build())
                .WithC1C2One2Many(new C2Builder(this.Transaction).Build())
                .Build();

            var c1B = new C1Builder(this.Transaction)
                .WithC1C2One2Many(new C2Builder(this.Transaction).Build())
                .WithC1C2One2Many(new C2Builder(this.Transaction).Build())
                .WithC1C2One2Many(new C2Builder(this.Transaction).Build())
                .Build();

            c1B.Merge(c1A);

            Assert.Equal(5, c1A.C1C2One2Manies.Count());
        }

        [Fact]
        public void MergeManyToManyWhenExist()
        {
            var c2 = new C2Builder(this.Transaction).Build();

            var c1A = new C1Builder(this.Transaction)
                .WithC1C2Many2Many(c2)
                .Build();

            var c1B = new C1Builder(this.Transaction)
                .WithC1C2Many2Many(c2)
                .Build();

            c1B.Merge(c1A);

            Assert.Single(c1A.C1C2Many2Manies);
        }

        [Fact]
        public void MergeManyToMany()
        {
            var c1A = new C1Builder(this.Transaction)
                .Build();

            var c1B = new C1Builder(this.Transaction)
                .WithC1C2Many2Many(new C2Builder(this.Transaction).Build())
                .WithC1C2Many2Many(new C2Builder(this.Transaction).Build())
                .Build();

            c1B.Merge(c1A);

            Assert.Equal(2, c1A.C1C2Many2Manies.Count());
        }

        [Fact(Skip = "TODO: Koen")]
        public void Merge()
        {
            var c1A = new C1Builder(this.Transaction).WithC1AllorsString("c1A").Build();
            var c1B = new C1Builder(this.Transaction).WithC1AllorsString("c1B").Build();

            var c2 = new C2Builder(this.Transaction)
                .WithC2C1Many2One(c1B)
                .Build();

            c1B.Merge(c1A);

            Assert.Equal(c1A, c2.C2C1Many2One);
        }
    }
}

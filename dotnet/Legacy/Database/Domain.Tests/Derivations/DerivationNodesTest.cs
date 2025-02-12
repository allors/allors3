// <copyright file="DerivationNodesTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Collections.Generic;
    using Allors.Database.Domain;
    using Derivations.Legacy;
    using Xunit;

    public class DerivationNodesTest : DomainTest, IClassFixture<Fixture>
    {
        public DerivationNodesTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void Sort()
        {
            var x = new C1Builder(this.Transaction).Build();
            var y = new C1Builder(this.Transaction).Build();
            var z = new C1Builder(this.Transaction).Build();

            x.AddDependency(y);
            y.AddDependency(z);

            var derivation = (ILegacyDerivation)this.DerivationService.CreateDerivation(this.Transaction);

            var sequence = new List<IObject>();
            derivation["sequence"] = sequence;

            derivation.Derive();

            Assert.Equal(z, sequence[0]);
            Assert.Equal(y, sequence[1]);
            Assert.Equal(x, sequence[2]);

            Assert.Equal(1, x.DerivationCount);
            Assert.Equal(1, y.DerivationCount);
            Assert.Equal(1, z.DerivationCount);
        }

        [Fact]
        public void SortDiamond()
        {
            var a = new C1Builder(this.Transaction).WithName("a").Build();
            var b = new C1Builder(this.Transaction).WithName("b").Build();
            var c = new C1Builder(this.Transaction).WithName("c").Build();
            var d = new C1Builder(this.Transaction).WithName("d").Build();

            a.AddDependency(b);
            a.AddDependency(c);

            b.AddDependency(d);
            c.AddDependency(d);

            var derivation = (ILegacyDerivation)this.DerivationService.CreateDerivation(this.Transaction);

            var sequence = new List<IObject>();
            derivation["sequence"] = sequence;

            derivation.Derive();

            Assert.Equal(d, sequence[0]);
            Assert.Equal(a, sequence[3]);

            Assert.Equal(1, a.DerivationCount);
            Assert.Equal(1, b.DerivationCount);
            Assert.Equal(1, c.DerivationCount);
            Assert.Equal(1, d.DerivationCount);
        }

        [Fact]
        public void Dependency()
        {
            var left = new LeftBuilder(this.Transaction).Build();
            var middle = new MiddleBuilder(this.Transaction).Build();
            var right = new RightBuilder(this.Transaction).Build();

            left.Middle = middle;
            middle.Right = right;

            var derivation = this.DerivationService.CreateDerivation(this.Transaction);
            derivation.Derive();
            this.Transaction.Commit();

            Assert.Equal(1, left.DerivationCount);
            Assert.Equal(1, middle.DerivationCount);
            Assert.Equal(1, right.DerivationCount);

            left.Counter += 1;
            middle.Counter += 1;
            right.Counter += 1;

            derivation = this.DerivationService.CreateDerivation(this.Transaction);
            derivation.Derive();
            this.Transaction.Commit();

            Assert.Equal(2, left.DerivationCount);
            Assert.Equal(2, middle.DerivationCount);
            Assert.Equal(2, right.DerivationCount);

            middle.Counter += 1;
            left.Counter += 1;
            right.Counter += 1;

            derivation = this.DerivationService.CreateDerivation(this.Transaction);
            derivation.Derive();
            this.Transaction.Commit();

            Assert.Equal(3, left.DerivationCount);
            Assert.Equal(3, middle.DerivationCount);
            Assert.Equal(3, right.DerivationCount);

            right.Counter += 1;
            middle.Counter += 1;
            left.Counter += 1;

            derivation = this.DerivationService.CreateDerivation(this.Transaction);
            derivation.Derive();
            this.Transaction.Commit();

            Assert.Equal(4, left.DerivationCount);
            Assert.Equal(4, middle.DerivationCount);
            Assert.Equal(4, right.DerivationCount);

            right.Counter += 1;
            left.Counter += 1;
            middle.Counter += 1;

            derivation = this.DerivationService.CreateDerivation(this.Transaction);
            derivation.Derive();
            this.Transaction.Commit();

            Assert.Equal(5, left.DerivationCount);
            Assert.Equal(5, middle.DerivationCount);
            Assert.Equal(5, right.DerivationCount);

            left.Counter += 1;
            middle.Counter += 1;

            derivation = this.DerivationService.CreateDerivation(this.Transaction);
            derivation.Derive();
            this.Transaction.Commit();

            Assert.Equal(6, left.DerivationCount);
            Assert.Equal(6, middle.DerivationCount);
            Assert.Equal(6, right.DerivationCount);

            middle.Counter += 1;
            left.Counter += 1;

            derivation = this.DerivationService.CreateDerivation(this.Transaction);
            derivation.Derive();
            this.Transaction.Commit();

            Assert.Equal(7, left.DerivationCount);
            Assert.Equal(7, middle.DerivationCount);
            Assert.Equal(7, right.DerivationCount);

            right.Counter += 1;
            middle.Counter += 1;

            derivation = this.DerivationService.CreateDerivation(this.Transaction);
            derivation.Derive();
            this.Transaction.Commit();

            Assert.Equal(8, left.DerivationCount);
            Assert.Equal(8, middle.DerivationCount);
            Assert.Equal(8, right.DerivationCount);

            middle.Counter += 1;
            right.Counter += 1;

            derivation = this.DerivationService.CreateDerivation(this.Transaction);
            derivation.Derive();
            this.Transaction.Commit();

            Assert.Equal(9, left.DerivationCount);
            Assert.Equal(9, middle.DerivationCount);
            Assert.Equal(9, right.DerivationCount);

            middle.Counter += 1;

            derivation = this.DerivationService.CreateDerivation(this.Transaction);
            derivation.Derive();
            this.Transaction.Commit();

            Assert.Equal(10, left.DerivationCount);
            Assert.Equal(10, middle.DerivationCount);
            Assert.Equal(10, right.DerivationCount);
        }

        [Fact]
        public void CreateCyclicDependencyDuringDerive()
        {
            var left = new LeftBuilder(this.Transaction).Build();

            left.CreateMiddle = true;

            this.Transaction.Derive();

            Assert.Equal(2, left.DerivationCount);
            Assert.Equal(1, left.Middle.DerivationCount);
        }

        [Fact]
        public void NoCyclicDependencyDuringSyncCreate()
        {
            var syncRoot = new SyncRootBuilder(this.Transaction).Build();

            var errorThrown = false;

            var derivation = this.DerivationService.CreateDerivation(this.Transaction);
            try
            {
                derivation.Derive();
            }
            catch (Exception)
            {
                errorThrown = true;
            }

            Assert.False(errorThrown);

            Assert.Equal(1, syncRoot.DerivationCount);
            Assert.Equal(1, syncRoot.SyncDepth1.DerivationCount);
            Assert.Equal(1, syncRoot.SyncDepth1.SyncDepth2.DerivationCount);
        }

        [Fact]
        public void NoCyclicDependencyDuringSyncExisting()
        {
            var syncRoot = new SyncRootBuilder(this.Transaction).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var errorThrown = false;

            syncRoot.SyncDepth1.SyncDepth2.Value = 1;

            var derivation = this.DerivationService.CreateDerivation(this.Transaction);
            try
            {
                derivation.Derive();
            }
            catch (Exception)
            {
                errorThrown = true;
            }

            Assert.False(errorThrown);

            Assert.Equal(2, syncRoot.DerivationCount);
            Assert.Equal(2, syncRoot.SyncDepth1.DerivationCount);
            Assert.Equal(2, syncRoot.SyncDepth1.SyncDepth2.DerivationCount);
        }
    }
}

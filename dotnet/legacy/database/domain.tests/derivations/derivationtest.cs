// <copyright file="DerivationTest.cs" company="Allors bvba">
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
    using Derivations.Legacy;
    using Xunit;

    public class DerivationTest : DomainTest, IClassFixture<Fixture>
    {
        public DerivationTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void Next()
        {
            var first = new FirstBuilder(this.Transaction).Build();

            this.Transaction.Derive();

            Assert.True(first.ExistIsDerived);
            Assert.True(first.Second.ExistIsDerived);
            Assert.True(first.Second.Third.ExistIsDerived);

            Assert.Equal(1, first.DerivationCount);
            Assert.Equal(1, first.Second.DerivationCount);
            Assert.Equal(1, first.Second.Third.DerivationCount);
        }

        [Fact]
        public void Dependency()
        {
            var dependent = new DependentBuilder(this.Transaction).Build();
            var dependee = new DependeeBuilder(this.Transaction).Build();

            dependent.Dependee = dependee;

            this.Transaction.Commit();

            dependee.Counter = 10;

            this.Transaction.Derive();

            Assert.Equal(11, dependent.Counter);
            Assert.Equal(11, dependee.Counter);
        }

        [Fact]
        public void Subdependency()
        {
            var dependent = new DependentBuilder(this.Transaction).Build();
            var dependee = new DependeeBuilder(this.Transaction).Build();
            var subdependee = new SubdependeeBuilder(this.Transaction).Build();

            dependent.Dependee = dependee;
            dependee.Subdependee = subdependee;

            this.Transaction.Commit();

            subdependee.Subcounter = 10;

            this.Transaction.Derive();

            Assert.Equal(1, dependent.Counter);
            Assert.Equal(1, dependee.Counter);

            Assert.Equal(11, dependent.Subcounter);
            Assert.Equal(11, dependee.Subcounter);
            Assert.Equal(11, subdependee.Subcounter);
        }

        [Fact]
        public void Deleted()
        {
            var dependent = new DependentBuilder(this.Transaction).Build();
            var dependee = new DependeeBuilder(this.Transaction).Build();

            dependent.Dependee = dependee;

            this.Transaction.Commit();

            dependee.DeleteDependent = true;

            this.Transaction.Derive();

            Assert.True(dependent.Strategy.IsDeleted);
            Assert.Equal(1, dependee.Counter);
        }

        [Fact]
        public void Marked()
        {
            var first = new FirstBuilder(this.Transaction).Build();

            this.Transaction.Commit();

            var derivation = (ILegacyDerivation)this.DerivationService.CreateDerivation(this.Transaction);
            derivation.Mark(first);
            derivation.Derive();

            Assert.True(first.ExistIsDerived);
            Assert.True(first.Second.ExistIsDerived);
            Assert.True(first.Second.Third.ExistIsDerived);

            Assert.Equal(1, first.DerivationCount);
            Assert.Equal(1, first.Second.DerivationCount);
            Assert.Equal(1, first.Second.Third.DerivationCount);
        }
    }
}

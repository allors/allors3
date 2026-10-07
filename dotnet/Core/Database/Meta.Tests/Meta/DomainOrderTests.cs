// <copyright file="DomainOrderTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Xunit;

    // The order of the domains: a domain before the domains it extends; branches in the id order of
    // their top domain, each branch kept whole. Names and the declared order of the superdomains play
    // no part. The graphs are hand-built, so the ids below decide the order, not the names.
    public class DomainOrderTests
    {
        [Fact]
        public void AChainIsOrderedMostDerivedFirst()
        {
            var core = new TestDomain("Core", 1);
            var identity = new TestDomain("Identity", 2).Extends(core);
            var test = new TestDomain("Test", 3).Extends(identity);

            Assert.Equal(new[] { "Test", "Identity", "Core" }, Names(DomainLinearization.Linearize(test)));
            Assert.Equal(new[] { "Test", "Identity", "Core" }, Names(DomainLinearization.Sort(new[] { core, identity, test })));
        }

        [Theory]
        [InlineData(2, 4, "Test", "Level2", "Level1", "Plugin1", "Core")]
        [InlineData(4, 2, "Test", "Plugin1", "Level2", "Level1", "Core")]
        public void ADiamondOrdersTheBranchesByTheIdOfTheirTopDomain(int level2Id, int plugin1Id, params string[] expected)
        {
            var core = new TestDomain("Core", 1);
            var level1 = new TestDomain("Level1", 3).Extends(core);
            var level2 = new TestDomain("Level2", level2Id).Extends(level1);
            var plugin1 = new TestDomain("Plugin1", plugin1Id).Extends(core);
            var test = new TestDomain("Test", 5).Extends(level2, plugin1);

            Assert.Equal(expected, Names(DomainLinearization.Linearize(test)));
            Assert.Equal(expected, Names(DomainLinearization.Sort(new[] { core, level1, level2, plugin1, test })));
        }

        [Fact]
        public void TheOrderOfADomainIsKeptInEveryDomainThatExtendsIt()
        {
            var core = new TestDomain("Core", 1);
            var level1 = new TestDomain("Level1", 3).Extends(core);
            var level2 = new TestDomain("Level2", 4).Extends(level1);
            var plugin1 = new TestDomain("Plugin1", 2).Extends(core);
            var test = new TestDomain("Test", 5).Extends(level2, plugin1);

            var order = DomainLinearization.Linearize(test);

            foreach (var domain in new IDomain[] { level1, level2, plugin1 })
            {
                var own = DomainLinearization.Linearize(domain);
                Assert.Equal(Names(own), Names(order.Where(own.Contains)));
            }
        }

        [Fact]
        public void ASuperdomainThatAnotherSuperdomainExtendsIsDropped()
        {
            // By id Core would come before Level1, but Level1 already extends Core.
            var core = new TestDomain("Core", 1);
            var level1 = new TestDomain("Level1", 2).Extends(core);
            var test = new TestDomain("Test", 3).Extends(core, level1);

            Assert.Equal(new[] { "Test", "Level1", "Core" }, Names(DomainLinearization.Linearize(test)));
        }

        [Fact]
        public void TheOrderDependsOnNeitherTheNamesNorTheDeclaredOrderOfTheSuperdomains()
        {
            var expected = Ids(DomainLinearization.Linearize(Diamond(("Core", 1), ("Level1", 3), ("Level2", 4), ("Plugin1", 2), ("Test", 5), shuffled: false)));

            var shuffled = Diamond(("Core", 1), ("Level1", 3), ("Level2", 4), ("Plugin1", 2), ("Test", 5), shuffled: true);
            var renamed = Diamond(("Zebra", 1), ("Apple", 3), ("Mango", 4), ("Kiwi", 2), ("Fig", 5), shuffled: false);

            Assert.Equal(expected, Ids(DomainLinearization.Linearize(shuffled)));
            Assert.Equal(expected, Ids(DomainLinearization.Linearize(renamed)));
        }

        [Fact]
        public void TheIdsCompareAsWritten()
        {
            // 8… sorts after 7…, as the written ids do; read as a signed number it would come first.
            var core = new TestDomain("Core", Guid.Parse("00000000-0000-0000-0000-000000000001"));
            var seven = new TestDomain("Seven", Guid.Parse("7fffffff-ffff-ffff-ffff-ffffffffffff")).Extends(core);
            var eight = new TestDomain("Eight", Guid.Parse("80000000-0000-0000-0000-000000000000")).Extends(core);
            var test = new TestDomain("Test", Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff")).Extends(eight, seven);

            Assert.Equal(new[] { "Test", "Seven", "Eight", "Core" }, Names(DomainLinearization.Linearize(test)));
        }

        [Fact]
        public void OppositeOrdersInTheSuperdomainsAreAnError()
        {
            // Sales puts Level1 before Plugin1 (Level2 sorts first and brings Level1), Stock puts Plugin1 before Level1.
            var core = new TestDomain("Core", 1);
            var level1 = new TestDomain("Level1", 4).Extends(core);
            var level2 = new TestDomain("Level2", 2).Extends(level1);
            var plugin1 = new TestDomain("Plugin1", 3).Extends(core);
            var sales = new TestDomain("Sales", 5).Extends(level2, plugin1);
            var stock = new TestDomain("Stock", 6).Extends(level1, plugin1);
            var custom = new TestDomain("Custom", 7).Extends(sales, stock);

            var exception = Assert.Throws<Exception>(() => DomainLinearization.Linearize(custom));

            Assert.Equal(
                "Custom cannot order its superdomains: Stock orders Plugin1 before Level1 and Sales orders Level1 before Plugin1. " +
                "Change the structure of the domains so that these orders agree, for example by letting one of them extend the other.",
                exception.Message);
        }

        [Fact]
        public void TheSameStructureWithOtherIdsIsAccepted()
        {
            var core = new TestDomain("Core", 1);
            var level1 = new TestDomain("Level1", 4).Extends(core);
            var level2 = new TestDomain("Level2", 3).Extends(level1);
            var plugin1 = new TestDomain("Plugin1", 2).Extends(core);
            var sales = new TestDomain("Sales", 5).Extends(level2, plugin1);
            var stock = new TestDomain("Stock", 6).Extends(level1, plugin1);
            var custom = new TestDomain("Custom", 7).Extends(sales, stock);

            Assert.Equal(
                new[] { "Custom", "Sales", "Stock", "Plugin1", "Level2", "Level1", "Core" },
                Names(DomainLinearization.Linearize(custom)));
        }

        [Fact]
        public void ACycleIsAnError()
        {
            var a = new TestDomain("A", 1);
            var b = new TestDomain("B", 2).Extends(a);
            a.Extends(b);
            var test = new TestDomain("Test", 3).Extends(a);

            var exception = Assert.Throws<Exception>(() => DomainLinearization.Linearize(test));

            Assert.Equal("The domains have a cycle: A extends B and B extends A. Remove one of these [Extends].", exception.Message);
            Assert.Throws<Exception>(() => DomainLinearization.Sort(new[] { a, b, test }));
        }

        [Fact]
        public void ADomainThatExtendsItselfIsACycle()
        {
            var a = new TestDomain("A", 1);
            a.Extends(a);

            var exception = Assert.Throws<Exception>(() => DomainLinearization.Linearize(a));

            Assert.Equal("The domains have a cycle: A extends A. Remove one of these [Extends].", exception.Message);
        }

        [Fact]
        public void TwoDomainsThatNoDomainExtendsAreAnError()
        {
            var core = new TestDomain("Core", 1);
            var sales = new TestDomain("Sales", 2).Extends(core);
            var stock = new TestDomain("Stock", 3).Extends(core);

            var exception = Assert.Throws<Exception>(() => DomainLinearization.Sort(new[] { core, sales, stock }));

            Assert.Equal(
                "A population has one domain that no other domain extends, but here no domain extends Sales and Stock. " +
                "Let one of them extend the other, or add a domain that extends both.",
                exception.Message);
        }

        [Fact]
        public void APopulationWithOneDomainOrNoneSortsToItself()
        {
            var core = new TestDomain("Core", 1);

            Assert.Equal(new[] { "Core" }, Names(DomainLinearization.Sort(new[] { core })));
            Assert.Empty(DomainLinearization.Sort(Array.Empty<IDomain>()));
        }

        private static IDomain Diamond((string Name, int Id) core, (string Name, int Id) level1, (string Name, int Id) level2, (string Name, int Id) plugin1, (string Name, int Id) test, bool shuffled)
        {
            var coreDomain = new TestDomain(core.Name, core.Id);
            var level1Domain = new TestDomain(level1.Name, level1.Id).Extends(coreDomain);
            var level2Domain = new TestDomain(level2.Name, level2.Id).Extends(level1Domain);
            var plugin1Domain = new TestDomain(plugin1.Name, plugin1.Id).Extends(coreDomain);
            return shuffled
                ? new TestDomain(test.Name, test.Id).Extends(plugin1Domain, level2Domain)
                : new TestDomain(test.Name, test.Id).Extends(level2Domain, plugin1Domain);
        }

        private static string[] Names(IEnumerable<IDomain> domains) => domains.Select(v => v.Name).ToArray();

        private static Guid[] Ids(IEnumerable<IDomain> domains) => domains.Select(v => v.Id).ToArray();

        private sealed class TestDomain : IDomain
        {
            private readonly List<IDomain> directSuperdomains = new List<IDomain>();

            public TestDomain(string name, int id)
                : this(name, new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)id))
            {
            }

            public TestDomain(string name, Guid id)
            {
                this.Name = name;
                this.Id = id;
            }

            public Guid Id { get; }

            public string Tag => this.Id.ToString("N");

            public string Name { get; }

            public IEnumerable<IDomain> DirectSuperdomains => this.directSuperdomains;

            public IMetaPopulation MetaPopulation => null;

            public Origin Origin => Origin.Database;

            public TestDomain Extends(params IDomain[] superdomains)
            {
                this.directSuperdomains.AddRange(superdomains);
                return this;
            }

            public int CompareTo(object other) => this.Id.CompareTo(((IDomain)other).Id);

            public override string ToString() => this.Name;
        }
    }
}

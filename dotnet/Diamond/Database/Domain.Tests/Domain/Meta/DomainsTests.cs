// <copyright file="DomainsTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Meta;
    using Xunit;

    // The population is a diamond: Core <- Level1 <- Level2 <- Test and Core <- Plugin1 <- Test.
    // These tests are the end-to-end proof of [Extends] with two names: the parser reads both,
    // the generated MetaBuilder declares both, and the meta population orders the five domains.
    public class DomainsTests : DomainTest, IClassFixture<Fixture>
    {
        public DomainsTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ThePopulationHoldsTheFiveDomains() =>
            Assert.Equal(new[] { "Core", "Level1", "Level2", "Plugin1", "Test" }, this.M.Domains.Select(v => v.Name).OrderBy(v => v));

        [Fact]
        public void TestExtendsLevel2AndPlugin1()
        {
            Assert.Empty(this.DirectSuperdomains("Core"));
            Assert.Equal(new[] { "Core" }, this.DirectSuperdomains("Level1"));
            Assert.Equal(new[] { "Level1" }, this.DirectSuperdomains("Level2"));
            Assert.Equal(new[] { "Core" }, this.DirectSuperdomains("Plugin1"));
            Assert.Equal(new[] { "Level2", "Plugin1" }, this.DirectSuperdomains("Test"));
        }

        [Fact]
        public void TheSuperdomainsOfTestReachCoreThroughBothBranches() =>
            Assert.Equal(new[] { "Core", "Level1", "Level2", "Plugin1" }, this.Superdomains("Test"));

        // The order of the domains: a domain before the domains it extends; branches in the id
        // order of their top domain, each branch kept whole. The ids were generated, not chosen:
        // Plugin1's id sorts before Level2's, so the branch of Plugin1 comes before the branch of
        // Level2, and Level1 follows Level2 because its branch is kept whole.
        [Fact]
        public void TheSortedDomainsFollowTheIdOrderOfTheBranches()
        {
            Assert.True(this.Domain("Plugin1").Id.CompareTo(this.Domain("Level2").Id) < 0, "The id of Plugin1 sorts before the id of Level2.");

            Assert.Equal(new[] { "Test", "Plugin1", "Level2", "Level1", "Core" }, this.M.SortedDomains.Select(v => v.Name));
        }

        [Fact]
        public void TheSuperdomainsOfADomainAreOrderedAsTheSortedDomainsAre()
        {
            var sorted = this.M.SortedDomains.Select(v => v.Name).ToArray();

            foreach (var domain in this.M.Domains)
            {
                var superdomains = domain.Superdomains.Select(v => v.Name).ToArray();
                Assert.Equal(superdomains, sorted.Where(superdomains.Contains));
            }
        }

        [Fact]
        public void TheOrderOfADomainIsKeptInEveryDomainThatExtendsIt()
        {
            var sorted = this.M.SortedDomains.ToArray();

            foreach (var domain in this.M.Domains)
            {
                var own = DomainLinearization.Linearize(domain);
                Assert.Equal(own.Select(v => v.Name), sorted.Where(own.Contains).Select(v => v.Name));
            }
        }

        private string[] DirectSuperdomains(string name) => this.Domain(name).DirectSuperdomains.Select(v => v.Name).OrderBy(v => v).ToArray();

        private string[] Superdomains(string name) => this.Domain(name).Superdomains.Select(v => v.Name).OrderBy(v => v).ToArray();

        private IDomainBase Domain(string name) => this.M.Domains.Single(v => v.Name == name);
    }
}

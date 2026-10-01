// <copyright file="DomainsTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Meta;
    using Xunit;

    // The tree's concrete domain selects the Identity plug-in by extending it: Core <- Identity <- Test.
    public class DomainsTests : DomainTest, IClassFixture<Fixture>
    {
        public DomainsTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ThePopulationHoldsCoreIdentityAndTest() =>
            Assert.Equal(new[] { "Core", "Identity", "Test" }, this.M.Domains.Select(v => v.Name).OrderBy(v => v));

        [Fact]
        public void IdentityExtendsCoreAndTestExtendsIdentity()
        {
            Assert.Empty(this.DirectSuperdomains("Core"));
            Assert.Equal(new[] { "Core" }, this.DirectSuperdomains("Identity"));
            Assert.Equal(new[] { "Identity" }, this.DirectSuperdomains("Test"));
        }

        [Fact]
        public void TheSuperdomainsOfTestReachDownToCore() =>
            Assert.Equal(new[] { "Core", "Identity" }, this.Superdomains("Test"));

        private string[] DirectSuperdomains(string name) => this.Domain(name).DirectSuperdomains.Select(v => v.Name).OrderBy(v => v).ToArray();

        private string[] Superdomains(string name) => this.Domain(name).Superdomains.Select(v => v.Name).OrderBy(v => v).ToArray();

        private IDomainBase Domain(string name) => this.M.Domains.Single(v => v.Name == name);
    }
}

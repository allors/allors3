// <copyright file="SetupOrderTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Configuration.Derivations.Default;
    using Xunit;

    // The hand-written shims in Virtual/*.v.cs dispatch each phase of the setup and of the security
    // to the hooks of the domains, base first: the order of MetaPopulation.SortedDomains reversed,
    // Level1, Level2, Plugin1, Test, after Core, which does not record. A shim that skips a hook or
    // calls the hooks in another order fails here.
    public class SetupOrderTests : DomainTest, IClassFixture<Fixture>
    {
        public SetupOrderTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Theory]
        [InlineData("Setup.OnPrePrepare")]
        [InlineData("Setup.OnPostPrepare")]
        [InlineData("Setup.OnPreSetup")]
        [InlineData("Setup.OnPostSetup")]
        [InlineData("Security.OnPreSetup")]
        [InlineData("Security.OnPostSetup")]
        public void APhaseOfTheSetupRunsTheHooksOfTheDomainsBaseFirst(string hook)
        {
            var domains = this.HookLog.Entries.Where(v => v.Hook == hook).Select(v => v.Domain).ToArray();

            Assert.Equal(this.SortedDomainsReversedWithoutCore(), domains);
        }

        [Theory]
        [InlineData("ObjectsBase.Prepare(Setup)")]
        [InlineData("ObjectsBase.Setup(Setup)")]
        [InlineData("ObjectsBase.Prepare(Security)")]
        [InlineData("ObjectsBase.Secure(Security)")]
        public void APhaseOfTheObjectsOfEveryTypeRunsTheHooksOfTheDomainsBaseFirst(string hook)
        {
            var domainsByObjects = this.HookLog.Entries
                .Where(v => v.Hook == hook)
                .GroupBy(v => (IObjects)v.Subject)
                .ToDictionary(v => v.Key, v => v.Select(w => w.Domain).ToArray());

            Assert.Contains(domainsByObjects.Keys, v => v.ObjectType.Equals(this.M.Level1Item));
            Assert.Contains(domainsByObjects.Keys, v => v.ObjectType.Equals(this.M.Person));

            foreach (var (objects, domains) in domainsByObjects)
            {
                Assert.True(this.SortedDomainsReversedWithoutCore().SequenceEqual(domains), $"{objects.ObjectType.Name}: {string.Join(", ", domains)}");
            }
        }

        private string[] SortedDomainsReversedWithoutCore() => this.M.SortedDomains.Reverse().Select(v => v.Name).Where(v => v != "Core").ToArray();
    }
}

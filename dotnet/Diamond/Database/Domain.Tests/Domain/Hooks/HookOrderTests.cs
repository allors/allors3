// <copyright file="HookOrderTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    // Every domain but Core hooks OnPostBuild of Object and records that it ran. Allors runs the
    // hooks in the order of the domains, most derived first: Test, Plugin1, Level2, Level1, then
    // Core, which does not record. The log is read for the object built, because the hooks of
    // Core build a grant and a security token for a new user and the hooks record those too.
    public class HookOrderTests : DomainTest, IClassFixture<Fixture>
    {
        public HookOrderTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnPostBuildOfAPersonRunsTheHooksMostDerivedFirst()
        {
            var person = new PersonBuilder(this.Transaction).Build();

            Assert.Equal(this.SortedDomainsWithoutCore(), this.OnPostBuildHooks(person));
        }

        [Fact]
        public void OnPostBuildOfALevel1ItemRunsTheHooksMostDerivedFirst()
        {
            var item = new Level1ItemBuilder(this.Transaction).Build();

            Assert.Equal(this.SortedDomainsWithoutCore(), this.OnPostBuildHooks(item));
        }

        [Fact]
        public void TheHooksOfCoreRunAsWell()
        {
            var person = new PersonBuilder(this.Transaction).Build();

            Assert.True(person.ExistOwnerGrant);
            Assert.True(person.ExistOwnerSecurityToken);
        }

        private string[] SortedDomainsWithoutCore() => this.M.SortedDomains.Select(v => v.Name).Where(v => v != "Core").ToArray();

        private string[] OnPostBuildHooks(IObject @object) =>
            this.HookLog.Entries
                .Where(v => v.Hook == "Object.OnPostBuild" && @object.Equals(v.Subject))
                .Select(v => v.Domain)
                .ToArray();
    }
}

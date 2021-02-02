// <copyright file="OrganisationRollUpTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class OrganisationRollUpTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationRollUpTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedParentDeriveParties()
        {
            var parent = new OrganisationBuilder(this.Session).Build();
            var child = new OrganisationBuilder(this.Session).Build();
            var rollup = new OrganisationRollUpBuilder(this.Session).WithChild(child).Build();
            this.Session.Derive(false);

            rollup.Parent = parent;
            this.Session.Derive(false);

            Assert.Contains(parent, rollup.Parties);
        }

        [Fact]
        public void ChangedChildDeriveParties()
        {
            var parent = new OrganisationBuilder(this.Session).Build();
            var child = new OrganisationBuilder(this.Session).Build();
            var rollup = new OrganisationRollUpBuilder(this.Session).WithParent(parent).Build();
            this.Session.Derive(false);

            rollup.Child = child;
            this.Session.Derive(false);

            Assert.Contains(child, rollup.Parties);
        }
    }
}

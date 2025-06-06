// <copyright file="CacheTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class CacheTest : DomainTest, IClassFixture<Fixture>
    {
        public CacheTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void Default()
        {
            var existingOrganisation = new OrganisationBuilder(this.Transaction).WithName("existing organisation").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            foreach (var session in new[] { this.Transaction })
            {
                session.Commit();

                var cachedOrganisation = new Organisations(session).Cache[existingOrganisation.UniqueId];
                Assert.Equal(existingOrganisation.UniqueId, cachedOrganisation.UniqueId);
                Assert.Same(session, cachedOrganisation.Strategy.Transaction);

                var newOrganisation = new OrganisationBuilder(session).WithName("new organisation").Build();
                cachedOrganisation = new Organisations(session).Cache[newOrganisation.UniqueId];
                Assert.Equal(newOrganisation.UniqueId, cachedOrganisation.UniqueId);
                Assert.Same(session, cachedOrganisation.Strategy.Transaction);

                session.Rollback();
            }
        }
    }
}

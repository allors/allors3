// <copyright file="CacheTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using Xunit;

    public class CacheTest : Adapters.CacheTest, IClassFixture<Fixture<CacheTest>>
    {
        private readonly Profile profile;

        public CacheTest() => this.profile = new Profile(this.GetType().Name);

        public override void Dispose() => this.profile.Dispose();

        protected override IDatabase CreateDatabase() => this.profile.CreateDatabase();
    }
}

// <copyright file="CacheTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using Xunit;

    public class TracingTest : Sql.TracingTest, IClassFixture<Fixture<TracingTest>>
    {
        private readonly Profile profile;

        public TracingTest() => this.profile = new Profile(this.GetType().Name);

        public override void Dispose() => this.profile.Dispose();

        protected override IDatabase CreateDatabase() => this.profile.CreateDatabase();
    }
}

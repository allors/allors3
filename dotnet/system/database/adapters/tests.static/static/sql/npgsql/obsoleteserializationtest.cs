// <copyright file="SerializationTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using Xunit;
    using Adapters;

    public class ObsoleteSerializationTest : Adapters.ObsoleteSerializationTest, IClassFixture<Fixture<ObsoleteSerializationTest>>
    {
        private readonly Profile profile;

        public ObsoleteSerializationTest() => this.profile = new Profile(this.GetType().Name);

        protected override IProfile Profile => this.profile;

        public override void Dispose() => this.profile.Dispose();

        protected override IDatabase CreatePopulation() => this.profile.CreateMemoryDatabase();
    }
}

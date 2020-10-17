// <copyright file="SerializationTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.SqlClient
{
    using Allors;
    using Adapters;
    using Xunit;

    public class SerializationTest : Adapters.SerializationTest, IClassFixture<Fixture<SerializationTest>>
    {
        private readonly Profile profile;

        public SerializationTest() => this.profile = new Profile(this.GetType().Name);

        protected override IProfile Profile => this.profile;

        public override void Dispose() => this.profile.Dispose();

        protected override IDatabase CreatePopulation() => this.profile.CreateMemoryDatabase();
    }
}

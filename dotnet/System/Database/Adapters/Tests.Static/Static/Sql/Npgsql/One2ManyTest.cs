// <copyright file="One2ManyTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using Xunit;
    using Adapters;

    public class One2ManyTest : Adapters.One2ManyTest, IClassFixture<Fixture<One2ManyTest>>
    {
        private readonly Profile profile;

        public One2ManyTest() => this.profile = new Profile(this.GetType().Name);

        protected override IProfile Profile => this.profile;

        public override void Dispose() => this.profile.Dispose();
    }
}

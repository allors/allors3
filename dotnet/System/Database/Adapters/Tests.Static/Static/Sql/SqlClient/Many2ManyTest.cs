// <copyright file="Many2ManyTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the Default type.
// </summary>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using Adapters;
    using Xunit;

    public class Many2ManyTest : Adapters.Many2ManyTest, IClassFixture<Fixture<Many2ManyTest>>
    {
        private readonly Profile profile;

        public Many2ManyTest() => this.profile = new Profile(this.GetType().Name);

        protected override IProfile Profile => this.profile;

        public override void Dispose() => this.profile.Dispose();
    }
}

// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.OriginDatabase.DatabaseDatabase.Local
{
    using Xunit;

    public class OneToManyTests : DatabaseDatabase.OneToManyTests, IClassFixture<Fixture>
    {
        public OneToManyTests(Fixture fixture) : base(fixture) => this.Profile = new Workspace.Local.Profile(fixture);

        public override IProfile Profile { get; }
    }
}

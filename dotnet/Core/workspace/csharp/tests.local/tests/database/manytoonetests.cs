// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.DatabaseAssociation.DatabaseRelation.DatabaseRole.Local
{
    using Workspace.Local;
    using Xunit;

    public class ManyToOneTests : DatabaseRole.ManyToOneTests, IClassFixture<Fixture>
    {
        public ManyToOneTests(Fixture fixture) : base(fixture) => this.Profile = new Profile(fixture);

        public override IProfile Profile { get; }
    }
}

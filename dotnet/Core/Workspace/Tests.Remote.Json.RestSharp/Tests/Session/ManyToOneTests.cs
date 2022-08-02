// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.DatabaseAssociation.SessionRelation.Local
{
    using Workspace.Remote;
    using Xunit;

    public class ManyToOneTests : SessionRole.ManyToOneTests, IClassFixture<Fixture>
    {
        public ManyToOneTests(Fixture fixture) : base(fixture) => this.Profile = new Profile();

        public override IProfile Profile { get; }
    }
}

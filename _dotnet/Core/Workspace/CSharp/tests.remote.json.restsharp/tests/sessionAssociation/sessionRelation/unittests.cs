// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.SessionAssociation.SessionRelation.Remote
{
    using Xunit;

    public class UnitTests : SessionRelation.UnitTests, IClassFixture<Fixture>
    {
        public UnitTests(Fixture fixture) : base(fixture) => this.Profile = new Workspace.Remote.Profile();

        public override IProfile Profile { get; }
    }
}

// <copyright file="Many2OneTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.DatabaseAssociation.SessionRelation.Local
{
    using Workspace.Local;
    using Xunit;

    public class UnitTests : SessionRelation.UnitTests, IClassFixture<Fixture>
    {
        public UnitTests(Fixture fixture) : base(fixture) => this.Profile = new Profile(fixture);

        public override IProfile Profile { get; }
    }
}

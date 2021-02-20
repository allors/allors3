// <copyright file="UnitTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Local
{
    using Remote;
    using Xunit;

    public class UnitTests : Workspace.UnitTests, IClassFixture<Fixture>
    {
        public UnitTests(Fixture fixture) : base(fixture) => this.Profile = new Profile(fixture);

        protected override IProfile Profile { get; }
    }
}

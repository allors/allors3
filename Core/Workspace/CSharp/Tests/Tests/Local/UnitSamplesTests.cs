// <copyright file="UnitSamplesTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Local
{
    using Xunit;

    public class UnitSamplesTests : Workspace.UnitSamplesTests, IClassFixture<Fixture>
    {
        public UnitSamplesTests(Fixture fixture) : base(fixture) => this.Profile = new Profile(fixture);

        protected override IProfile Profile { get; }
    }
}

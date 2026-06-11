// <copyright file="SignalTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Remote
{
    using Xunit;

    public class SignalTests : Workspace.SignalTests, IClassFixture<Fixture>
    {
        public SignalTests(Fixture fixture) : base(fixture) => this.Profile = new Profile();

        public override IProfile Profile { get; }
    }
}

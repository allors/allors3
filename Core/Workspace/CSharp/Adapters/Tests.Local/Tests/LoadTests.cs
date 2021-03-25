// <copyright file="PullTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Local
{
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public class LoadTests : Test, IClassFixture<Fixture>
    {
        public LoadTests(Fixture fixture) : base(fixture, true) { }

        [Fact]
        public void Default()
        {
            var m = this.Workspace.Context().M;

            var session = this.Workspace.CreateSession();
            var pull = new Pull { Extent = new Extent(m.C1.ObjectType) };
            var result = session.Pull(pull).Result;

            var c1s = result.GetCollection<C1>();
            Assert.NotNull(c1s);
            Assert.Equal(4, c1s.Length);
        }
    }
}

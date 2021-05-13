// <copyright file="PullTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Tests.Workspace
{
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class PullTests : Test
    {
        protected PullTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async void Predicate()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Extent(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            var c1s = result.GetCollection<C1>();

            Assert.Single(c1s);

            var c1a = c1s.First();

            Assert.Equal("c1A", c1a.Name);
        }

    }
}

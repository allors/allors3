// <copyright file="PullTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Allors.Workspace.Meta;
    using Remote;
    using Xunit;
    using Result = Allors.Workspace.Data.Result;

    public abstract class TreeTests : Test
    {
        protected TreeTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void C1()
        {
            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Extent(this.M.C1.Class),
                Results = new[]
                {
                    new Result
                    {
                        Fetch = new Fetch
                        {
                            Include = new C1NodeBuilder(this.M,v => v.C1C2One2One()),
                        },
                    },
                },
            };

            var result = await session.Load(pull);

            var c1s = result.GetCollection<C1>();
            var c1b = c1s.Single(v => v.Name == "c1B");
            var c1c = c1s.Single(v => v.Name == "c1C");
            var c1d = c1s.Single(v => v.Name == "c1D");

            var c2ByC1 = c1s.ToDictionary(v => v, v => v.C1C2One2One);

            Assert.Equal("c2B", c2ByC1[c1b].Name);
            Assert.Equal("c2C", c2ByC1[c1c].Name);
            Assert.Equal("c2D", c2ByC1[c1d].Name);
        }
    }
}

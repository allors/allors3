// <copyright file="TreeTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
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
            await this.Login("administrator");
            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1),
                Results = new[]
                {
                    new Result
                    {
                        Select = new Select
                        {
                            Include = this.M.C1.Nodes(v=>v.C1C2One2One.Node())
                        }
                    }
                }
            };

            var result = await session.Pull(pull);

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

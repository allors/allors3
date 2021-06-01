// <copyright file="UnitTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class UnitTests : Test
    {
        protected UnitTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void Load()
        {
            await this.Login("administrator");

            var pull = new[]
            {
                new Pull
                {
                    Extent = new Extent(this.M.C1)
                }
            };

            var session = this.Workspace.CreateSession();
            var result = await session.Pull(pull);

            var c1s = result.GetCollection<C1>();

            var c1A = c1s.First(v => v.Name.Equals("c1A"));
            var c1B = c1s.First(v => v.Name.Equals("c1B"));
            var c1C = c1s.First(v => v.Name.Equals("c1C"));
            var c1D = c1s.First(v => v.Name.Equals("c1D"));

            Assert.Equal("ᴀbra", c1B.C1AllorsString);
        }
    }
}

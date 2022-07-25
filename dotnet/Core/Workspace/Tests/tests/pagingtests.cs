// <copyright file="PullTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Tests.Workspace
{
    using Allors.Workspace.Data;
    using Xunit;
    using I12 = Allors.Workspace.Domain.I12;

    public abstract class PagingTests : Test
    {
        protected PagingTests(Fixture fixture) : base(fixture) { }


        // Ascending Order
        // c2D -> c2C -> c1B -> c1A -> c2A -> c2B -> c1D -> c1C

        [Fact]
        public async void Take()
        {
            await this.Login("administrator");
            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.I12) { Sorting = new[] { new Sort(this.M.I12.Order) } },
                Results = new[]
                {
                    new Result
                    {
                        Take = 1
                    }
                }
            };

            var result = await session.PullAsync(pull);

            var i12s = result.GetCollection<I12>();

            Assert.Single(i12s);

            Assert.Equal("c2D", i12s[0].Name);
        }
    }
}

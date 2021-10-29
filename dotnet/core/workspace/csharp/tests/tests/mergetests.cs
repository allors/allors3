// <copyright file="ChangeSetTests.cs" company="Allors bvba">
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

    public abstract class MergeTests : Test
    {
        protected MergeTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async void DatabaseMergeError()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();
            var session2 = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };

            var result = await session1.PullAsync(pull);
            var c1a_1 = result.GetCollection<C1>()[0];

            result = await session2.PullAsync(pull);
            var c1a_2 = result.GetCollection<C1>()[0];

            c1a_1.C1AllorsString = "X";
            c1a_2.C1AllorsString = "Y";

            await session2.PushAsync();

            result = await session1.PullAsync(pull);

            Assert.True(result.HasErrors);
            Assert.Single(result.MergeErrors);

            var mergeError = result.MergeErrors.First();

            Assert.Equal(c1a_1.Strategy, mergeError.Strategy);
        }
    }
}

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
        public async void MergeTwoDifferentVersions()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();
            var session2 = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };

            var result = await this.AsyncDatabaseClient.PullAsync(session1, pull);
            var c1a = result.GetCollection<C1>()[0];

            result = await this.AsyncDatabaseClient.PullAsync(session2, pull);
            var c1b = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";
            c1b.C1AllorsString = "Y";

            Assert.Equal("X", c1a.C1AllorsString);
            Assert.Equal("Y", c1b.C1AllorsString);

            await this.AsyncDatabaseClient.PushAsync(session2);

            result = await this.AsyncDatabaseClient.PullAsync(session1, pull);

            Assert.True(result.HasErrors);
        }
    }
}

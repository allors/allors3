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
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class DiffTests : Test
    {
        protected DiffTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async void DatabaseUnitDiffTest()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a_1 = result.GetCollection<C1>()[0];

            c1a_1.C1AllorsString = "X";

            await session.PushAsync();

            result = await session.PullAsync(pull);
            var c1a_2 = result.GetCollection<C1>()[0];

            c1a_2.C1AllorsString = "Y";

            var diffs = c1a_2.Strategy.Diff();
            Assert.Single(diffs);

            var diff = (IUnitDiff)diffs[0];
            Assert.Equal("X", diff.OriginalRole);
            Assert.Equal("Y", diff.ChangedRole);
            Assert.Equal(this.M.C1.C1AllorsString.RelationType, diff.RelationType);
        }

        [Fact]
        public async void DatabaseUnitDiffAfterResetTest()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a_1 = result.GetCollection<C1>()[0];

            c1a_1.C1AllorsString = "X";

            await session.PushAsync();

            result = await session.PullAsync(pull);
            var c1a_2 = result.GetCollection<C1>()[0];

            c1a_2.C1AllorsString = "Y";

            c1a_2.Strategy.Reset();
            var diff = c1a_2.Strategy.Diff();

            Assert.Empty(diff);
        }

        [Fact]
        public async void DatabaseUnitDiffAfterDoubleResetTest()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            await session.PushAsync();

            result = await session.PullAsync(pull);
            var c1b = result.GetCollection<C1>()[0];

            c1b.C1AllorsString = "Y";

            c1b.Strategy.Reset();
            c1b.Strategy.Reset();

            var diff = c1b.Strategy.Diff();

            Assert.Empty(diff);

        }

        [Fact]
        public async void DatabaseMultipleUnitDiffTest()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";
            c1a.C1AllorsInteger = 1;

            await session.PushAsync();

            result = await session.PullAsync(pull);
            var c1b = result.GetCollection<C1>()[0];

            c1b.C1AllorsString = "Y";
            c1b.C1AllorsInteger = 2;

            var diffs = c1b.Strategy.Diff();

            Assert.Equal(2, diffs.Count);

            var stringDiff = diffs.First(v => v.RelationType == this.M.C1.C1AllorsString.RelationType) as IUnitDiff;
            var intDiff = diffs.First(v => v.RelationType == this.M.C1.C1AllorsInteger.RelationType) as IUnitDiff;

            Assert.Equal("X", stringDiff.OriginalRole);
            Assert.Equal("Y", stringDiff.ChangedRole);
            Assert.Equal(this.M.C1.C1AllorsString.RelationType, stringDiff.RelationType);

            Assert.Equal(1, intDiff.OriginalRole);
            Assert.Equal(2, intDiff.ChangedRole);
            Assert.Equal(this.M.C1.C1AllorsInteger.RelationType, intDiff.RelationType);
        }
    }
}

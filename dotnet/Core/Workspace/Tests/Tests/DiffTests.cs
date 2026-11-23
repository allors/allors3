// <copyright file="ChangeSetTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Tests.Workspace
{
    using System.Threading.Tasks;
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

        [Fact]
        public async Task DatabaseDiffRetainsBaselineWhenAnotherSessionRefreshes()
        {
            await this.Login("administrator");
            var remote = this.Workspace.CreateSession();
            var editing = this.Workspace.CreateSession();
            var pull = new Pull { Extent = new Filter(this.M.C1) };
            var objects = (await remote.PullAsync(pull)).GetCollection<C1>();
            var source = objects.Single(v => v.Name == "c1A");
            var originalTarget = objects.Single(v => v.Name == "c1B");
            var remoteTarget = objects.Single(v => v.Name == "c1D");
            source.C1AllorsString = "baseline";
            source.C1AllorsInteger = 1;
            source.C1C1Many2One = originalTarget;
            source.C1C1Many2Manies = new[] { originalTarget };
            Assert.False((await remote.PushAsync()).HasErrors);
            Assert.False((await remote.PullAsync(pull)).HasErrors);
            objects = (await editing.PullAsync(pull)).GetCollection<C1>();
            var edited = objects.Single(v => v.Name == "c1A");
            var baselineTarget = objects.Single(v => v.Name == "c1B");
            var localTarget = objects.Single(v => v.Name == "c1C");
            var version = edited.Strategy.Version;
            edited.C1AllorsString = "local";
            edited.C1C1Many2One = localTarget;
            edited.AddC1C1Many2Many(localTarget);

            source.C1AllorsString = "remote";
            source.C1AllorsInteger = 2;
            source.C1C1Many2One = remoteTarget;
            source.AddC1C1Many2Many(remoteTarget);
            Assert.False((await remote.PushAsync()).HasErrors);
            Assert.False((await remote.PullAsync(pull)).HasErrors);
            Assert.NotEqual(version, source.Strategy.Version);

            // The shared connection has the new record, but editing has not pulled it.
            Assert.Equal(version, edited.Strategy.Version);
            Assert.Equal(1, edited.C1AllorsInteger);
            Assert.Equal("local", edited.C1AllorsString);
            Assert.Same(localTarget, edited.C1C1Many2One);
            Assert.Equal(new[] { baselineTarget.Id, localTarget.Id }.OrderBy(v => v), edited.C1C1Many2Manies.Select(v => v.Id).OrderBy(v => v));
            var diffs = edited.Strategy.Diff();
            Assert.Equal(3, diffs.Count);
            var unit = (IUnitDiff)diffs.Single(v => v.RelationType == this.M.C1.C1AllorsString.RelationType);
            Assert.Equal("baseline", unit.OriginalRole);
            Assert.Equal("local", unit.ChangedRole);
            var one = (ICompositeDiff)diffs.Single(v => v.RelationType == this.M.C1.C1C1Many2One.RelationType);
            Assert.Same(baselineTarget.Strategy, one.OriginalRole);
            Assert.Same(localTarget.Strategy, one.ChangedRole);
            var many = (ICompositesDiff)diffs.Single(v => v.RelationType == this.M.C1.C1C1Many2Manies.RelationType);
            Assert.Same(baselineTarget.Strategy, Assert.Single(many.OriginalRoles));
            Assert.Equal(new[] { baselineTarget.Id, localTarget.Id }.OrderBy(v => v), many.ChangedRoles.Select(v => v.Id).OrderBy(v => v));

            var pushed = await editing.PushAsync();
            Assert.True(pushed.HasErrors);
            Assert.Same(edited, Assert.Single(pushed.VersionErrors));
            Assert.Equal(version, edited.Strategy.Version);
            Assert.Equal("local", edited.C1AllorsString);
            Assert.Equal(3, edited.Strategy.Diff().Count);

            edited.Strategy.Reset();
            Assert.Equal(version, edited.Strategy.Version);
            Assert.Equal("baseline", edited.C1AllorsString);
            Assert.Equal(1, edited.C1AllorsInteger);
            Assert.Same(baselineTarget, edited.C1C1Many2One);
            Assert.Same(baselineTarget, Assert.Single(edited.C1C1Many2Manies));
            Assert.Empty(edited.Strategy.Diff());
            Assert.False(edited.Strategy.HasChanges);
            Assert.False((await editing.PullAsync(pull)).HasErrors);
            Assert.Equal(source.Strategy.Version, edited.Strategy.Version);
            Assert.Equal("remote", edited.C1AllorsString);
            Assert.Equal(2, edited.C1AllorsInteger);
            Assert.Equal(remoteTarget.Id, edited.C1C1Many2One.Id);
            Assert.Equal(source.C1C1Many2Manies.Select(v => v.Id).OrderBy(v => v), edited.C1C1Many2Manies.Select(v => v.Id).OrderBy(v => v));
            Assert.Empty(edited.Strategy.Diff());
        }
    }
}

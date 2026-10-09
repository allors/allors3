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
    using Allors.Workspace;
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using IRoleType = Allors.Workspace.Meta.IRoleType;
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

        [Theory]
        [InlineData("disjoint")]
        [InlineData("string")]
        [InlineData("to-one")]
        [InlineData("to-many")]
        public async Task DatabaseEditingBaselineAfterPull(string remoteEdit)
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

            source.C1AllorsInteger = 2;
            switch (remoteEdit)
            {
                case "string": source.C1AllorsString = "remote"; break;
                case "to-one": source.C1C1Many2One = remoteTarget; break;
                case "to-many": source.AddC1C1Many2Many(remoteTarget); break;
            }

            Assert.False((await remote.PushAsync()).HasErrors);
            Assert.False((await remote.PullAsync(pull)).HasErrors);
            Assert.NotEqual(version, source.Strategy.Version);
            var result = await editing.PullAsync(pull);
            var conflict = remoteEdit != "disjoint";
            Assert.Equal(conflict, result.HasErrors);
            if (conflict)
            {
                Assert.Same(edited.Strategy, Assert.Single(result.MergeErrors).Strategy);
            }
            else
            {
                Assert.Empty(result.MergeErrors);
            }

            // A conflicting role rejects the entire replacement record for this object.
            Assert.Equal(conflict ? version : source.Strategy.Version, edited.Strategy.Version);
            Assert.Equal(conflict ? 1 : 2, edited.C1AllorsInteger);
            Assert.Equal("local", edited.C1AllorsString);
            Assert.Same(localTarget, edited.C1C1Many2One);
            Assert.Equal(new[] { baselineTarget.Id, localTarget.Id }.OrderBy(v => v), edited.C1C1Many2Manies.Select(v => v.Id).OrderBy(v => v));
            this.AssertEditingDiff(edited, baselineTarget, localTarget);

            if (!conflict)
            {
                // Push must now use the accepted version, preserving the remote integer.
                Assert.False((await editing.PushAsync()).HasErrors);
                Assert.False((await editing.PullAsync(pull)).HasErrors);
                Assert.Empty(edited.Strategy.Diff());
                Assert.False((await remote.PullAsync(pull)).HasErrors);
                Assert.Equal("local", source.C1AllorsString);
                Assert.Equal(2, source.C1AllorsInteger);
                Assert.Equal(localTarget.Id, source.C1C1Many2One.Id);
                Assert.Equal(new[] { baselineTarget.Id, localTarget.Id }.OrderBy(v => v), source.C1C1Many2Manies.Select(v => v.Id).OrderBy(v => v));
                return;
            }

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
            Assert.Equal(source.C1AllorsString, edited.C1AllorsString);
            Assert.Equal(2, edited.C1AllorsInteger);
            Assert.Equal(source.C1C1Many2One.Id, edited.C1C1Many2One.Id);
            Assert.Equal(source.C1C1Many2Manies.Select(v => v.Id).OrderBy(v => v), edited.C1C1Many2Manies.Select(v => v.Id).OrderBy(v => v));
            Assert.Empty(edited.Strategy.Diff());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task DatabaseUnitEqualityDuringDisjointPull(bool binary)
        {
            await this.Login("administrator");
            var remote = this.Workspace.CreateSession();
            var editing = this.Workspace.CreateSession();
            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var source = (await remote.PullAsync(pull)).GetCollection<C1>()[0];
            IRoleType roleType = binary ? this.M.C1.C1AllorsBinary : this.M.C1.C1AllorsDateTime;
            object baseline = binary ? new byte[] { 1, 2 } : new System.DateTime(2020, 1, 2, 3, 4, 5, System.DateTimeKind.Utc);
            object local = binary ? new byte[] { 3, 4 } : new System.DateTime(2021, 1, 2, 3, 4, 5, System.DateTimeKind.Utc);
            source.Strategy.SetUnitRole(roleType, baseline);
            source.C1AllorsInteger = 1;
            Assert.False((await remote.PushAsync()).HasErrors);
            Assert.False((await remote.PullAsync(pull)).HasErrors);
            var edited = (await editing.PullAsync(pull)).GetCollection<C1>()[0];
            var version = edited.Strategy.Version;
            edited.Strategy.SetUnitRole(roleType, local);
            source.C1AllorsInteger = 2;
            Assert.False((await remote.PushAsync()).HasErrors);
            Assert.False((await remote.PullAsync(pull)).HasErrors);
            Assert.Equal(baseline, source.Strategy.GetUnitRole(roleType));

            var result = await editing.PullAsync(pull);
            // DateTime has value equality; byte arrays use reference equality in CanMerge.
            Assert.Equal(binary, result.HasErrors);
            if (binary)
            {
                Assert.Same(edited.Strategy, Assert.Single(result.MergeErrors).Strategy);
            }
            else
            {
                Assert.Empty(result.MergeErrors);
            }

            Assert.Equal(binary ? version : source.Strategy.Version, edited.Strategy.Version);
            Assert.Equal(binary ? 1 : 2, edited.C1AllorsInteger);
            Assert.Equal(local, edited.Strategy.GetUnitRole(roleType));
            var diff = Assert.IsAssignableFrom<IUnitDiff>(Assert.Single(edited.Strategy.Diff()));
            Assert.Equal(roleType.RelationType, diff.RelationType);
            Assert.Equal(baseline, diff.OriginalRole);
            Assert.Equal(local, diff.ChangedRole);
            edited.Strategy.Reset();
            Assert.Equal(baseline, edited.Strategy.GetUnitRole(roleType));
            Assert.Empty(edited.Strategy.Diff());
            Assert.False((await editing.PullAsync(pull)).HasErrors);
            Assert.Equal(source.Strategy.Version, edited.Strategy.Version);
            Assert.Equal(2, edited.C1AllorsInteger);
            Assert.Equal(baseline, edited.Strategy.GetUnitRole(roleType));
        }

        private void AssertEditingDiff(C1 edited, C1 baselineTarget, C1 localTarget)
        {
            Assert.True(edited.Strategy.HasChanges);
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
        }
    }
}

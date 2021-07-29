// <copyright file="ChangeSetTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Tests.Workspace
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class DiffTests : Test
    {
        protected DiffTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async void DiffTest()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            await session.Push();

            result = await session.Pull(pull);
            var c1b = result.GetCollection<C1>()[0];

            c1b.C1AllorsString = "Y";

            var diff = c1b.Strategy.Diff();
            Assert.Single(diff);

            var unitdiff = (IUnitDiff)diff[0];
            Assert.Equal("X", unitdiff.OriginalRole);
            Assert.Equal("Y", unitdiff.ChangedRole);
            Assert.Equal(this.M.C1.C1AllorsString.RelationType, unitdiff.RelationType);
        }

        [Fact]
        public async void DiffBeforeAndAfterResetTest()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            await session.Push();

            result = await session.Pull(pull);
            var c1b = result.GetCollection<C1>()[0];

            c1b.C1AllorsString = "Y";

            var diff = c1b.Strategy.Diff();

            Assert.Single(diff);

            c1b.Strategy.Reset();
            diff = c1b.Strategy.Diff();

            Assert.Empty(diff);
        }

        [Fact]
        public async void DiffAfterDoubleReset()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            await session.Push();

            result = await session.Pull(pull);
            var c1b = result.GetCollection<C1>()[0];

            c1b.C1AllorsString = "Y";

            c1b.Strategy.Reset();
            c1b.Strategy.Reset();

            var diff = c1b.Strategy.Diff();

            Assert.Empty(diff);

        }

        [Fact]
        public async void DiffTestIntAndString()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";
            c1a.C1AllorsInteger = 1;

            await session.Push();

            result = await session.Pull(pull);
            var c1b = result.GetCollection<C1>()[0];

            c1b.C1AllorsString = "Y";
            c1b.C1AllorsInteger = 2;

            var diff = c1b.Strategy.Diff();

            Assert.Equal(2, diff.Count);
            var unitDiffs = new List<IUnitDiff>();

            foreach (var diffToAdd in diff)
            {
                unitDiffs.Add((IUnitDiff)diffToAdd);
            }

            var unitdiffString = unitDiffs.First(v => v.RelationType == this.M.C1.C1AllorsString.RelationType);
            var unitdiffInt = unitDiffs.First(v => v.RelationType == this.M.C1.C1AllorsInteger.RelationType);

            Assert.Equal("X", unitdiffString.OriginalRole);
            Assert.Equal("Y", unitdiffString.ChangedRole);
            Assert.Equal(this.M.C1.C1AllorsString.RelationType, unitdiffString.RelationType);

            Assert.Equal(1, unitdiffInt.OriginalRole);
            Assert.Equal(2, unitdiffInt.ChangedRole);
            Assert.Equal(this.M.C1.C1AllorsInteger.RelationType, unitdiffInt.RelationType);
        }
    }
}

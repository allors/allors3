// <copyright file="MethodTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class MethodTests : Test
    {
        protected MethodTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void Call()
        {
            var session = this.Workspace.CreateSession();

            var pull = new[] { new Pull { Extent = new Extent(this.M.Organisation.ObjectType), }, };

            var organisation = (await session.Load(pull)).GetCollection<Organisation>().First();

            Assert.False(organisation.JustDidIt);

            var result = await session.Call(organisation.JustDoIt);

            Assert.False(result.HasErrors);

            pull = new[] { new Pull { Object = organisation, }, };

            organisation = (await session.Load(pull)).GetObject<Organisation>();

            session.Reset();

            Assert.True(organisation.JustDidIt);
        }
    }
}

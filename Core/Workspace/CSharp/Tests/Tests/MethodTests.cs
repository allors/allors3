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
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new[] { new Pull { Extent = new Extent(this.M.Organisation), }, };

            var organisation = (await session.Pull(pull)).GetCollection<Organisation>().First();

            Assert.False(organisation.JustDidIt);

            var invokeResult = await session.Invoke(organisation.JustDoIt);

            Assert.False(invokeResult.HasErrors);

            pull = new[] { new Pull { Object = organisation }, };

            var pullResult = await session.Pull(pull);
            organisation = pullResult.GetObject<Organisation>();

            Assert.True(organisation.JustDidIt);
        }
    }
}

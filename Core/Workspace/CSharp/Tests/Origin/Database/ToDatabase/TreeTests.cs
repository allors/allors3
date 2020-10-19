// <copyright file="PullTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Origin.Database.ToDatabase
{
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Allors.Workspace.Meta;
    using Xunit;
    using Result = Allors.Workspace.Data.Result;

    public class TreeTests : Test
    {
        [Fact]
        public async void Users()
        {
            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Extent(this.M.Organisation.Class),
                Results = new[]
                {
                    new Result
                    {
                        Fetch = new Fetch
                        {
                            Include = new OrganisationNodeBuilder(this.M,v => v.Organisation_Owner()),
                        },
                    },
                },
            };

            var result = await session.Load(pull);

            var organisations = result.GetCollection<Organisation>();

            var owner = organisations.Single(v => v.ExistOwner).Owner;

            Assert.NotNull(owner);
            Assert.Equal("Jane", owner.FirstName);
        }
    }
}

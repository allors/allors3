// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Threading.Tasks;
    using Allors;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class SecurityTests : Test
    {
        protected SecurityTests(Fixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");
        }

        [Fact]
        public async void WithGrant()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
            };

            var result = await session.PullAsync(pull);

            var c1s = result.GetCollection<C1>("C1s");
            foreach (var c1 in result.GetCollection<C1>())
            {
                foreach (var roleType in this.M.C1.DatabaseOriginRoleTypes)
                {
                    Assert.True(c1.Strategy.CanRead(roleType));
                    Assert.True(c1.Strategy.CanWrite(roleType));
                }
            }
        }

        [Fact]
        public async void WithoutAccessControl()
        {
            await this.Login("noacl");

            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
            };

            var result = await session.PullAsync(pull);

            foreach (var c1 in result.GetCollection<C1>())
            {
                foreach (var roleType in this.M.C1.DatabaseOriginRoleTypes)
                {
                    if (roleType.Origin == Origin.Database)
                    {
                        Assert.False(c1.Strategy.CanRead(roleType));
                        Assert.False(c1.Strategy.CanWrite(roleType));
                    }
                    else
                    {
                        Assert.True(c1.Strategy.CanRead(roleType));
                        Assert.True(c1.Strategy.CanWrite(roleType));
                    }
                }
            }
        }

        [Fact]
        public async void WithoutPermissions()
        {
            await this.Login("noperm");

            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
            };

            var result = await session.PullAsync(pull);

            foreach (var c1 in result.GetCollection<C1>())
            {
                foreach (var roleType in this.M.C1.DatabaseOriginRoleTypes)
                {
                    if (roleType.Origin == Origin.Database)
                    {
                        Assert.False(c1.Strategy.CanRead(roleType));
                        Assert.False(c1.Strategy.CanWrite(roleType));
                    }
                    else
                    {
                        Assert.True(c1.Strategy.CanRead(roleType));
                        Assert.True(c1.Strategy.CanWrite(roleType));
                    }
                }
            }
        }

        [Fact]
        public async void DeniedPermissions()
        {
            var session = this.Workspace.CreateSession();

            var result = await session.PullAsync(new Pull { Extent = new Filter(this.M.Denied) });

            foreach (var denied in result.GetCollection<Denied>())
            {
                foreach (var roleType in this.M.C1.DatabaseOriginRoleTypes)
                {
                    if (roleType.Origin == Origin.Database)
                    {
                        Assert.False(denied.Strategy.CanRead(roleType));
                        Assert.False(denied.Strategy.CanWrite(roleType));
                    }
                    else
                    {
                        Assert.True(denied.Strategy.CanRead(roleType));
                        Assert.True(denied.Strategy.CanWrite(roleType));
                    }
                }
            }
        }
    }
}

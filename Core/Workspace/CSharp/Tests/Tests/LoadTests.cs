// <copyright file="LoadTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class LoadTests : Test
    {
        protected LoadTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void WithAccessControl()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Extent(this.M.C1.ObjectType),
            };

            var result = session.Pull(pull).Result;

            var c1s = result.GetCollection<C1>("C1s");
            Assert.Equal(4, c1s.Length);

            result = session.Pull(pull).Result;

            var c1s2 = result.GetCollection<C1>("C1s");
            Assert.Equal(4, c1s2.Length);
        }

        [Fact]
        public async void WithoutAccessControl()
        {
            await this.Login("noacl");

            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Extent(this.M.C1.ObjectType),
            };

            var result = await session.Pull(pull);

            var c1s = result.GetCollection<C1>("C1s");

            foreach (var c1 in c1s)
            {
                foreach (var roleType in this.M.C1.ObjectType.DatabaseRoleTypes)
                {
                    Assert.False(c1.Strategy.Exist(roleType));
                }

                foreach (var associationType in this.M.C1.ObjectType.AssociationTypes)
                {
                    if (associationType.IsOne)
                    {
                        var association = c1.Strategy.GetComposite<IObject>(associationType);
                        Assert.Null(association);
                    }
                    else
                    {
                        var association = c1.Strategy.GetComposites<IObject>(associationType);
                        Assert.Empty(association);
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
                Extent = new Extent(this.M.C1.ObjectType),
            };

            var result = session.Pull(pull).Result;

            var c1s = result.GetCollection<C1>("C1s");

            foreach (var c1 in c1s)
            {
                foreach (var roleType in this.M.C1.ObjectType.DatabaseRoleTypes)
                {
                    Assert.False(c1.Strategy.Exist(roleType));
                }

                foreach (var associationType in this.M.C1.ObjectType.AssociationTypes)
                {
                    if (associationType.IsOne)
                    {
                        var association = c1.Strategy.GetComposite<IObject>(associationType);
                        Assert.Null(association);
                    }
                    else
                    {
                        var association = c1.Strategy.GetComposites<IObject>(associationType);
                        Assert.Empty(association);
                    }
                }
            }
        }
    }
}

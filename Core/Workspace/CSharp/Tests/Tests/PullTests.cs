// <copyright file="PullTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Tests.Workspace
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;
    using static Names;

    public abstract class PullTests : Test
    {
        protected PullTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async void AndGreaterThanLessThan()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            // Class
            var pull = new Pull
            {
                Extent = new Extent(this.M.C1)
                {
                    Predicate = new And
                    {
                        Operands = new IPredicate[]
                        {
                            new GreaterThan(m.C1.C1AllorsInteger){Value = 0},
                            new LessThan(m.C1.C1AllorsInteger){Value = 2}
                        }
                    }
                }
            };

            var result = await session.Pull(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);

            // Interface
            pull = new Pull
            {
                Extent = new Extent(this.M.I12)
                {
                    Predicate = new And
                    {
                        Operands = new IPredicate[]
                        {
                            new GreaterThan(m.I12.I12AllorsInteger){Value = 0},
                            new LessThan(m.I12.I12AllorsInteger){Value = 2}
                        }
                    }
                }
            };

            result = await session.Pull(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I12>().Equal(c1B, c2B);
        }

        [Fact]
        public async void AssociationMany2ManyContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            // Empty
            var pull = new Pull
            {
                Extent = new Extent(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1sWhereC1C2Many2Many)
                    {
                        Extent = new Extent(this.M.C1)
                        {
                            Predicate = new Equals(m.C1.C1AllorsString) { Value = "Nothing here!" }
                        }
                    }
                }
            };

            var result = await session.Pull(pull);

            Assert.Empty(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            // Full
            pull = new Pull
            {
                Extent = new Extent(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1sWhereC1C2Many2Many)
                    {
                        Extent = new Extent(this.M.C1)
                    }
                }
            };

            result = await session.Pull(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B, c2C, c2D);

            // Filtered
            pull = new Pull
            {
                Extent = new Extent(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1sWhereC1C2Many2Many)
                    {
                        Extent = new Extent(this.M.C1)
                        {
                            Predicate = new Equals(m.C1.C1AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            result = await session.Pull(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B);
        }

        [Fact]
        public async void AssociationMany2ManyContains()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c1c = await session.PullObject<C1>(c1C);

            // Full
            var pull = new Pull
            {
                Extent = new Extent(this.M.C2)
                {
                    Predicate = new Contains(m.C2.C1sWhereC1C2Many2Many)
                    {
                        Object = c1c
                    }
                }
            };

            var result = await session.Pull(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B, c2C);
        }
    }
}

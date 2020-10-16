// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Origin.Database.ToDatabase
{
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;
    using Result = Allors.Workspace.Data.Result;

    public class AssociationTests : Test
    {
        [Fact]
        public async void GetOne2Many()
        {
            var session = this.Workspace.CreateSession();

            var pull = new[]
            {
                        new Pull
                        {
                            Extent = new Extent(this.M.C2.ObjectType)
                            {
                                Predicate = new Equals(this.M.C2.Name) {Value = "c2C"},
                            },
                            Results = new[]
                            {
                                new Result
                                {
                                    Fetch = new Fetch
                                    {
                                        Include = new[] {new Node(this.M.C2.C1WhereC1C2One2Many),},
                                    },
                                },
                            },
                        },
                    };

            var result = await session.Load(pull);

            var c2s = result.GetCollection<C2>();

            var c2C = c2s.First(v => v.Name == "c2C");

            var c1WhereC1C2One2Many = c2C.C1WhereC1C2One2Many;

            // One to One
            Assert.NotNull(c1WhereC1C2One2Many);
            Assert.Equal("c1C", c1WhereC1C2One2Many.Name);
        }

        [Fact]
        public async void GetOne2One()
        {
            var session = this.Workspace.CreateSession();

            var pull = new[]
            {
                        new Pull
                        {
                            Extent = new Extent(this.M.C2.ObjectType)
                            {
                                Predicate = new Equals(this.M.C2.Name) {Value = "c2C"},
                            },
                            Results = new[]
                            {
                                new Result
                                {
                                    Fetch = new Fetch
                                    {
                                        Include = new[] {new Node(this.M.C2.C1WhereC1C2One2One),},
                                    },
                                },
                            },
                        },
                    };

            var result = await session.Load(pull);

            var c2s = result.GetCollection<C2>();

            var c2C = c2s.First(v => v.Name == "c2C");

            var c1WhereC1C2One2One = c2C.C1WhereC1C2One2One;

            // One to One
            Assert.NotNull(c1WhereC1C2One2One);
            Assert.Equal("c1C", c1WhereC1C2One2One.Name);
        }
    }
}

// <copyright file="PullTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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
    using Allors;
    using Allors.Workspace.Data;
    using Allors.Workspace.Meta;
    using Xunit;
    using static Names;
    using C1 = Allors.Workspace.Domain.C1;
    using C2 = Allors.Workspace.Domain.C2;
    using I12 = Allors.Workspace.Domain.I12;
    using I2 = Allors.Workspace.Domain.I2;

    public abstract class PullTests : Test
    {
        protected PullTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async Task AndGreaterThanLessThan()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            // Class
            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
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

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);

            // Interface
            pull = new Pull
            {
                Extent = new Filter(this.M.I12)
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

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I12>().Equal(c1B, c2B);
        }

        [Fact]
        public async Task AssociationMany2ManyContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            // Empty
            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1sWhereC1C2Many2Many)
                    {
                        Extent = new Filter(this.M.C1)
                        {
                            Predicate = new Equals(m.C1.C1AllorsString) { Value = "Nothing here!" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Empty(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            // Full
            pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1sWhereC1C2Many2Many)
                    {
                        Extent = new Filter(this.M.C1)
                    }
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B, c2C, c2D);

            // Filtered
            pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1sWhereC1C2Many2Many)
                    {
                        Extent = new Filter(this.M.C1)
                        {
                            Predicate = new Equals(m.C1.C1AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B);
        }

        [Fact]
        public async Task AssociationMany2ManyContains()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c1c = await session.PullObject<C1>(c1C);

            // Full
            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Contains(m.C2.C1sWhereC1C2Many2Many)
                    {
                        Object = c1c
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B, c2C);
        }

        [Fact]
        public async Task AssociationMany2ManyExist()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            // Full
            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Exists(m.C2.C1sWhereC1C2Many2Many)
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B, c2C, c2D);
        }

        [Fact]
        public async Task AssociationMany2OneContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1sWhereC1C2Many2One)
                    {
                        Extent = new Filter(this.M.C1)
                        {
                            Predicate = new Equals(m.C1.C1AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B);
        }

        [Fact]
        public async Task AssociationMany2OneContains()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c1c = await session.PullObject<C1>(c1C);

            // Full
            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Contains(m.C2.C1sWhereC1C2Many2One)
                    {
                        Object = c1c
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2C);
        }

        [Fact]
        public async Task AssociationOne2ManyContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1WhereC1C2One2Many)
                    {
                        Extent = new Filter(this.M.C1)
                        {
                            Predicate = new Equals(m.C1.C1AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B);
        }

        [Fact]
        public async Task AssociationOne2ManyEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c1b = await session.PullObject<C1>(c1B);
            var c1c = await session.PullObject<C1>(c1C);

            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Equals(m.C2.C1WhereC1C2One2Many)
                    {
                        Object = c1b
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B);

            pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Equals(m.C2.C1WhereC1C2One2Many)
                    {
                        Object = c1c
                    }
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2C, c2D);
        }

        [Fact]
        public async Task AssociationOne2ManyExists()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            // Class
            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Exists(m.C2.C1WhereC1C2One2Many)
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B, c2C, c2D);

            // Interface
            pull = new Pull
            {
                Extent = new Filter(this.M.I2)
                {
                    Predicate = new Exists(m.I2.I1WhereI1I2One2Many)
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I2>().Equal(c2B, c2C, c2D);
        }

        [Fact]
        public async Task AssociationOne2ManyInstanceOf()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Instanceof(m.C2.C1WhereC1C2One2Many) { ObjectType = m.C1 }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B, c2C, c2D);
        }

        [Fact]
        public async Task AssociationOne2OneContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1WhereC1C2One2One)
                    {
                        Extent = new Filter(this.M.C1)
                        {
                            Predicate = new Equals(m.C1.C1AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B);
        }

        [Fact]
        public async Task AssociationOne2OneEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c1b = await session.PullObject<C1>(c1B);
            var c1c = await session.PullObject<C1>(c1C);

            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Equals(m.C2.C1WhereC1C2One2One)
                    {
                        Object = c1b
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B);

            pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Equals(m.C2.C1WhereC1C2One2One)
                    {
                        Object = c1c
                    }
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2C);
        }

        [Fact]
        public async Task AssociationOne2OneExists()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;


            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new Exists(m.C1.C1WhereC1C1One2One)
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C, c1D);

            pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Exists(m.C2.C1WhereC1C2One2One)
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B, c2C, c2D);
        }

        [Fact]
        public async Task AssociationOne2OneInstanceOf()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.I12)
                {
                    Predicate = new Instanceof(m.I12.I12WhereI12I12One2One) { ObjectType = m.C1 }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I12>().Equal(c1D, c2B, c2C);

            pull = new Pull
            {
                Extent = new Filter(this.M.I12)
                {
                    Predicate = new Instanceof(m.I12.I12WhereI12I12One2One) { ObjectType = m.I2 }
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I12>().Equal(c1B, c1C, c2D);
        }

        [Fact]
        public async Task ObjectEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c1c = await session.PullObject<C1>(c1C);

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals { Object = c1c }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C);
        }

        [Fact]
        public async Task ExtentInterface()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.I12)
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I12>().Equal(c1A, c1B, c1C, c1D, c2A, c2B, c2C, c2D);
        }

        [Fact]
        public async Task InstanceOf()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.I12)
                {
                    Predicate = new Instanceof
                    {
                        ObjectType = m.C1
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I12>().Equal(c1A, c1B, c1C, c1D);
        }

        [Fact]
        public async Task NotEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c1c = await session.PullObject<C1>(c1C);

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Not
                    {
                        Operand = new Equals { Object = c1c }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1A, c1B, c1D);
        }

        [Fact]
        public async Task OrEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c1b = await session.PullObject<C1>(c1B);
            var c1c = await session.PullObject<C1>(c1C);

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Or
                    {
                        Operands = new[]
                        {
                            new Equals { Object = c1b },
                            new Equals { Object = c1c }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C);
        }

        [Fact]
        public async Task OperatorExcept()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Except
                {
                    Operands = new Extent[]
                    {
                        new Filter(m.I12),
                        new Filter(m.I12)
                        {
                            Predicate = new Instanceof{ObjectType = m.C2}
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I12>().Equal(c1A, c1B, c1C, c1D);
        }

        [Fact]
        public async Task OperatorIntersect()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Intersect()
                {
                    Operands = new Extent[]
                    {
                        new Filter(m.I12),
                        new Filter(m.I12)
                        {
                            Predicate = new Instanceof{ObjectType = m.C2}
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I12>().Equal(c2A, c2B, c2C, c2D);
        }

        [Fact]
        public async Task OperatorUnion()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Union
                {
                    Operands = new Extent[]
                    {
                        new Filter(m.C1){Predicate = new Equals(m.C1.Name) {Value = "c1A"}},
                        new Filter(m.C1){Predicate = new Equals(m.C1.Name) {Value = "c1B"}}
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1A, c1B);
        }

        [Fact]
        public async Task RoleDateTimeBetweenPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsDateTime)
                    {
                        Paths = new IRoleType[] { m.C1.C1DateTimeBetweenA, m.C1.C1DateTimeBetweenB }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async Task RoleDateTimeBetweenValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsDateTime)
                    {
                        Values = new object[]
                        {
                            new System.DateTime(2000, 1, 1, 0, 0, 4, DateTimeKind.Utc),
                            new System.DateTime(2000, 1, 1, 0, 0, 6, DateTimeKind.Utc)
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C, c1D);
        }

        [Fact]
        public async Task RoleDateTimeGreaterThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsDateTime)
                    {
                        Path = m.C1.C1DateTimeGreaterThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async Task RoleDateTimeGreaterThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsDateTime)
                    {
                        Value = new System.DateTime(2000, 1, 1, 0, 0, 4, DateTimeKind.Utc)
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async Task RoleDateTimeLessThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsDateTime)
                    {
                        Path = m.C1.C1DateTimeLessThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1D);
        }

        [Fact]
        public async Task RoleDateTimeLessThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsDateTime)
                    {
                        Value = new System.DateTime(2000, 1, 1, 0, 0, 5, DateTimeKind.Utc)
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async Task RoleDateTimeEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsDateTime)
                    {
                        Value = new System.DateTime(2000, 1, 1, 0, 0, 4, DateTimeKind.Utc)
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async Task RoleDecimalBetweenPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsDecimal)
                    {
                        Paths = new IRoleType[] { m.C1.C1DecimalBetweenA, m.C1.C1DecimalBetweenB }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1D);
        }

        [Fact]
        public async Task RoleDecimalBetweenValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsDecimal)
                    {
                        Values = new object[] { 2.1m, 2.3m }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async Task RoleDecimalGreaterThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsDecimal)
                    {
                        Path = m.C1.C1DecimalGreaterThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C);
        }

        [Fact]
        public async Task RoleDecimalGreaterThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsDecimal)
                    {
                        Value = 1.5m
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async Task RoleDecimalLessThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsDecimal)
                    {
                        Path = m.C1.C1DecimalLessThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1D);
        }

        [Fact]
        public async Task RoleDecimalLessThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsDecimal)
                    {
                        Value = 1.9m
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async Task RoleDecimalEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsDecimal)
                    {
                        Value = 2.2m
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async Task RoleDoubleBetweenPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsDouble)
                    {
                        Paths = new IRoleType[] { m.C1.C1DoubleBetweenA, m.C1.C1DoubleBetweenB }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1D);
        }

        [Fact]
        public async Task RoleDoubleBetweenValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsDouble)
                    {
                        Values = new object[] { 2.1d, 2.3d }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async Task RoleDoubleGreaterThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsDouble)
                    {
                        Path = m.C1.C1DoubleGreaterThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C);
        }

        [Fact]
        public async Task RoleDoubleGreaterThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsDouble)
                    {
                        Value = 1.5d
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async Task RoleDoubleLessThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsDouble)
                    {
                        Path = m.C1.C1DoubleLessThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1D);
        }

        [Fact]
        public async Task RoleDoubleLessThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsDouble)
                    {
                        Value = 1.9d
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async Task RoleDoubleEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsDouble)
                    {
                        Value = 2.2d
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async Task RoleIntegerBetweenPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsInteger)
                    {
                        Paths = new IRoleType[] { m.C1.C1IntegerBetweenA, m.C1.C1IntegerBetweenB }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async Task RoleIntegerBetweenValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsInteger)
                    {
                        Values = new object[] { 1, 2 }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C, c1D);
        }

        [Fact]
        public async Task RoleIntegerGreaterThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsInteger)
                    {
                        Path = m.C1.C1IntegerGreaterThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async Task RoleIntegerGreaterThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsInteger)
                    {
                        Value = 1
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async Task RoleIntegerLessThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsInteger)
                    {
                        Path = m.C1.C1IntegerLessThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1D);
        }

        [Fact]
        public async Task RoleIntegerLessThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsInteger)
                    {
                        Value = 2
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async Task RoleIntegerEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsInteger)
                    {
                        Value = 2
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async Task RoleIntegerExist()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Exists(m.C1.C1AllorsInteger)
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C, c1D);
        }

        [Fact]
        public async Task RoleStringEqualsPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsString)
                    {
                        Path = m.C1.C1AllorsStringEquals
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C);
        }

        [Fact]
        public async Task RoleStringEqualsValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsString)
                    {
                        Value = "ᴀbra"
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async Task RoleStringLike()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Like(m.C1.C1AllorsString)
                    {
                        Value = "ᴀ%"
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C, c1D);
        }

        [Fact]
        public async Task RoleUniqueEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsUnique)
                    {
                        Value = new Guid("8B3C4978-72D3-40BA-B302-114EB331FE04")
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async Task RoleMany2ManyContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            // Empty
            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new ContainedIn(m.C1.C1I12Many2Manies)
                    {
                        Extent = new Filter(this.M.I12)
                        {
                            Predicate = new Equals(m.I12.I12AllorsString) { Value = "Nothing here!" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Empty(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            // Full
            pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new ContainedIn(m.C1.C1I12Many2Manies)
                    {
                        Extent = new Filter(this.M.I12)
                    }
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C, c1D);

            // Filtered
            pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new ContainedIn(m.C1.C1I12Many2Manies)
                    {
                        Extent = new Filter(this.M.I12)
                        {
                            Predicate = new Equals(m.I12.I12AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C, c1D);
        }

        [Fact]
        public async Task RoleMany2ManyContains()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c2c = await session.PullObject<C2>(c2C);

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new Contains(m.C1.C1C2Many2Manies)
                    {
                        Object = c2c
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async Task RoleOne2ManyContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new ContainedIn(m.C1.C1I12One2Manies)
                    {
                        Extent = new Filter(this.M.I12)
                        {
                            Predicate = new Equals(m.I12.I12AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async Task RoleOne2ManyContains()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c2d = await session.PullObject<C2>(c2D);

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new Contains(m.C1.C1C2One2Manies)
                    {
                        Object = c2d
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C);
        }

        [Fact]
        public async Task RoleMany2OneContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new ContainedIn(m.C1.C1I12Many2One)
                    {
                        Extent = new Filter(this.M.I12)
                        {
                            Predicate = new Equals(m.I12.I12AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async Task RoleOne2OneContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new ContainedIn(m.C1.C1I12One2One)
                    {
                        Extent = new Filter(this.M.I12)
                        {
                            Predicate = new Equals(m.I12.I12AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C);
        }

        [Fact]
        public async Task WithResultName()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsInteger)
                    {
                        Value = 2
                    }
                },
                Results = new[]{
                    new Result
                    {
                        Name = "IetsAnders",
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>("IetsAnders").Equal(c1C, c1D);
        }

        [Fact]
        public async Task PullWithObjectId()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var pull1 = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull1);
            var c1a = result.GetCollection<C1>()[0];

            var pull2 = new Pull
            {
                ObjectId = c1a.Id
            };

            result = await session.PullAsync(pull2);

            Assert.Single(result.Objects);
            Assert.Empty(result.Collections);
            Assert.Empty(result.Values);
        }

        [Fact]
        public async Task PullWithInclude()
        {
            await this.Login("administrator");
            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1),
                Results = new[]
                {
                    new Result
                    {
                        Select = new Select
                        {
                            Include = this.M.C1.Nodes(v=>v.C1C2One2One.Node())
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            var c1s = result.GetCollection<C1>();
            var c1b = c1s.Single(v => v.Name == "c1B");
            var c1c = c1s.Single(v => v.Name == "c1C");
            var c1d = c1s.Single(v => v.Name == "c1D");

            var c2ByC1 = c1s.ToDictionary(v => v, v => v.C1C2One2One);

            Assert.Equal("c2B", c2ByC1[c1b].Name);
            Assert.Equal("c2C", c2ByC1[c1c].Name);
            Assert.Equal("c2D", c2ByC1[c1d].Name);
        }

        [Fact]
        public async Task SortDirectionDefault()
        {
            await this.Login("administrator");
            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.I12) { Sorting = new[] { new Sort(this.M.I12.Order) } },
            };

            var result = await session.PullAsync(pull);

            var i12s = result.GetCollection<I12>();

            Assert.Equal("c2D", i12s[0].Name);
            Assert.Equal("c2C", i12s[1].Name);
            Assert.Equal("c1B", i12s[2].Name);
            Assert.Equal("c1A", i12s[3].Name);
            Assert.Equal("c2A", i12s[4].Name);
            Assert.Equal("c2B", i12s[5].Name);
            Assert.Equal("c1D", i12s[6].Name);
            Assert.Equal("c1C", i12s[7].Name);
        }

        [Fact]
        public async Task SortDirectionAscending()
        {
            await this.Login("administrator");
            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.I12) { Sorting = new[] { new Sort(this.M.I12.Order) { SortDirection = SortDirection.Ascending } } },
            };

            var result = await session.PullAsync(pull);

            var i12s = result.GetCollection<I12>();

            Assert.Equal("c2D", i12s[0].Name);
            Assert.Equal("c2C", i12s[1].Name);
            Assert.Equal("c1B", i12s[2].Name);
            Assert.Equal("c1A", i12s[3].Name);
            Assert.Equal("c2A", i12s[4].Name);
            Assert.Equal("c2B", i12s[5].Name);
            Assert.Equal("c1D", i12s[6].Name);
            Assert.Equal("c1C", i12s[7].Name);
        }


        [Fact]
        public async Task SortDirectionDescending()
        {
            await this.Login("administrator");
            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.I12) { Sorting = new[] { new Sort(this.M.I12.Order) { SortDirection = SortDirection.Descending } } },
            };

            var result = await session.PullAsync(pull);

            var i12s = result.GetCollection<I12>();

            Assert.Equal("c2D", i12s[7].Name);
            Assert.Equal("c2C", i12s[6].Name);
            Assert.Equal("c1B", i12s[5].Name);
            Assert.Equal("c1A", i12s[4].Name);
            Assert.Equal("c2A", i12s[3].Name);
            Assert.Equal("c2B", i12s[2].Name);
            Assert.Equal("c1D", i12s[1].Name);
            Assert.Equal("c1C", i12s[0].Name);
        }
    }
}

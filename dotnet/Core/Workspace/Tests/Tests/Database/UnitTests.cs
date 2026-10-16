// <copyright file="Many2OneTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.DatabaseAssociation.DatabaseRelation
{
    using System;
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Xunit;
    using Allors.Workspace.Data;

    public abstract class UnitTests : Test
    {
        private Func<Context>[] contextFactories;

        protected UnitTests(Fixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");

            var singleSessionContext = new SingleSessionContext(this, "Single shared");
            var multipleSessionContext = new MultipleSessionContext(this, "Multiple shared");

            this.contextFactories = new Func<Context>[]
            {
                () => singleSessionContext,
                //() => new SingleSessionContext(this, "Single"),
                //() => multipleSessionContext,
                () => new MultipleSessionContext(this, "Multiple"),
            };
        }

        [Fact]
        public async Task PushAndPullUnitValues()
        {
            var writer = this.Workspace.CreateSession();
            var reader = (await this.Profile.CreateExclusiveWorkspaceAsync()).CreateSession();
            var source = writer.Create<C1>();
            SetUnitValues(source, false);
            Assert.False((await writer.PushAsync()).HasErrors);
            Assert.True(source.Id > 0);
            Assert.False((await writer.PullAsync(new Pull { ObjectId = source.Id })).HasErrors);

            var query = new Pull { ObjectId = source.Id, Results = new[] { new Result { Name = "unitSample" } } };
            var first = await reader.PullAsync(query);
            Assert.False(first.HasErrors);
            var unitSample = first.GetObject<C1>("unitSample");
            Assert.NotNull(unitSample);
            AssertUnitValues(unitSample, false);

            SetUnitValues(source, true);
            Assert.False((await writer.PushAsync()).HasErrors);
            var refreshed = await reader.PullAsync(query);
            Assert.False(refreshed.HasErrors);
            Assert.Same(unitSample, refreshed.GetObject<C1>("unitSample"));
            AssertUnitValues(unitSample, true);
        }

        [Fact]
        public async Task PushAndPullClearedUnitValues()
        {
            var writer = this.Workspace.CreateSession();
            var reader = (await this.Profile.CreateExclusiveWorkspaceAsync()).CreateSession();
            var source = writer.Create<C1>();
            SetUnitValues(source, true);
            Assert.False((await writer.PushAsync()).HasErrors);
            Assert.True(source.Id > 0);
            Assert.False((await writer.PullAsync(new Pull { ObjectId = source.Id })).HasErrors);

            var query = new Pull { ObjectId = source.Id, Results = new[] { new Result { Name = "unitSample" } } };
            var first = await reader.PullAsync(query);
            Assert.False(first.HasErrors);
            var unitSample = first.GetObject<C1>("unitSample");
            Assert.NotNull(unitSample);
            AssertUnitValues(unitSample, true);

            SetUnitValues(source, false);
            Assert.False((await writer.PushAsync()).HasErrors);
            var refreshed = await reader.PullAsync(query);
            Assert.False(refreshed.HasErrors);
            Assert.Same(unitSample, refreshed.GetObject<C1>("unitSample"));
            AssertUnitValues(unitSample, false);
        }

        private static void SetUnitValues(C1 item, bool present)
        {
            item.C1AllorsBinary = present ? new byte[] { 1, 2, 3 } : null;
            item.C1AllorsBoolean = present ? true : null;
            item.C1AllorsDateTime = present ? new DateTime(1973, 3, 27, 0, 0, 0, DateTimeKind.Utc) : null;
            item.C1AllorsDecimal = present ? 12.34m : null;
            item.C1AllorsDouble = present ? 123d : null;
            item.C1AllorsInteger = present ? 1000 : null;
            item.C1AllorsString = present ? "a string" : null;
            item.C1AllorsUnique = present ? new Guid("2946CF37-71BE-4681-8FE6-D0024D59BEFF") : null;
        }

        private static void AssertUnitValues(C1 item, bool present)
        {
            Assert.Equal(present, item.ExistC1AllorsBinary);
            Assert.Equal(present, item.ExistC1AllorsBoolean);
            Assert.Equal(present, item.ExistC1AllorsDateTime);
            Assert.Equal(present, item.ExistC1AllorsDecimal);
            Assert.Equal(present, item.ExistC1AllorsDouble);
            Assert.Equal(present, item.ExistC1AllorsInteger);
            Assert.Equal(present, item.ExistC1AllorsString);
            Assert.Equal(present, item.ExistC1AllorsUnique);
            if (present)
            {
                Assert.Equal(new byte[] { 1, 2, 3 }, item.C1AllorsBinary);
                Assert.Equal(true, item.C1AllorsBoolean);
                Assert.Equal(new DateTime(1973, 3, 27, 0, 0, 0, DateTimeKind.Utc), item.C1AllorsDateTime);
                Assert.Equal(12.34m, item.C1AllorsDecimal);
                Assert.Equal(123d, item.C1AllorsDouble);
                Assert.Equal(1000, item.C1AllorsInteger);
                Assert.Equal("a string", item.C1AllorsString);
                Assert.Equal(new Guid("2946CF37-71BE-4681-8FE6-D0024D59BEFF"), item.C1AllorsUnique);
            }
            else
            {
                Assert.Null(item.C1AllorsBinary);
                Assert.Null(item.C1AllorsBoolean);
                Assert.Null(item.C1AllorsDateTime);
                Assert.Null(item.C1AllorsDecimal);
                Assert.Null(item.C1AllorsDouble);
                Assert.Null(item.C1AllorsInteger);
                Assert.Null(item.C1AllorsString);
                Assert.Null(item.C1AllorsUnique);
            }
        }

        [Fact]
        public async void SetRole()
        {
            foreach (DatabaseMode mode in Enum.GetValues(typeof(DatabaseMode)))
            {
                foreach (var contextFactory in this.contextFactories)
                {
                    var ctx = contextFactory();
                    var (session1, _) = ctx;

                    var c1 = await ctx.Create<C1>(session1, mode);

                    Assert.NotNull(c1);

                    if (!c1.CanWriteC1C1One2One)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    c1.C1AllorsBinary = new byte[] { 1, 2 };
                    c1.C1AllorsBoolean = true;
                    c1.C1AllorsDateTime = new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc);
                    c1.C1AllorsDecimal = 10.10m;
                    c1.C1AllorsDouble = 11.11d;
                    c1.C1AllorsInteger = 12;
                    c1.C1AllorsString = "a string";
                    c1.C1AllorsUnique = new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66");

                    Assert.Equal(new byte[] { 1, 2 }, c1.C1AllorsBinary);
                    Assert.True(c1.C1AllorsBoolean);
                    Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1.C1AllorsDateTime);
                    Assert.Equal(10.10m, c1.C1AllorsDecimal);
                    Assert.Equal(11.11d, c1.C1AllorsDouble);
                    Assert.Equal(12, c1.C1AllorsInteger);
                    Assert.Equal("a string", c1.C1AllorsString);
                    Assert.Equal(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"), c1.C1AllorsUnique);

                    if (c1.Strategy.Id > 0)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    Assert.Equal(new byte[] { 1, 2 }, c1.C1AllorsBinary);
                    Assert.True(c1.C1AllorsBoolean);
                    Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1.C1AllorsDateTime);
                    Assert.Equal(10.10m, c1.C1AllorsDecimal);
                    Assert.Equal(11.11d, c1.C1AllorsDouble);
                    Assert.Equal(12, c1.C1AllorsInteger);
                    Assert.Equal("a string", c1.C1AllorsString);
                    Assert.Equal(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"), c1.C1AllorsUnique);
                }
            }
        }

        [Fact]
        public async void SetRoleNull()
        {
            foreach (DatabaseMode mode in Enum.GetValues(typeof(DatabaseMode)))
            {
                foreach (var contextFactory in this.contextFactories)
                {
                    var ctx = contextFactory();
                    var (session1, _) = ctx;

                    var c1 = await ctx.Create<C1>(session1, mode);

                    Assert.NotNull(c1);

                    if (!c1.CanWriteC1C1One2One)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    c1.C1AllorsBinary = null;
                    c1.C1AllorsBoolean = null;
                    c1.C1AllorsDateTime = null;
                    c1.C1AllorsDecimal = null;
                    c1.C1AllorsDouble = null;
                    c1.C1AllorsInteger = null;
                    c1.C1AllorsString = null;
                    c1.C1AllorsUnique = null;

                    Assert.False(c1.ExistC1AllorsBinary);
                    Assert.False(c1.ExistC1AllorsBoolean);
                    Assert.False(c1.ExistC1AllorsDateTime);
                    Assert.False(c1.ExistC1AllorsDecimal);
                    Assert.False(c1.ExistC1AllorsDouble);
                    Assert.False(c1.ExistC1AllorsInteger);
                    Assert.False(c1.ExistC1AllorsString);
                    Assert.False(c1.ExistC1AllorsUnique);

                    Assert.Null(c1.C1AllorsBinary);
                    Assert.Null(c1.C1AllorsBoolean);
                    Assert.Null(c1.C1AllorsDateTime);
                    Assert.Null(c1.C1AllorsDecimal);
                    Assert.Null(c1.C1AllorsDouble);
                    Assert.Null(c1.C1AllorsInteger);
                    Assert.Null(c1.C1AllorsString);
                    Assert.Null(c1.C1AllorsUnique);

                    if (c1.Strategy.Id > 0)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    Assert.False(c1.ExistC1AllorsBinary);
                    Assert.False(c1.ExistC1AllorsBoolean);
                    Assert.False(c1.ExistC1AllorsDateTime);
                    Assert.False(c1.ExistC1AllorsDecimal);
                    Assert.False(c1.ExistC1AllorsDouble);
                    Assert.False(c1.ExistC1AllorsInteger);
                    Assert.False(c1.ExistC1AllorsString);
                    Assert.False(c1.ExistC1AllorsUnique);

                    Assert.Null(c1.C1AllorsBinary);
                    Assert.Null(c1.C1AllorsBoolean);
                    Assert.Null(c1.C1AllorsDateTime);
                    Assert.Null(c1.C1AllorsDecimal);
                    Assert.Null(c1.C1AllorsDouble);
                    Assert.Null(c1.C1AllorsInteger);
                    Assert.Null(c1.C1AllorsString);
                    Assert.Null(c1.C1AllorsUnique);

                    if (!c1.CanWriteC1C1One2One)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    c1.C1AllorsBinary = new byte[] { 1, 2 };
                    c1.C1AllorsBoolean = true;
                    c1.C1AllorsDateTime = new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc);
                    c1.C1AllorsDecimal = 10.10m;
                    c1.C1AllorsDouble = 11.11d;
                    c1.C1AllorsInteger = 12;
                    c1.C1AllorsString = "a string";
                    c1.C1AllorsUnique = new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66");


                    if (!c1.CanWriteC1C1One2One)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    c1.C1AllorsBinary = null;
                    c1.C1AllorsBoolean = null;
                    c1.C1AllorsDateTime = null;
                    c1.C1AllorsDecimal = null;
                    c1.C1AllorsDouble = null;
                    c1.C1AllorsInteger = null;
                    c1.C1AllorsString = null;
                    c1.C1AllorsUnique = null;

                    Assert.False(c1.ExistC1AllorsBinary);
                    Assert.False(c1.ExistC1AllorsBoolean);
                    Assert.False(c1.ExistC1AllorsDateTime);
                    Assert.False(c1.ExistC1AllorsDecimal);
                    Assert.False(c1.ExistC1AllorsDouble);
                    Assert.False(c1.ExistC1AllorsInteger);
                    Assert.False(c1.ExistC1AllorsString);
                    Assert.False(c1.ExistC1AllorsUnique);

                    Assert.Null(c1.C1AllorsBinary);
                    Assert.Null(c1.C1AllorsBoolean);
                    Assert.Null(c1.C1AllorsDateTime);
                    Assert.Null(c1.C1AllorsDecimal);
                    Assert.Null(c1.C1AllorsDouble);
                    Assert.Null(c1.C1AllorsInteger);
                    Assert.Null(c1.C1AllorsString);
                    Assert.Null(c1.C1AllorsUnique);

                    if (c1.Strategy.Id > 0)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    Assert.False(c1.ExistC1AllorsBinary);
                    Assert.False(c1.ExistC1AllorsBoolean);
                    Assert.False(c1.ExistC1AllorsDateTime);
                    Assert.False(c1.ExistC1AllorsDecimal);
                    Assert.False(c1.ExistC1AllorsDouble);
                    Assert.False(c1.ExistC1AllorsInteger);
                    Assert.False(c1.ExistC1AllorsString);
                    Assert.False(c1.ExistC1AllorsUnique);

                    Assert.Null(c1.C1AllorsBinary);
                    Assert.Null(c1.C1AllorsBoolean);
                    Assert.Null(c1.C1AllorsDateTime);
                    Assert.Null(c1.C1AllorsDecimal);
                    Assert.Null(c1.C1AllorsDouble);
                    Assert.Null(c1.C1AllorsInteger);
                    Assert.Null(c1.C1AllorsString);
                    Assert.Null(c1.C1AllorsUnique);
                }
            }
        }

        [Fact]
        public async void RemoveRole()
        {
            foreach (DatabaseMode mode in Enum.GetValues(typeof(DatabaseMode)))
            {
                foreach (var contextFactory in this.contextFactories)
                {
                    var ctx = contextFactory();
                    var (session1, _) = ctx;

                    var c1 = await ctx.Create<C1>(session1, mode);

                    Assert.NotNull(c1);

                    if (!c1.CanWriteC1C1One2One)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    c1.RemoveC1AllorsBinary();
                    c1.RemoveC1AllorsBoolean();
                    c1.RemoveC1AllorsDateTime();
                    c1.RemoveC1AllorsDecimal();
                    c1.RemoveC1AllorsDouble();
                    c1.RemoveC1AllorsInteger();
                    c1.RemoveC1AllorsString();
                    c1.RemoveC1AllorsUnique();

                    Assert.False(c1.ExistC1AllorsBinary);
                    Assert.False(c1.ExistC1AllorsBoolean);
                    Assert.False(c1.ExistC1AllorsDateTime);
                    Assert.False(c1.ExistC1AllorsDecimal);
                    Assert.False(c1.ExistC1AllorsDouble);
                    Assert.False(c1.ExistC1AllorsInteger);
                    Assert.False(c1.ExistC1AllorsString);
                    Assert.False(c1.ExistC1AllorsUnique);

                    Assert.Null(c1.C1AllorsBinary);
                    Assert.Null(c1.C1AllorsBoolean);
                    Assert.Null(c1.C1AllorsDateTime);
                    Assert.Null(c1.C1AllorsDecimal);
                    Assert.Null(c1.C1AllorsDouble);
                    Assert.Null(c1.C1AllorsInteger);
                    Assert.Null(c1.C1AllorsString);
                    Assert.Null(c1.C1AllorsUnique);

                    if (c1.Strategy.Id > 0)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    Assert.False(c1.ExistC1AllorsBinary);
                    Assert.False(c1.ExistC1AllorsBoolean);
                    Assert.False(c1.ExistC1AllorsDateTime);
                    Assert.False(c1.ExistC1AllorsDecimal);
                    Assert.False(c1.ExistC1AllorsDouble);
                    Assert.False(c1.ExistC1AllorsInteger);
                    Assert.False(c1.ExistC1AllorsString);
                    Assert.False(c1.ExistC1AllorsUnique);

                    Assert.Null(c1.C1AllorsBinary);
                    Assert.Null(c1.C1AllorsBoolean);
                    Assert.Null(c1.C1AllorsDateTime);
                    Assert.Null(c1.C1AllorsDecimal);
                    Assert.Null(c1.C1AllorsDouble);
                    Assert.Null(c1.C1AllorsInteger);
                    Assert.Null(c1.C1AllorsString);
                    Assert.Null(c1.C1AllorsUnique);

                    if (!c1.CanWriteC1C1One2One)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    c1.C1AllorsBinary = new byte[] { 1, 2 };
                    c1.C1AllorsBoolean = true;
                    c1.C1AllorsDateTime = new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc);
                    c1.C1AllorsDecimal = 10.10m;
                    c1.C1AllorsDouble = 11.11d;
                    c1.C1AllorsInteger = 12;
                    c1.C1AllorsString = "a string";
                    c1.C1AllorsUnique = new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66");

                    if (!c1.CanWriteC1C1One2One)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    c1.RemoveC1AllorsBinary();
                    c1.RemoveC1AllorsBoolean();
                    c1.RemoveC1AllorsDateTime();
                    c1.RemoveC1AllorsDecimal();
                    c1.RemoveC1AllorsDouble();
                    c1.RemoveC1AllorsInteger();
                    c1.RemoveC1AllorsString();
                    c1.RemoveC1AllorsUnique();

                    Assert.False(c1.ExistC1AllorsBinary);
                    Assert.False(c1.ExistC1AllorsBoolean);
                    Assert.False(c1.ExistC1AllorsDateTime);
                    Assert.False(c1.ExistC1AllorsDecimal);
                    Assert.False(c1.ExistC1AllorsDouble);
                    Assert.False(c1.ExistC1AllorsInteger);
                    Assert.False(c1.ExistC1AllorsString);
                    Assert.False(c1.ExistC1AllorsUnique);

                    Assert.Null(c1.C1AllorsBinary);
                    Assert.Null(c1.C1AllorsBoolean);
                    Assert.Null(c1.C1AllorsDateTime);
                    Assert.Null(c1.C1AllorsDecimal);
                    Assert.Null(c1.C1AllorsDouble);
                    Assert.Null(c1.C1AllorsInteger);
                    Assert.Null(c1.C1AllorsString);
                    Assert.Null(c1.C1AllorsUnique);

                    if (c1.Strategy.Id > 0)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    Assert.False(c1.ExistC1AllorsBinary);
                    Assert.False(c1.ExistC1AllorsBoolean);
                    Assert.False(c1.ExistC1AllorsDateTime);
                    Assert.False(c1.ExistC1AllorsDecimal);
                    Assert.False(c1.ExistC1AllorsDouble);
                    Assert.False(c1.ExistC1AllorsInteger);
                    Assert.False(c1.ExistC1AllorsString);
                    Assert.False(c1.ExistC1AllorsUnique);

                    Assert.Null(c1.C1AllorsBinary);
                    Assert.Null(c1.C1AllorsBoolean);
                    Assert.Null(c1.C1AllorsDateTime);
                    Assert.Null(c1.C1AllorsDecimal);
                    Assert.Null(c1.C1AllorsDouble);
                    Assert.Null(c1.C1AllorsInteger);
                    Assert.Null(c1.C1AllorsString);
                    Assert.Null(c1.C1AllorsUnique);
                }
            }
        }
    }
}

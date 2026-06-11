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

                    c1.C1AllorsBinary.Set(new byte[] { 1, 2 });
                    c1.C1AllorsBoolean.Set(true);
                    c1.C1AllorsDateTime.Set(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc));
                    c1.C1AllorsDecimal.Set(10.10m);
                    c1.C1AllorsDouble.Set(11.11d);
                    c1.C1AllorsInteger.Set(12);
                    c1.C1AllorsString.Set("a string");
                    c1.C1AllorsUnique.Set(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"));

                    Assert.Equal(new byte[] { 1, 2 }, c1.C1AllorsBinary.Value);
                    Assert.True(c1.C1AllorsBoolean.Value);
                    Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1.C1AllorsDateTime.Value);
                    Assert.Equal(10.10m, c1.C1AllorsDecimal.Value);
                    Assert.Equal(11.11d, c1.C1AllorsDouble.Value);
                    Assert.Equal(12, c1.C1AllorsInteger.Value);
                    Assert.Equal("a string", c1.C1AllorsString.Value);
                    Assert.Equal(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"), c1.C1AllorsUnique.Value);

                    if (c1.Strategy.Id > 0)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    Assert.Equal(new byte[] { 1, 2 }, c1.C1AllorsBinary.Value);
                    Assert.True(c1.C1AllorsBoolean.Value);
                    Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1.C1AllorsDateTime.Value);
                    Assert.Equal(10.10m, c1.C1AllorsDecimal.Value);
                    Assert.Equal(11.11d, c1.C1AllorsDouble.Value);
                    Assert.Equal(12, c1.C1AllorsInteger.Value);
                    Assert.Equal("a string", c1.C1AllorsString.Value);
                    Assert.Equal(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"), c1.C1AllorsUnique.Value);
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

                    c1.C1AllorsBinary.Set(null);
                    c1.C1AllorsBoolean.Set(null);
                    c1.C1AllorsDateTime.Set(null);
                    c1.C1AllorsDecimal.Set(null);
                    c1.C1AllorsDouble.Set(null);
                    c1.C1AllorsInteger.Set(null);
                    c1.C1AllorsString.Set(null);
                    c1.C1AllorsUnique.Set(null);

                    Assert.False(c1.ExistC1AllorsBinary);
                    Assert.False(c1.ExistC1AllorsBoolean);
                    Assert.False(c1.ExistC1AllorsDateTime);
                    Assert.False(c1.ExistC1AllorsDecimal);
                    Assert.False(c1.ExistC1AllorsDouble);
                    Assert.False(c1.ExistC1AllorsInteger);
                    Assert.False(c1.ExistC1AllorsString);
                    Assert.False(c1.ExistC1AllorsUnique);

                    Assert.Null(c1.C1AllorsBinary.Value);
                    Assert.Null(c1.C1AllorsBoolean.Value);
                    Assert.Null(c1.C1AllorsDateTime.Value);
                    Assert.Null(c1.C1AllorsDecimal.Value);
                    Assert.Null(c1.C1AllorsDouble.Value);
                    Assert.Null(c1.C1AllorsInteger.Value);
                    Assert.Null(c1.C1AllorsString.Value);
                    Assert.Null(c1.C1AllorsUnique.Value);

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

                    Assert.Null(c1.C1AllorsBinary.Value);
                    Assert.Null(c1.C1AllorsBoolean.Value);
                    Assert.Null(c1.C1AllorsDateTime.Value);
                    Assert.Null(c1.C1AllorsDecimal.Value);
                    Assert.Null(c1.C1AllorsDouble.Value);
                    Assert.Null(c1.C1AllorsInteger.Value);
                    Assert.Null(c1.C1AllorsString.Value);
                    Assert.Null(c1.C1AllorsUnique.Value);

                    if (!c1.CanWriteC1C1One2One)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    c1.C1AllorsBinary.Set(new byte[] { 1, 2 });
                    c1.C1AllorsBoolean.Set(true);
                    c1.C1AllorsDateTime.Set(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc));
                    c1.C1AllorsDecimal.Set(10.10m);
                    c1.C1AllorsDouble.Set(11.11d);
                    c1.C1AllorsInteger.Set(12);
                    c1.C1AllorsString.Set("a string");
                    c1.C1AllorsUnique.Set(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"));


                    if (!c1.CanWriteC1C1One2One)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    c1.C1AllorsBinary.Set(null);
                    c1.C1AllorsBoolean.Set(null);
                    c1.C1AllorsDateTime.Set(null);
                    c1.C1AllorsDecimal.Set(null);
                    c1.C1AllorsDouble.Set(null);
                    c1.C1AllorsInteger.Set(null);
                    c1.C1AllorsString.Set(null);
                    c1.C1AllorsUnique.Set(null);

                    Assert.False(c1.ExistC1AllorsBinary);
                    Assert.False(c1.ExistC1AllorsBoolean);
                    Assert.False(c1.ExistC1AllorsDateTime);
                    Assert.False(c1.ExistC1AllorsDecimal);
                    Assert.False(c1.ExistC1AllorsDouble);
                    Assert.False(c1.ExistC1AllorsInteger);
                    Assert.False(c1.ExistC1AllorsString);
                    Assert.False(c1.ExistC1AllorsUnique);

                    Assert.Null(c1.C1AllorsBinary.Value);
                    Assert.Null(c1.C1AllorsBoolean.Value);
                    Assert.Null(c1.C1AllorsDateTime.Value);
                    Assert.Null(c1.C1AllorsDecimal.Value);
                    Assert.Null(c1.C1AllorsDouble.Value);
                    Assert.Null(c1.C1AllorsInteger.Value);
                    Assert.Null(c1.C1AllorsString.Value);
                    Assert.Null(c1.C1AllorsUnique.Value);

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

                    Assert.Null(c1.C1AllorsBinary.Value);
                    Assert.Null(c1.C1AllorsBoolean.Value);
                    Assert.Null(c1.C1AllorsDateTime.Value);
                    Assert.Null(c1.C1AllorsDecimal.Value);
                    Assert.Null(c1.C1AllorsDouble.Value);
                    Assert.Null(c1.C1AllorsInteger.Value);
                    Assert.Null(c1.C1AllorsString.Value);
                    Assert.Null(c1.C1AllorsUnique.Value);
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

                    Assert.Null(c1.C1AllorsBinary.Value);
                    Assert.Null(c1.C1AllorsBoolean.Value);
                    Assert.Null(c1.C1AllorsDateTime.Value);
                    Assert.Null(c1.C1AllorsDecimal.Value);
                    Assert.Null(c1.C1AllorsDouble.Value);
                    Assert.Null(c1.C1AllorsInteger.Value);
                    Assert.Null(c1.C1AllorsString.Value);
                    Assert.Null(c1.C1AllorsUnique.Value);

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

                    Assert.Null(c1.C1AllorsBinary.Value);
                    Assert.Null(c1.C1AllorsBoolean.Value);
                    Assert.Null(c1.C1AllorsDateTime.Value);
                    Assert.Null(c1.C1AllorsDecimal.Value);
                    Assert.Null(c1.C1AllorsDouble.Value);
                    Assert.Null(c1.C1AllorsInteger.Value);
                    Assert.Null(c1.C1AllorsString.Value);
                    Assert.Null(c1.C1AllorsUnique.Value);

                    if (!c1.CanWriteC1C1One2One)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    c1.C1AllorsBinary.Set(new byte[] { 1, 2 });
                    c1.C1AllorsBoolean.Set(true);
                    c1.C1AllorsDateTime.Set(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc));
                    c1.C1AllorsDecimal.Set(10.10m);
                    c1.C1AllorsDouble.Set(11.11d);
                    c1.C1AllorsInteger.Set(12);
                    c1.C1AllorsString.Set("a string");
                    c1.C1AllorsUnique.Set(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"));

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

                    Assert.Null(c1.C1AllorsBinary.Value);
                    Assert.Null(c1.C1AllorsBoolean.Value);
                    Assert.Null(c1.C1AllorsDateTime.Value);
                    Assert.Null(c1.C1AllorsDecimal.Value);
                    Assert.Null(c1.C1AllorsDouble.Value);
                    Assert.Null(c1.C1AllorsInteger.Value);
                    Assert.Null(c1.C1AllorsString.Value);
                    Assert.Null(c1.C1AllorsUnique.Value);

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

                    Assert.Null(c1.C1AllorsBinary.Value);
                    Assert.Null(c1.C1AllorsBoolean.Value);
                    Assert.Null(c1.C1AllorsDateTime.Value);
                    Assert.Null(c1.C1AllorsDecimal.Value);
                    Assert.Null(c1.C1AllorsDouble.Value);
                    Assert.Null(c1.C1AllorsInteger.Value);
                    Assert.Null(c1.C1AllorsString.Value);
                    Assert.Null(c1.C1AllorsUnique.Value);
                }
            }
        }
    }
}

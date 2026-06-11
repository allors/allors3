// <copyright file="Many2OneTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.DatabaseAssociation.SessionRelation
{
    using System;
    using System.Threading.Tasks;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

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
            foreach (var contextFactory in this.contextFactories)
            {
                foreach (DatabaseMode mode in Enum.GetValues(typeof(DatabaseMode)))
                {
                    var ctx = contextFactory();
                    var (session1, _) = ctx;

                    var c1 = await ctx.Create<C1>(session1, mode);

                    Assert.NotNull(c1);

                    if (!c1.CanWriteC1C1One2One)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    c1.SessionAllorsBinary.Set(new byte[] { 1, 2 });
                    c1.SessionAllorsBoolean.Set(true);
                    c1.SessionAllorsDateTime.Set(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc));
                    c1.SessionAllorsDecimal.Set(10.10m);
                    c1.SessionAllorsDouble.Set(11.11d);
                    c1.SessionAllorsInteger.Set(12);
                    c1.SessionAllorsString.Set("a string");
                    c1.SessionAllorsUnique.Set(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"));

                    Assert.Equal(new byte[] { 1, 2 }, c1.SessionAllorsBinary.Value);
                    Assert.True(c1.SessionAllorsBoolean.Value);
                    Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1.SessionAllorsDateTime.Value);
                    Assert.Equal(10.10m, c1.SessionAllorsDecimal.Value);
                    Assert.Equal(11.11d, c1.SessionAllorsDouble.Value);
                    Assert.Equal(12, c1.SessionAllorsInteger.Value);
                    Assert.Equal("a string", c1.SessionAllorsString.Value);
                    Assert.Equal(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"), c1.SessionAllorsUnique.Value);

                    if (c1.Strategy.Id > 0)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    Assert.Equal(new byte[] { 1, 2 }, c1.SessionAllorsBinary.Value);
                    Assert.True(c1.SessionAllorsBoolean.Value);
                    Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1.SessionAllorsDateTime.Value);
                    Assert.Equal(10.10m, c1.SessionAllorsDecimal.Value);
                    Assert.Equal(11.11d, c1.SessionAllorsDouble.Value);
                    Assert.Equal(12, c1.SessionAllorsInteger.Value);
                    Assert.Equal("a string", c1.SessionAllorsString.Value);
                    Assert.Equal(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"), c1.SessionAllorsUnique.Value);
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

                    c1.SessionAllorsBinary.Set(null);
                    c1.SessionAllorsBoolean.Set(null);
                    c1.SessionAllorsDateTime.Set(null);
                    c1.SessionAllorsDecimal.Set(null);
                    c1.SessionAllorsDouble.Set(null);
                    c1.SessionAllorsInteger.Set(null);
                    c1.SessionAllorsString.Set(null);
                    c1.SessionAllorsUnique.Set(null);

                    Assert.False(c1.ExistSessionAllorsBinary);
                    Assert.False(c1.ExistSessionAllorsBoolean);
                    Assert.False(c1.ExistSessionAllorsDateTime);
                    Assert.False(c1.ExistSessionAllorsDecimal);
                    Assert.False(c1.ExistSessionAllorsDouble);
                    Assert.False(c1.ExistSessionAllorsInteger);
                    Assert.False(c1.ExistSessionAllorsString);
                    Assert.False(c1.ExistSessionAllorsUnique);

                    Assert.Null(c1.SessionAllorsBinary.Value);
                    Assert.Null(c1.SessionAllorsBoolean.Value);
                    Assert.Null(c1.SessionAllorsDateTime.Value);
                    Assert.Null(c1.SessionAllorsDecimal.Value);
                    Assert.Null(c1.SessionAllorsDouble.Value);
                    Assert.Null(c1.SessionAllorsInteger.Value);
                    Assert.Null(c1.SessionAllorsString.Value);
                    Assert.Null(c1.SessionAllorsUnique.Value);

                    if (c1.Strategy.Id > 0)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    Assert.False(c1.ExistSessionAllorsBinary);
                    Assert.False(c1.ExistSessionAllorsBoolean);
                    Assert.False(c1.ExistSessionAllorsDateTime);
                    Assert.False(c1.ExistSessionAllorsDecimal);
                    Assert.False(c1.ExistSessionAllorsDouble);
                    Assert.False(c1.ExistSessionAllorsInteger);
                    Assert.False(c1.ExistSessionAllorsString);
                    Assert.False(c1.ExistSessionAllorsUnique);

                    Assert.Null(c1.SessionAllorsBinary.Value);
                    Assert.Null(c1.SessionAllorsBoolean.Value);
                    Assert.Null(c1.SessionAllorsDateTime.Value);
                    Assert.Null(c1.SessionAllorsDecimal.Value);
                    Assert.Null(c1.SessionAllorsDouble.Value);
                    Assert.Null(c1.SessionAllorsInteger.Value);
                    Assert.Null(c1.SessionAllorsString.Value);
                    Assert.Null(c1.SessionAllorsUnique.Value);

                    if (!c1.CanWriteC1C1One2One)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    c1.SessionAllorsBinary.Set(new byte[] { 1, 2 });
                    c1.SessionAllorsBoolean.Set(true);
                    c1.SessionAllorsDateTime.Set(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc));
                    c1.SessionAllorsDecimal.Set(10.10m);
                    c1.SessionAllorsDouble.Set(11.11d);
                    c1.SessionAllorsInteger.Set(12);
                    c1.SessionAllorsString.Set("a string");
                    c1.SessionAllorsUnique.Set(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"));

                    if (!c1.CanWriteC1C1One2One)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    c1.SessionAllorsBinary.Set(null);
                    c1.SessionAllorsBoolean.Set(null);
                    c1.SessionAllorsDateTime.Set(null);
                    c1.SessionAllorsDecimal.Set(null);
                    c1.SessionAllorsDouble.Set(null);
                    c1.SessionAllorsInteger.Set(null);
                    c1.SessionAllorsString.Set(null);
                    c1.SessionAllorsUnique.Set(null);

                    Assert.False(c1.ExistSessionAllorsBinary);
                    Assert.False(c1.ExistSessionAllorsBoolean);
                    Assert.False(c1.ExistSessionAllorsDateTime);
                    Assert.False(c1.ExistSessionAllorsDecimal);
                    Assert.False(c1.ExistSessionAllorsDouble);
                    Assert.False(c1.ExistSessionAllorsInteger);
                    Assert.False(c1.ExistSessionAllorsString);
                    Assert.False(c1.ExistSessionAllorsUnique);

                    Assert.Null(c1.SessionAllorsBinary.Value);
                    Assert.Null(c1.SessionAllorsBoolean.Value);
                    Assert.Null(c1.SessionAllorsDateTime.Value);
                    Assert.Null(c1.SessionAllorsDecimal.Value);
                    Assert.Null(c1.SessionAllorsDouble.Value);
                    Assert.Null(c1.SessionAllorsInteger.Value);
                    Assert.Null(c1.SessionAllorsString.Value);
                    Assert.Null(c1.SessionAllorsUnique.Value);

                    if (c1.Strategy.Id > 0)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    Assert.False(c1.ExistSessionAllorsBinary);
                    Assert.False(c1.ExistSessionAllorsBoolean);
                    Assert.False(c1.ExistSessionAllorsDateTime);
                    Assert.False(c1.ExistSessionAllorsDecimal);
                    Assert.False(c1.ExistSessionAllorsDouble);
                    Assert.False(c1.ExistSessionAllorsInteger);
                    Assert.False(c1.ExistSessionAllorsString);
                    Assert.False(c1.ExistSessionAllorsUnique);

                    Assert.Null(c1.SessionAllorsBinary.Value);
                    Assert.Null(c1.SessionAllorsBoolean.Value);
                    Assert.Null(c1.SessionAllorsDateTime.Value);
                    Assert.Null(c1.SessionAllorsDecimal.Value);
                    Assert.Null(c1.SessionAllorsDouble.Value);
                    Assert.Null(c1.SessionAllorsInteger.Value);
                    Assert.Null(c1.SessionAllorsString.Value);
                    Assert.Null(c1.SessionAllorsUnique.Value);
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

                    c1.RemoveSessionAllorsBinary();
                    c1.RemoveSessionAllorsBoolean();
                    c1.RemoveSessionAllorsDateTime();
                    c1.RemoveSessionAllorsDecimal();
                    c1.RemoveSessionAllorsDouble();
                    c1.RemoveSessionAllorsInteger();
                    c1.RemoveSessionAllorsString();
                    c1.RemoveSessionAllorsUnique();

                    Assert.False(c1.ExistSessionAllorsBinary);
                    Assert.False(c1.ExistSessionAllorsBoolean);
                    Assert.False(c1.ExistSessionAllorsDateTime);
                    Assert.False(c1.ExistSessionAllorsDecimal);
                    Assert.False(c1.ExistSessionAllorsDouble);
                    Assert.False(c1.ExistSessionAllorsInteger);
                    Assert.False(c1.ExistSessionAllorsString);
                    Assert.False(c1.ExistSessionAllorsUnique);

                    Assert.Null(c1.SessionAllorsBinary.Value);
                    Assert.Null(c1.SessionAllorsBoolean.Value);
                    Assert.Null(c1.SessionAllorsDateTime.Value);
                    Assert.Null(c1.SessionAllorsDecimal.Value);
                    Assert.Null(c1.SessionAllorsDouble.Value);
                    Assert.Null(c1.SessionAllorsInteger.Value);
                    Assert.Null(c1.SessionAllorsString.Value);
                    Assert.Null(c1.SessionAllorsUnique.Value);

                    if (c1.Strategy.Id > 0)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    Assert.False(c1.ExistSessionAllorsBinary);
                    Assert.False(c1.ExistSessionAllorsBoolean);
                    Assert.False(c1.ExistSessionAllorsDateTime);
                    Assert.False(c1.ExistSessionAllorsDecimal);
                    Assert.False(c1.ExistSessionAllorsDouble);
                    Assert.False(c1.ExistSessionAllorsInteger);
                    Assert.False(c1.ExistSessionAllorsString);
                    Assert.False(c1.ExistSessionAllorsUnique);

                    Assert.Null(c1.SessionAllorsBinary.Value);
                    Assert.Null(c1.SessionAllorsBoolean.Value);
                    Assert.Null(c1.SessionAllorsDateTime.Value);
                    Assert.Null(c1.SessionAllorsDecimal.Value);
                    Assert.Null(c1.SessionAllorsDouble.Value);
                    Assert.Null(c1.SessionAllorsInteger.Value);
                    Assert.Null(c1.SessionAllorsString.Value);
                    Assert.Null(c1.SessionAllorsUnique.Value);

                    if (!c1.CanWriteC1C1One2One)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    c1.SessionAllorsBinary.Set(new byte[] { 1, 2 });
                    c1.SessionAllorsBoolean.Set(true);
                    c1.SessionAllorsDateTime.Set(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc));
                    c1.SessionAllorsDecimal.Set(10.10m);
                    c1.SessionAllorsDouble.Set(11.11d);
                    c1.SessionAllorsInteger.Set(12);
                    c1.SessionAllorsString.Set("a string");
                    c1.SessionAllorsUnique.Set(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"));

                    if (!c1.CanWriteC1C1One2One)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    c1.RemoveSessionAllorsBinary();
                    c1.RemoveSessionAllorsBoolean();
                    c1.RemoveSessionAllorsDateTime();
                    c1.RemoveSessionAllorsDecimal();
                    c1.RemoveSessionAllorsDouble();
                    c1.RemoveSessionAllorsInteger();
                    c1.RemoveSessionAllorsString();
                    c1.RemoveSessionAllorsUnique();

                    Assert.False(c1.ExistSessionAllorsBinary);
                    Assert.False(c1.ExistSessionAllorsBoolean);
                    Assert.False(c1.ExistSessionAllorsDateTime);
                    Assert.False(c1.ExistSessionAllorsDecimal);
                    Assert.False(c1.ExistSessionAllorsDouble);
                    Assert.False(c1.ExistSessionAllorsInteger);
                    Assert.False(c1.ExistSessionAllorsString);
                    Assert.False(c1.ExistSessionAllorsUnique);

                    Assert.Null(c1.SessionAllorsBinary.Value);
                    Assert.Null(c1.SessionAllorsBoolean.Value);
                    Assert.Null(c1.SessionAllorsDateTime.Value);
                    Assert.Null(c1.SessionAllorsDecimal.Value);
                    Assert.Null(c1.SessionAllorsDouble.Value);
                    Assert.Null(c1.SessionAllorsInteger.Value);
                    Assert.Null(c1.SessionAllorsString.Value);
                    Assert.Null(c1.SessionAllorsUnique.Value);

                    if (c1.Strategy.Id > 0)
                    {
                        await session1.PullAsync(new Pull { Object = c1 });
                    }

                    Assert.False(c1.ExistSessionAllorsBinary);
                    Assert.False(c1.ExistSessionAllorsBoolean);
                    Assert.False(c1.ExistSessionAllorsDateTime);
                    Assert.False(c1.ExistSessionAllorsDecimal);
                    Assert.False(c1.ExistSessionAllorsDouble);
                    Assert.False(c1.ExistSessionAllorsInteger);
                    Assert.False(c1.ExistSessionAllorsString);
                    Assert.False(c1.ExistSessionAllorsUnique);

                    Assert.Null(c1.SessionAllorsBinary.Value);
                    Assert.Null(c1.SessionAllorsBoolean.Value);
                    Assert.Null(c1.SessionAllorsDateTime.Value);
                    Assert.Null(c1.SessionAllorsDecimal.Value);
                    Assert.Null(c1.SessionAllorsDouble.Value);
                    Assert.Null(c1.SessionAllorsInteger.Value);
                    Assert.Null(c1.SessionAllorsString.Value);
                    Assert.Null(c1.SessionAllorsUnique.Value);
                }
            }
        }
    }
}

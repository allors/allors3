// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.DatabaseAssociation.WorkspaceRelation
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class UnitTests : Test
    {
        private Func<Context>[] contextFactories;
        private Func<ISession, Task>[] pushes;

        protected UnitTests(Fixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");

            var singleSessionContext = new SingleSessionContext(this, "Single shared");
            var multipleSessionContext = new MultipleSessionContext(this, "Multiple shared");

            this.pushes = new Func<ISession, Task>[]
            {
                (_) => Task.CompletedTask,
                (session) =>
                {
                    session.PushToWorkspace();
                    return Task.CompletedTask;
                },
                (session) =>
                {
                    session.PullFromWorkspace();
                    return Task.CompletedTask;
                },
                (session) =>
                {
                    session.PushToWorkspace();
                    session.PullFromWorkspace();
                    return Task.CompletedTask;
                },
                async (session) => await this.AsyncDatabaseClient.PushAsync(session),
            };

            this.contextFactories = new Func<Context>[]
            {
                () => singleSessionContext,
                () => new SingleSessionContext(this, "Single"),
                () => multipleSessionContext,
                () => new MultipleSessionContext(this, "Multiple"),
            };
        }

        [Fact]
        public async void SetRole()
        {
            var counter = 0;
            foreach (var push in this.pushes)
            {
                foreach (DatabaseMode mode in Enum.GetValues(typeof(DatabaseMode)))
                {
                    foreach (var contextFactory in this.contextFactories)
                    {
                        ++counter;

                        var ctx = contextFactory();
                        var (session1, _) = ctx;

                        var c1 = await ctx.Create<C1>(session1, mode);

                        Assert.NotNull(c1);

                        await push(session1);

                        if (!c1.CanWriteC1C1One2One)
                        {
                            await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1 });
                        }

                        c1.WorkspaceAllorsBinary = new byte[] { 1, 2 };
                        c1.WorkspaceAllorsBoolean = true;
                        c1.WorkspaceAllorsDateTime = new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc);
                        c1.WorkspaceAllorsDecimal = 10.10m;
                        c1.WorkspaceAllorsDouble = 11.11d;
                        c1.WorkspaceAllorsInteger = 12;
                        c1.WorkspaceAllorsString = "a string";
                        c1.WorkspaceAllorsUnique = new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66");

                        Assert.Equal(new byte[] { 1, 2 }, c1.WorkspaceAllorsBinary);
                        Assert.True(c1.WorkspaceAllorsBoolean);
                        Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1.WorkspaceAllorsDateTime);
                        Assert.Equal(10.10m, c1.WorkspaceAllorsDecimal);
                        Assert.Equal(11.11d, c1.WorkspaceAllorsDouble);
                        Assert.Equal(12, c1.WorkspaceAllorsInteger);
                        Assert.Equal("a string", c1.WorkspaceAllorsString);
                        Assert.Equal(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"), c1.WorkspaceAllorsUnique);

                        if (counter == 41)
                        {
                            Debugger.Break();
                        }

                        await push(session1);


                        Assert.Equal(new byte[] { 1, 2 }, c1.WorkspaceAllorsBinary);
                        Assert.True(c1.WorkspaceAllorsBoolean);
                        Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1.WorkspaceAllorsDateTime);
                        Assert.Equal(10.10m, c1.WorkspaceAllorsDecimal);
                        Assert.Equal(11.11d, c1.WorkspaceAllorsDouble);
                        Assert.Equal(12, c1.WorkspaceAllorsInteger);
                        Assert.Equal("a string", c1.WorkspaceAllorsString);
                        Assert.Equal(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"), c1.WorkspaceAllorsUnique);

                        if (c1.Strategy.Id > 0)
                        {
                            await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1 });
                        }

                        Assert.Equal(new byte[] { 1, 2 }, c1.WorkspaceAllorsBinary);
                        Assert.True(c1.WorkspaceAllorsBoolean);
                        Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1.WorkspaceAllorsDateTime);
                        Assert.Equal(10.10m, c1.WorkspaceAllorsDecimal);
                        Assert.Equal(11.11d, c1.WorkspaceAllorsDouble);
                        Assert.Equal(12, c1.WorkspaceAllorsInteger);
                        Assert.Equal("a string", c1.WorkspaceAllorsString);
                        Assert.Equal(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"), c1.WorkspaceAllorsUnique);

                        await push(session1);

                        Assert.Equal(new byte[] { 1, 2 }, c1.WorkspaceAllorsBinary);
                        Assert.True(c1.WorkspaceAllorsBoolean);
                        Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1.WorkspaceAllorsDateTime);
                        Assert.Equal(10.10m, c1.WorkspaceAllorsDecimal);
                        Assert.Equal(11.11d, c1.WorkspaceAllorsDouble);
                        Assert.Equal(12, c1.WorkspaceAllorsInteger);
                        Assert.Equal("a string", c1.WorkspaceAllorsString);
                        Assert.Equal(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"), c1.WorkspaceAllorsUnique);
                    }
                }
            }
        }

        [Fact]
        public async void SetRoleNull()
        {
            foreach (var push in this.pushes)
            {
                foreach (DatabaseMode mode in Enum.GetValues(typeof(DatabaseMode)))
                {
                    foreach (var contextFactory in this.contextFactories)
                    {
                        var ctx = contextFactory();
                        var (session1, _) = ctx;

                        var c1 = await ctx.Create<C1>(session1, mode);

                        Assert.NotNull(c1);

                        await push(session1);

                        if (!c1.CanWriteC1C1One2One)
                        {
                            await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1 });
                        }

                        c1.WorkspaceAllorsBinary = null;
                        c1.WorkspaceAllorsBoolean = null;
                        c1.WorkspaceAllorsDateTime = null;
                        c1.WorkspaceAllorsDecimal = null;
                        c1.WorkspaceAllorsDouble = null;
                        c1.WorkspaceAllorsInteger = null;
                        c1.WorkspaceAllorsString = null;
                        c1.WorkspaceAllorsUnique = null;

                        Assert.False(c1.ExistWorkspaceAllorsBinary);
                        Assert.False(c1.ExistWorkspaceAllorsBoolean);
                        Assert.False(c1.ExistWorkspaceAllorsDateTime);
                        Assert.False(c1.ExistWorkspaceAllorsDecimal);
                        Assert.False(c1.ExistWorkspaceAllorsDouble);
                        Assert.False(c1.ExistWorkspaceAllorsInteger);
                        Assert.False(c1.ExistWorkspaceAllorsString);
                        Assert.False(c1.ExistWorkspaceAllorsUnique);

                        Assert.Null(c1.WorkspaceAllorsBinary);
                        Assert.Null(c1.WorkspaceAllorsBoolean);
                        Assert.Null(c1.WorkspaceAllorsDateTime);
                        Assert.Null(c1.WorkspaceAllorsDecimal);
                        Assert.Null(c1.WorkspaceAllorsDouble);
                        Assert.Null(c1.WorkspaceAllorsInteger);
                        Assert.Null(c1.WorkspaceAllorsString);
                        Assert.Null(c1.WorkspaceAllorsUnique);

                        if (c1.Strategy.Id > 0)
                        {
                            await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1 });
                        }

                        Assert.False(c1.ExistWorkspaceAllorsBinary);
                        Assert.False(c1.ExistWorkspaceAllorsBoolean);
                        Assert.False(c1.ExistWorkspaceAllorsDateTime);
                        Assert.False(c1.ExistWorkspaceAllorsDecimal);
                        Assert.False(c1.ExistWorkspaceAllorsDouble);
                        Assert.False(c1.ExistWorkspaceAllorsInteger);
                        Assert.False(c1.ExistWorkspaceAllorsString);
                        Assert.False(c1.ExistWorkspaceAllorsUnique);

                        Assert.Null(c1.WorkspaceAllorsBinary);
                        Assert.Null(c1.WorkspaceAllorsBoolean);
                        Assert.Null(c1.WorkspaceAllorsDateTime);
                        Assert.Null(c1.WorkspaceAllorsDecimal);
                        Assert.Null(c1.WorkspaceAllorsDouble);
                        Assert.Null(c1.WorkspaceAllorsInteger);
                        Assert.Null(c1.WorkspaceAllorsString);
                        Assert.Null(c1.WorkspaceAllorsUnique);

                        await push(session1);

                        Assert.False(c1.ExistWorkspaceAllorsBinary);
                        Assert.False(c1.ExistWorkspaceAllorsBoolean);
                        Assert.False(c1.ExistWorkspaceAllorsDateTime);
                        Assert.False(c1.ExistWorkspaceAllorsDecimal);
                        Assert.False(c1.ExistWorkspaceAllorsDouble);
                        Assert.False(c1.ExistWorkspaceAllorsInteger);
                        Assert.False(c1.ExistWorkspaceAllorsString);
                        Assert.False(c1.ExistWorkspaceAllorsUnique);

                        Assert.Null(c1.WorkspaceAllorsBinary);
                        Assert.Null(c1.WorkspaceAllorsBoolean);
                        Assert.Null(c1.WorkspaceAllorsDateTime);
                        Assert.Null(c1.WorkspaceAllorsDecimal);
                        Assert.Null(c1.WorkspaceAllorsDouble);
                        Assert.Null(c1.WorkspaceAllorsInteger);
                        Assert.Null(c1.WorkspaceAllorsString);
                        Assert.Null(c1.WorkspaceAllorsUnique);

                        await push(session1);

                        if (!c1.CanWriteC1C1One2One)
                        {
                            await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1 });
                        }

                        c1.WorkspaceAllorsBinary = new byte[] { 1, 2 };
                        c1.WorkspaceAllorsBoolean = true;
                        c1.WorkspaceAllorsDateTime = new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc);
                        c1.WorkspaceAllorsDecimal = 10.10m;
                        c1.WorkspaceAllorsDouble = 11.11d;
                        c1.WorkspaceAllorsInteger = 12;
                        c1.WorkspaceAllorsString = "a string";
                        c1.WorkspaceAllorsUnique = new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66");

                        await push(session1);

                        if (!c1.CanWriteC1C1One2One)
                        {
                            await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1 });
                        }

                        c1.WorkspaceAllorsBinary = null;
                        c1.WorkspaceAllorsBoolean = null;
                        c1.WorkspaceAllorsDateTime = null;
                        c1.WorkspaceAllorsDecimal = null;
                        c1.WorkspaceAllorsDouble = null;
                        c1.WorkspaceAllorsInteger = null;
                        c1.WorkspaceAllorsString = null;
                        c1.WorkspaceAllorsUnique = null;

                        Assert.False(c1.ExistWorkspaceAllorsBinary);
                        Assert.False(c1.ExistWorkspaceAllorsBoolean);
                        Assert.False(c1.ExistWorkspaceAllorsDateTime);
                        Assert.False(c1.ExistWorkspaceAllorsDecimal);
                        Assert.False(c1.ExistWorkspaceAllorsDouble);
                        Assert.False(c1.ExistWorkspaceAllorsInteger);
                        Assert.False(c1.ExistWorkspaceAllorsString);
                        Assert.False(c1.ExistWorkspaceAllorsUnique);

                        Assert.Null(c1.WorkspaceAllorsBinary);
                        Assert.Null(c1.WorkspaceAllorsBoolean);
                        Assert.Null(c1.WorkspaceAllorsDateTime);
                        Assert.Null(c1.WorkspaceAllorsDecimal);
                        Assert.Null(c1.WorkspaceAllorsDouble);
                        Assert.Null(c1.WorkspaceAllorsInteger);
                        Assert.Null(c1.WorkspaceAllorsString);
                        Assert.Null(c1.WorkspaceAllorsUnique);

                        if (c1.Strategy.Id > 0)
                        {
                            await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1 });
                        }

                        Assert.False(c1.ExistWorkspaceAllorsBinary);
                        Assert.False(c1.ExistWorkspaceAllorsBoolean);
                        Assert.False(c1.ExistWorkspaceAllorsDateTime);
                        Assert.False(c1.ExistWorkspaceAllorsDecimal);
                        Assert.False(c1.ExistWorkspaceAllorsDouble);
                        Assert.False(c1.ExistWorkspaceAllorsInteger);
                        Assert.False(c1.ExistWorkspaceAllorsString);
                        Assert.False(c1.ExistWorkspaceAllorsUnique);

                        Assert.Null(c1.WorkspaceAllorsBinary);
                        Assert.Null(c1.WorkspaceAllorsBoolean);
                        Assert.Null(c1.WorkspaceAllorsDateTime);
                        Assert.Null(c1.WorkspaceAllorsDecimal);
                        Assert.Null(c1.WorkspaceAllorsDouble);
                        Assert.Null(c1.WorkspaceAllorsInteger);
                        Assert.Null(c1.WorkspaceAllorsString);
                        Assert.Null(c1.WorkspaceAllorsUnique);

                        await push(session1);

                        Assert.False(c1.ExistWorkspaceAllorsBinary);
                        Assert.False(c1.ExistWorkspaceAllorsBoolean);
                        Assert.False(c1.ExistWorkspaceAllorsDateTime);
                        Assert.False(c1.ExistWorkspaceAllorsDecimal);
                        Assert.False(c1.ExistWorkspaceAllorsDouble);
                        Assert.False(c1.ExistWorkspaceAllorsInteger);
                        Assert.False(c1.ExistWorkspaceAllorsString);
                        Assert.False(c1.ExistWorkspaceAllorsUnique);

                        Assert.Null(c1.WorkspaceAllorsBinary);
                        Assert.Null(c1.WorkspaceAllorsBoolean);
                        Assert.Null(c1.WorkspaceAllorsDateTime);
                        Assert.Null(c1.WorkspaceAllorsDecimal);
                        Assert.Null(c1.WorkspaceAllorsDouble);
                        Assert.Null(c1.WorkspaceAllorsInteger);
                        Assert.Null(c1.WorkspaceAllorsString);
                        Assert.Null(c1.WorkspaceAllorsUnique);
                    }
                }
            }
        }

        [Fact]
        public async void RemoveRole()
        {
            foreach (var push in this.pushes)
            {
                foreach (DatabaseMode mode in Enum.GetValues(typeof(DatabaseMode)))
                {
                    foreach (var contextFactory in this.contextFactories)
                    {
                        var ctx = contextFactory();
                        var (session1, _) = ctx;

                        var c1 = await ctx.Create<C1>(session1, mode);

                        Assert.NotNull(c1);

                        await push(session1);

                        if (!c1.CanWriteC1C1One2One)
                        {
                            await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1 });
                        }

                        c1.RemoveWorkspaceAllorsBinary();
                        c1.RemoveWorkspaceAllorsBoolean();
                        c1.RemoveWorkspaceAllorsDateTime();
                        c1.RemoveWorkspaceAllorsDecimal();
                        c1.RemoveWorkspaceAllorsDouble();
                        c1.RemoveWorkspaceAllorsInteger();
                        c1.RemoveWorkspaceAllorsString();
                        c1.RemoveWorkspaceAllorsUnique();

                        Assert.False(c1.ExistWorkspaceAllorsBinary);
                        Assert.False(c1.ExistWorkspaceAllorsBoolean);
                        Assert.False(c1.ExistWorkspaceAllorsDateTime);
                        Assert.False(c1.ExistWorkspaceAllorsDecimal);
                        Assert.False(c1.ExistWorkspaceAllorsDouble);
                        Assert.False(c1.ExistWorkspaceAllorsInteger);
                        Assert.False(c1.ExistWorkspaceAllorsString);
                        Assert.False(c1.ExistWorkspaceAllorsUnique);

                        Assert.Null(c1.WorkspaceAllorsBinary);
                        Assert.Null(c1.WorkspaceAllorsBoolean);
                        Assert.Null(c1.WorkspaceAllorsDateTime);
                        Assert.Null(c1.WorkspaceAllorsDecimal);
                        Assert.Null(c1.WorkspaceAllorsDouble);
                        Assert.Null(c1.WorkspaceAllorsInteger);
                        Assert.Null(c1.WorkspaceAllorsString);
                        Assert.Null(c1.WorkspaceAllorsUnique);

                        if (c1.Strategy.Id > 0)
                        {
                            await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1 });
                        }

                        Assert.False(c1.ExistWorkspaceAllorsBinary);
                        Assert.False(c1.ExistWorkspaceAllorsBoolean);
                        Assert.False(c1.ExistWorkspaceAllorsDateTime);
                        Assert.False(c1.ExistWorkspaceAllorsDecimal);
                        Assert.False(c1.ExistWorkspaceAllorsDouble);
                        Assert.False(c1.ExistWorkspaceAllorsInteger);
                        Assert.False(c1.ExistWorkspaceAllorsString);
                        Assert.False(c1.ExistWorkspaceAllorsUnique);

                        Assert.Null(c1.WorkspaceAllorsBinary);
                        Assert.Null(c1.WorkspaceAllorsBoolean);
                        Assert.Null(c1.WorkspaceAllorsDateTime);
                        Assert.Null(c1.WorkspaceAllorsDecimal);
                        Assert.Null(c1.WorkspaceAllorsDouble);
                        Assert.Null(c1.WorkspaceAllorsInteger);
                        Assert.Null(c1.WorkspaceAllorsString);
                        Assert.Null(c1.WorkspaceAllorsUnique);

                        await push(session1);

                        Assert.False(c1.ExistWorkspaceAllorsBinary);
                        Assert.False(c1.ExistWorkspaceAllorsBoolean);
                        Assert.False(c1.ExistWorkspaceAllorsDateTime);
                        Assert.False(c1.ExistWorkspaceAllorsDecimal);
                        Assert.False(c1.ExistWorkspaceAllorsDouble);
                        Assert.False(c1.ExistWorkspaceAllorsInteger);
                        Assert.False(c1.ExistWorkspaceAllorsString);
                        Assert.False(c1.ExistWorkspaceAllorsUnique);

                        Assert.Null(c1.WorkspaceAllorsBinary);
                        Assert.Null(c1.WorkspaceAllorsBoolean);
                        Assert.Null(c1.WorkspaceAllorsDateTime);
                        Assert.Null(c1.WorkspaceAllorsDecimal);
                        Assert.Null(c1.WorkspaceAllorsDouble);
                        Assert.Null(c1.WorkspaceAllorsInteger);
                        Assert.Null(c1.WorkspaceAllorsString);
                        Assert.Null(c1.WorkspaceAllorsUnique);

                        await push(session1);

                        if (!c1.CanWriteC1C1One2One)
                        {
                            await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1 });
                        }

                        c1.WorkspaceAllorsBinary = new byte[] { 1, 2 };
                        c1.WorkspaceAllorsBoolean = true;
                        c1.WorkspaceAllorsDateTime = new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc);
                        c1.WorkspaceAllorsDecimal = 10.10m;
                        c1.WorkspaceAllorsDouble = 11.11d;
                        c1.WorkspaceAllorsInteger = 12;
                        c1.WorkspaceAllorsString = "a string";
                        c1.WorkspaceAllorsUnique = new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66");

                        await push(session1);

                        if (!c1.CanWriteC1C1One2One)
                        {
                            await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1 });
                        }

                        c1.RemoveWorkspaceAllorsBinary();
                        c1.RemoveWorkspaceAllorsBoolean();
                        c1.RemoveWorkspaceAllorsDateTime();
                        c1.RemoveWorkspaceAllorsDecimal();
                        c1.RemoveWorkspaceAllorsDouble();
                        c1.RemoveWorkspaceAllorsInteger();
                        c1.RemoveWorkspaceAllorsString();
                        c1.RemoveWorkspaceAllorsUnique();

                        Assert.False(c1.ExistWorkspaceAllorsBinary);
                        Assert.False(c1.ExistWorkspaceAllorsBoolean);
                        Assert.False(c1.ExistWorkspaceAllorsDateTime);
                        Assert.False(c1.ExistWorkspaceAllorsDecimal);
                        Assert.False(c1.ExistWorkspaceAllorsDouble);
                        Assert.False(c1.ExistWorkspaceAllorsInteger);
                        Assert.False(c1.ExistWorkspaceAllorsString);
                        Assert.False(c1.ExistWorkspaceAllorsUnique);

                        Assert.Null(c1.WorkspaceAllorsBinary);
                        Assert.Null(c1.WorkspaceAllorsBoolean);
                        Assert.Null(c1.WorkspaceAllorsDateTime);
                        Assert.Null(c1.WorkspaceAllorsDecimal);
                        Assert.Null(c1.WorkspaceAllorsDouble);
                        Assert.Null(c1.WorkspaceAllorsInteger);
                        Assert.Null(c1.WorkspaceAllorsString);
                        Assert.Null(c1.WorkspaceAllorsUnique);

                        if (c1.Strategy.Id > 0)
                        {
                            await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1 });
                        }

                        Assert.False(c1.ExistWorkspaceAllorsBinary);
                        Assert.False(c1.ExistWorkspaceAllorsBoolean);
                        Assert.False(c1.ExistWorkspaceAllorsDateTime);
                        Assert.False(c1.ExistWorkspaceAllorsDecimal);
                        Assert.False(c1.ExistWorkspaceAllorsDouble);
                        Assert.False(c1.ExistWorkspaceAllorsInteger);
                        Assert.False(c1.ExistWorkspaceAllorsString);
                        Assert.False(c1.ExistWorkspaceAllorsUnique);

                        Assert.Null(c1.WorkspaceAllorsBinary);
                        Assert.Null(c1.WorkspaceAllorsBoolean);
                        Assert.Null(c1.WorkspaceAllorsDateTime);
                        Assert.Null(c1.WorkspaceAllorsDecimal);
                        Assert.Null(c1.WorkspaceAllorsDouble);
                        Assert.Null(c1.WorkspaceAllorsInteger);
                        Assert.Null(c1.WorkspaceAllorsString);
                        Assert.Null(c1.WorkspaceAllorsUnique);

                        await push(session1);

                        Assert.False(c1.ExistWorkspaceAllorsBinary);
                        Assert.False(c1.ExistWorkspaceAllorsBoolean);
                        Assert.False(c1.ExistWorkspaceAllorsDateTime);
                        Assert.False(c1.ExistWorkspaceAllorsDecimal);
                        Assert.False(c1.ExistWorkspaceAllorsDouble);
                        Assert.False(c1.ExistWorkspaceAllorsInteger);
                        Assert.False(c1.ExistWorkspaceAllorsString);
                        Assert.False(c1.ExistWorkspaceAllorsUnique);

                        Assert.Null(c1.WorkspaceAllorsBinary);
                        Assert.Null(c1.WorkspaceAllorsBoolean);
                        Assert.Null(c1.WorkspaceAllorsDateTime);
                        Assert.Null(c1.WorkspaceAllorsDecimal);
                        Assert.Null(c1.WorkspaceAllorsDouble);
                        Assert.Null(c1.WorkspaceAllorsInteger);
                        Assert.Null(c1.WorkspaceAllorsString);
                        Assert.Null(c1.WorkspaceAllorsUnique);
                    }
                }
            }
        }
    }
}

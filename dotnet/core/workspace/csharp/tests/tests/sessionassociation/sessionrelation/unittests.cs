// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.SessionAssociation.SessionRelation
{
    using System;
    using System.Threading.Tasks;
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class UnitTests : Test
    {
        private Func<Context>[] contextFactories;
        private Action<ISession>[] workspacePushes;

        protected UnitTests(Fixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");

            var singleSessionContext = new SingleSessionContext(this, "Single shared");
            var multipleSessionContext = new MultipleSessionContext(this, "Multiple shared");

            this.workspacePushes = new Action<ISession>[]
            {
                (_) => { },
                (session) => session.PushToWorkspace(),
                (session) => session.PullFromWorkspace(),
                (session) =>
                {
                    session.PushToWorkspace();
                    session.PullFromWorkspace();
                },
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
            foreach (var contextFactory in this.contextFactories)
            {
                foreach (WorkspaceMode mode in Enum.GetValues(typeof(WorkspaceMode)))
                {
                    foreach (var push in this.workspacePushes)
                    {
                        var ctx = contextFactory();
                        var (session1, _) = ctx;

                        var c1 = ctx.Create<SC1>(session1, mode);

                        Assert.NotNull(c1);

                        push(session1);

                        c1.SessionAllorsBinary = new byte[] { 1, 2 };
                        c1.SessionAllorsBoolean = true;
                        c1.SessionAllorsDateTime = new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc);
                        c1.SessionAllorsDecimal = 10.10m;
                        c1.SessionAllorsDouble = 11.11d;
                        c1.SessionAllorsInteger = 12;
                        c1.SessionAllorsString = "a string";
                        c1.SessionAllorsUnique = new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66");

                        Assert.Equal(new byte[] { 1, 2 }, c1.SessionAllorsBinary);
                        Assert.True(c1.SessionAllorsBoolean);
                        Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1.SessionAllorsDateTime);
                        Assert.Equal(10.10m, c1.SessionAllorsDecimal);
                        Assert.Equal(11.11d, c1.SessionAllorsDouble);
                        Assert.Equal(12, c1.SessionAllorsInteger);
                        Assert.Equal("a string", c1.SessionAllorsString);
                        Assert.Equal(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"), c1.SessionAllorsUnique);

                        push(session1);

                        Assert.Equal(new byte[] { 1, 2 }, c1.SessionAllorsBinary);
                        Assert.True(c1.SessionAllorsBoolean);
                        Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1.SessionAllorsDateTime);
                        Assert.Equal(10.10m, c1.SessionAllorsDecimal);
                        Assert.Equal(11.11d, c1.SessionAllorsDouble);
                        Assert.Equal(12, c1.SessionAllorsInteger);
                        Assert.Equal("a string", c1.SessionAllorsString);
                        Assert.Equal(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"), c1.SessionAllorsUnique);

                        Assert.Equal(new byte[] { 1, 2 }, c1.SessionAllorsBinary);
                        Assert.True(c1.SessionAllorsBoolean);
                        Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1.SessionAllorsDateTime);
                        Assert.Equal(10.10m, c1.SessionAllorsDecimal);
                        Assert.Equal(11.11d, c1.SessionAllorsDouble);
                        Assert.Equal(12, c1.SessionAllorsInteger);
                        Assert.Equal("a string", c1.SessionAllorsString);
                        Assert.Equal(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"), c1.SessionAllorsUnique);

                        push(session1);

                        Assert.Equal(new byte[] { 1, 2 }, c1.SessionAllorsBinary);
                        Assert.True(c1.SessionAllorsBoolean);
                        Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1.SessionAllorsDateTime);
                        Assert.Equal(10.10m, c1.SessionAllorsDecimal);
                        Assert.Equal(11.11d, c1.SessionAllorsDouble);
                        Assert.Equal(12, c1.SessionAllorsInteger);
                        Assert.Equal("a string", c1.SessionAllorsString);
                        Assert.Equal(new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66"), c1.SessionAllorsUnique);
                    }
                }
            }
        }

        [Fact]
        public async void SetRoleNull()
        {
            foreach (var push in this.workspacePushes)
            {
                foreach (WorkspaceMode mode in Enum.GetValues(typeof(WorkspaceMode)))
                {
                    foreach (var contextFactory in this.contextFactories)
                    {
                        var ctx = contextFactory();
                        var (session1, _) = ctx;

                        var c1 = ctx.Create<SC1>(session1, mode);

                        Assert.NotNull(c1);

                        push(session1);

                        c1.SessionAllorsBinary = null;
                        c1.SessionAllorsBoolean = null;
                        c1.SessionAllorsDateTime = null;
                        c1.SessionAllorsDecimal = null;
                        c1.SessionAllorsDouble = null;
                        c1.SessionAllorsInteger = null;
                        c1.SessionAllorsString = null;
                        c1.SessionAllorsUnique = null;

                        Assert.False(c1.ExistSessionAllorsBinary);
                        Assert.False(c1.ExistSessionAllorsBoolean);
                        Assert.False(c1.ExistSessionAllorsDateTime);
                        Assert.False(c1.ExistSessionAllorsDecimal);
                        Assert.False(c1.ExistSessionAllorsDouble);
                        Assert.False(c1.ExistSessionAllorsInteger);
                        Assert.False(c1.ExistSessionAllorsString);
                        Assert.False(c1.ExistSessionAllorsUnique);

                        Assert.Null(c1.SessionAllorsBinary);
                        Assert.Null(c1.SessionAllorsBoolean);
                        Assert.Null(c1.SessionAllorsDateTime);
                        Assert.Null(c1.SessionAllorsDecimal);
                        Assert.Null(c1.SessionAllorsDouble);
                        Assert.Null(c1.SessionAllorsInteger);
                        Assert.Null(c1.SessionAllorsString);
                        Assert.Null(c1.SessionAllorsUnique);

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

                        Assert.Null(c1.SessionAllorsBinary);
                        Assert.Null(c1.SessionAllorsBoolean);
                        Assert.Null(c1.SessionAllorsDateTime);
                        Assert.Null(c1.SessionAllorsDecimal);
                        Assert.Null(c1.SessionAllorsDouble);
                        Assert.Null(c1.SessionAllorsInteger);
                        Assert.Null(c1.SessionAllorsString);
                        Assert.Null(c1.SessionAllorsUnique);

                        push(session1);

                        Assert.False(c1.ExistSessionAllorsBinary);
                        Assert.False(c1.ExistSessionAllorsBoolean);
                        Assert.False(c1.ExistSessionAllorsDateTime);
                        Assert.False(c1.ExistSessionAllorsDecimal);
                        Assert.False(c1.ExistSessionAllorsDouble);
                        Assert.False(c1.ExistSessionAllorsInteger);
                        Assert.False(c1.ExistSessionAllorsString);
                        Assert.False(c1.ExistSessionAllorsUnique);

                        Assert.Null(c1.SessionAllorsBinary);
                        Assert.Null(c1.SessionAllorsBoolean);
                        Assert.Null(c1.SessionAllorsDateTime);
                        Assert.Null(c1.SessionAllorsDecimal);
                        Assert.Null(c1.SessionAllorsDouble);
                        Assert.Null(c1.SessionAllorsInteger);
                        Assert.Null(c1.SessionAllorsString);
                        Assert.Null(c1.SessionAllorsUnique);

                        push(session1);

                        c1.SessionAllorsBinary = new byte[] { 1, 2 };
                        c1.SessionAllorsBoolean = true;
                        c1.SessionAllorsDateTime = new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc);
                        c1.SessionAllorsDecimal = 10.10m;
                        c1.SessionAllorsDouble = 11.11d;
                        c1.SessionAllorsInteger = 12;
                        c1.SessionAllorsString = "a string";
                        c1.SessionAllorsUnique = new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66");

                        push(session1);

                        c1.SessionAllorsBinary = null;
                        c1.SessionAllorsBoolean = null;
                        c1.SessionAllorsDateTime = null;
                        c1.SessionAllorsDecimal = null;
                        c1.SessionAllorsDouble = null;
                        c1.SessionAllorsInteger = null;
                        c1.SessionAllorsString = null;
                        c1.SessionAllorsUnique = null;

                        Assert.False(c1.ExistSessionAllorsBinary);
                        Assert.False(c1.ExistSessionAllorsBoolean);
                        Assert.False(c1.ExistSessionAllorsDateTime);
                        Assert.False(c1.ExistSessionAllorsDecimal);
                        Assert.False(c1.ExistSessionAllorsDouble);
                        Assert.False(c1.ExistSessionAllorsInteger);
                        Assert.False(c1.ExistSessionAllorsString);
                        Assert.False(c1.ExistSessionAllorsUnique);

                        Assert.Null(c1.SessionAllorsBinary);
                        Assert.Null(c1.SessionAllorsBoolean);
                        Assert.Null(c1.SessionAllorsDateTime);
                        Assert.Null(c1.SessionAllorsDecimal);
                        Assert.Null(c1.SessionAllorsDouble);
                        Assert.Null(c1.SessionAllorsInteger);
                        Assert.Null(c1.SessionAllorsString);
                        Assert.Null(c1.SessionAllorsUnique);

                        Assert.False(c1.ExistSessionAllorsBinary);
                        Assert.False(c1.ExistSessionAllorsBoolean);
                        Assert.False(c1.ExistSessionAllorsDateTime);
                        Assert.False(c1.ExistSessionAllorsDecimal);
                        Assert.False(c1.ExistSessionAllorsDouble);
                        Assert.False(c1.ExistSessionAllorsInteger);
                        Assert.False(c1.ExistSessionAllorsString);
                        Assert.False(c1.ExistSessionAllorsUnique);

                        Assert.Null(c1.SessionAllorsBinary);
                        Assert.Null(c1.SessionAllorsBoolean);
                        Assert.Null(c1.SessionAllorsDateTime);
                        Assert.Null(c1.SessionAllorsDecimal);
                        Assert.Null(c1.SessionAllorsDouble);
                        Assert.Null(c1.SessionAllorsInteger);
                        Assert.Null(c1.SessionAllorsString);
                        Assert.Null(c1.SessionAllorsUnique);

                        push(session1);

                        Assert.False(c1.ExistSessionAllorsBinary);
                        Assert.False(c1.ExistSessionAllorsBoolean);
                        Assert.False(c1.ExistSessionAllorsDateTime);
                        Assert.False(c1.ExistSessionAllorsDecimal);
                        Assert.False(c1.ExistSessionAllorsDouble);
                        Assert.False(c1.ExistSessionAllorsInteger);
                        Assert.False(c1.ExistSessionAllorsString);
                        Assert.False(c1.ExistSessionAllorsUnique);

                        Assert.Null(c1.SessionAllorsBinary);
                        Assert.Null(c1.SessionAllorsBoolean);
                        Assert.Null(c1.SessionAllorsDateTime);
                        Assert.Null(c1.SessionAllorsDecimal);
                        Assert.Null(c1.SessionAllorsDouble);
                        Assert.Null(c1.SessionAllorsInteger);
                        Assert.Null(c1.SessionAllorsString);
                        Assert.Null(c1.SessionAllorsUnique);
                    }
                }
            }
        }

        [Fact]
        public async void RemoveRole()
        {
            foreach (var push in this.workspacePushes)
            {
                foreach (WorkspaceMode mode in Enum.GetValues(typeof(WorkspaceMode)))
                {
                    foreach (var contextFactory in this.contextFactories)
                    {
                        var ctx = contextFactory();
                        var (session1, _) = ctx;

                        var c1 = ctx.Create<SC1>(session1, mode);

                        Assert.NotNull(c1);

                        push(session1);

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

                        Assert.Null(c1.SessionAllorsBinary);
                        Assert.Null(c1.SessionAllorsBoolean);
                        Assert.Null(c1.SessionAllorsDateTime);
                        Assert.Null(c1.SessionAllorsDecimal);
                        Assert.Null(c1.SessionAllorsDouble);
                        Assert.Null(c1.SessionAllorsInteger);
                        Assert.Null(c1.SessionAllorsString);
                        Assert.Null(c1.SessionAllorsUnique);

                        Assert.False(c1.ExistSessionAllorsBinary);
                        Assert.False(c1.ExistSessionAllorsBoolean);
                        Assert.False(c1.ExistSessionAllorsDateTime);
                        Assert.False(c1.ExistSessionAllorsDecimal);
                        Assert.False(c1.ExistSessionAllorsDouble);
                        Assert.False(c1.ExistSessionAllorsInteger);
                        Assert.False(c1.ExistSessionAllorsString);
                        Assert.False(c1.ExistSessionAllorsUnique);

                        Assert.Null(c1.SessionAllorsBinary);
                        Assert.Null(c1.SessionAllorsBoolean);
                        Assert.Null(c1.SessionAllorsDateTime);
                        Assert.Null(c1.SessionAllorsDecimal);
                        Assert.Null(c1.SessionAllorsDouble);
                        Assert.Null(c1.SessionAllorsInteger);
                        Assert.Null(c1.SessionAllorsString);
                        Assert.Null(c1.SessionAllorsUnique);

                        push(session1);

                        Assert.False(c1.ExistSessionAllorsBinary);
                        Assert.False(c1.ExistSessionAllorsBoolean);
                        Assert.False(c1.ExistSessionAllorsDateTime);
                        Assert.False(c1.ExistSessionAllorsDecimal);
                        Assert.False(c1.ExistSessionAllorsDouble);
                        Assert.False(c1.ExistSessionAllorsInteger);
                        Assert.False(c1.ExistSessionAllorsString);
                        Assert.False(c1.ExistSessionAllorsUnique);

                        Assert.Null(c1.SessionAllorsBinary);
                        Assert.Null(c1.SessionAllorsBoolean);
                        Assert.Null(c1.SessionAllorsDateTime);
                        Assert.Null(c1.SessionAllorsDecimal);
                        Assert.Null(c1.SessionAllorsDouble);
                        Assert.Null(c1.SessionAllorsInteger);
                        Assert.Null(c1.SessionAllorsString);
                        Assert.Null(c1.SessionAllorsUnique);

                        push(session1);

                        c1.SessionAllorsBinary = new byte[] { 1, 2 };
                        c1.SessionAllorsBoolean = true;
                        c1.SessionAllorsDateTime = new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc);
                        c1.SessionAllorsDecimal = 10.10m;
                        c1.SessionAllorsDouble = 11.11d;
                        c1.SessionAllorsInteger = 12;
                        c1.SessionAllorsString = "a string";
                        c1.SessionAllorsUnique = new Guid("0208BB9B-E87B-4CED-8DEC-516E6778CD66");

                        push(session1);

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

                        Assert.Null(c1.SessionAllorsBinary);
                        Assert.Null(c1.SessionAllorsBoolean);
                        Assert.Null(c1.SessionAllorsDateTime);
                        Assert.Null(c1.SessionAllorsDecimal);
                        Assert.Null(c1.SessionAllorsDouble);
                        Assert.Null(c1.SessionAllorsInteger);
                        Assert.Null(c1.SessionAllorsString);
                        Assert.Null(c1.SessionAllorsUnique);

                        Assert.False(c1.ExistSessionAllorsBinary);
                        Assert.False(c1.ExistSessionAllorsBoolean);
                        Assert.False(c1.ExistSessionAllorsDateTime);
                        Assert.False(c1.ExistSessionAllorsDecimal);
                        Assert.False(c1.ExistSessionAllorsDouble);
                        Assert.False(c1.ExistSessionAllorsInteger);
                        Assert.False(c1.ExistSessionAllorsString);
                        Assert.False(c1.ExistSessionAllorsUnique);

                        Assert.Null(c1.SessionAllorsBinary);
                        Assert.Null(c1.SessionAllorsBoolean);
                        Assert.Null(c1.SessionAllorsDateTime);
                        Assert.Null(c1.SessionAllorsDecimal);
                        Assert.Null(c1.SessionAllorsDouble);
                        Assert.Null(c1.SessionAllorsInteger);
                        Assert.Null(c1.SessionAllorsString);
                        Assert.Null(c1.SessionAllorsUnique);

                        push(session1);

                        Assert.False(c1.ExistSessionAllorsBinary);
                        Assert.False(c1.ExistSessionAllorsBoolean);
                        Assert.False(c1.ExistSessionAllorsDateTime);
                        Assert.False(c1.ExistSessionAllorsDecimal);
                        Assert.False(c1.ExistSessionAllorsDouble);
                        Assert.False(c1.ExistSessionAllorsInteger);
                        Assert.False(c1.ExistSessionAllorsString);
                        Assert.False(c1.ExistSessionAllorsUnique);

                        Assert.Null(c1.SessionAllorsBinary);
                        Assert.Null(c1.SessionAllorsBoolean);
                        Assert.Null(c1.SessionAllorsDateTime);
                        Assert.Null(c1.SessionAllorsDecimal);
                        Assert.Null(c1.SessionAllorsDouble);
                        Assert.Null(c1.SessionAllorsInteger);
                        Assert.Null(c1.SessionAllorsString);
                        Assert.Null(c1.SessionAllorsUnique);
                    }
                }
            }
        }
    }
}

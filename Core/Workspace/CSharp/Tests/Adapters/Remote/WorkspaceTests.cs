// <copyright file="WorkspaceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Adapters.Remote
{
    using System.Linq;
    using Allors.Protocol.Database.Pull;
    using Allors.Workspace.Meta;
    using Adapters;
    using Allors.Workspace.Adapters.Remote;
    using Xunit;

    public class WorkspaceTests : Test
    {
        [Fact]
        public void Load()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));

            object Value(DatabaseObject @object, IRoleType roleType) => @object.Roles.First(v => Equals(v.RoleType, roleType)).Value;

            var martien = this.Database.Get(3);

            Assert.Equal(3, martien.Id);
            Assert.Equal(1003, martien.Version);
            Assert.Equal("Person", martien.Class.Name);
            Assert.Equal("Martien", Value(martien, this.M.Person.FirstName));
            Assert.Equal("van", Value(martien, this.M.Person.MiddleName));
            Assert.Equal("Knippenberg", Value(martien, this.M.Person.LastName));
            Assert.DoesNotContain(martien.Roles, v => Equals(v.RoleType, this.M.Person.IsStudent));
            Assert.DoesNotContain(martien.Roles, v => Equals(v.RoleType, this.M.Person.BirthDate));
        }

        [Fact]
        public void Diff()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));
            var pullResponse = new PullResponse
            {
                Objects =
                       new[]
                           {
                                new[] { "1", "1001", "101" },
                                new[] { "2", "1002", "102", "103" },
                                new[] { "3", "1003" },
                           },
            };

            var requireLoad = this.Database.Diff(pullResponse);

            Assert.Empty(requireLoad.Objects);
        }

        [Fact]
        public void DiffVersion()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));
            var pullResponse = new PullResponse
            {
                Objects =
                    new[]
                    {
                        new[] { "1", "1001", "101" },
                        new[] { "2", "1002", "102", "103" },
                        new[] { "3", "1004" },
                    },
            };

            var requireLoad = this.Database.Diff(pullResponse);

            Assert.Single(requireLoad.Objects);

            Assert.Equal("3", requireLoad.Objects[0]);
        }

        [Fact]
        public void DiffAccessControl()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));
            var pullResponse = new PullResponse
            {
                Objects =
                    new[]
                    {
                        new[] { "1", "1001", "201" },
                        new[] { "2", "1002", "102", "103" },
                        new[] { "3", "1003" },
                    },
            };

            var requireLoad = this.Database.Diff(pullResponse);

            Assert.Single(requireLoad.Objects);

            Assert.Equal("1", requireLoad.Objects[0]);
        }

        [Fact]
        public void DiffChangeDeniedPermission()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));
            var pullResponse = new PullResponse
            {
                Objects =
                    new[]
                    {
                        new[] { "1", "1001", "101" },
                        new[] { "2", "1002", "102", "104" },
                        new[] { "3", "1003" },
                    },
            };

            var requireLoad = this.Database.Diff(pullResponse);

            Assert.Single(requireLoad.Objects);

            Assert.Equal("2", requireLoad.Objects[0]);
        }

        [Fact]
        public void DiffAddDeniedPermission()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));
            var pullResponse = new PullResponse
            {
                Objects =
                    new[]
                    {
                        new[] { "1", "1001", "101", "104" },
                        new[] { "2", "1002", "102", "103" },
                        new[] { "3", "1003" },
                    },
            };

            var requireLoad = this.Database.Diff(pullResponse);

            Assert.Single(requireLoad.Objects);

            Assert.Equal("1", requireLoad.Objects[0]);
        }

        [Fact]
        public void DiffRemoveDeniedPermission()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));
            var pullResponse = new PullResponse
            {
                Objects =
                    new[]
                    {
                        //new[] { "1", "1001", "101" },
                        new[] { "2", "1002", "102" },
                        //new[] { "3", "1003" },
                    },
            };

            var requireLoad = this.Database.Diff(pullResponse);

            Assert.Single(requireLoad.Objects);

            Assert.Equal("2", requireLoad.Objects[0]);
        }
    }
}

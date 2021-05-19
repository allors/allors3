// <copyright file="WorkspaceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Remote
{
    using Allors.Protocol.Json.Api.Pull;
    using Xunit;

    public class WorkspaceTests : Test
    {
        [Fact]
        public void Load()
        {
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));

            var martien = this.Database.GetRecord(3);

            Assert.Equal(3, martien.Id);
            Assert.Equal(1003, martien.Version);
            Assert.Equal("Person", martien.Class.SingularName);
            Assert.Equal("Martien", martien.GetRole(this.M.Person.FirstName));
            Assert.Equal("van", martien.GetRole(this.M.Person.MiddleName));
            Assert.Equal("Knippenberg", martien.GetRole(this.M.Person.LastName));
            Assert.Null(martien.GetRole(this.M.Person.IsStudent));
            Assert.Null(martien.GetRole(this.M.Person.BirthDate));
        }

        [Fact]
        public void Diff()
        {
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));
            var pullResponse = new PullResponse
            {
                Pool =
                       new[]
                           {
                                new PullResponseObject { Id = 1,Version = 1001, AccessControls = new long[]{101} },
                                new PullResponseObject { Id = 2, Version = 1002, AccessControls = new long[]{102}, DeniedPermissions = new long[]{103}},
                                new PullResponseObject { Id = 3, Version = 1003 },
                           },
            };

            var requireLoad = this.Database.Diff(pullResponse);

            Assert.Empty(requireLoad.Objects);
        }

        [Fact]
        public void DiffVersion()
        {
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));
            var pullResponse = new PullResponse
            {
                Pool =
                    new[]
                    {
                        new PullResponseObject { Id = 1,Version = 1001, AccessControls = new long[]{101} },
                        new PullResponseObject { Id = 2, Version = 1002, AccessControls = new long[]{102}, DeniedPermissions = new long[]{103}},
                        new PullResponseObject { Id = 3, Version = 1004 },
                    },
            };

            var requireLoad = this.Database.Diff(pullResponse);

            _ = Assert.Single(requireLoad.Objects);

            Assert.Equal(3, requireLoad.Objects[0]);
        }

        [Fact]
        public void DiffAccessControl()
        {
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));
            var pullResponse = new PullResponse
            {
                Pool =
                    new[]
                    {
                        new PullResponseObject { Id = 1,Version = 1001, AccessControls = new long[]{201} },
                        new PullResponseObject { Id = 2, Version = 1002, AccessControls = new long[]{102}, DeniedPermissions = new long[]{103}},
                        new PullResponseObject { Id = 3, Version = 1003 },
                    },
            };

            var requireLoad = this.Database.Diff(pullResponse);

            _ = Assert.Single(requireLoad.Objects);

            Assert.Equal(1, requireLoad.Objects[0]);
        }

        [Fact]
        public void DiffChangeDeniedPermission()
        {
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));
            var pullResponse = new PullResponse
            {
                Pool =
                    new[]
                    {
                        new PullResponseObject { Id = 1,Version = 1001, AccessControls = new long[]{101} },
                        new PullResponseObject { Id = 2, Version = 1002, AccessControls = new long[]{102}, DeniedPermissions = new long[]{104}},
                        new PullResponseObject { Id = 3, Version = 1003 },
                    },
            };

            var requireLoad = this.Database.Diff(pullResponse);

            _ = Assert.Single(requireLoad.Objects);

            Assert.Equal(2, requireLoad.Objects[0]);
        }

        [Fact]
        public void DiffAddDeniedPermission()
        {
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));
            var pullResponse = new PullResponse
            {
                Pool =
                    new[]
                    {
                        new PullResponseObject { Id = 1,Version = 1001, AccessControls = new long[]{101}, DeniedPermissions = new long[]{104}},
                        new PullResponseObject { Id = 2, Version = 1002, AccessControls = new long[]{102}, DeniedPermissions = new long[]{103}},
                        new PullResponseObject { Id = 3, Version = 1003 },
                    },
            };

            var requireLoad = this.Database.Diff(pullResponse);

            Assert.Empty(requireLoad.Objects);
        }

        [Fact]
        public void DiffRemoveDeniedPermission()
        {
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));
            var pullResponse = new PullResponse
            {
                Pool =
                    new PullResponseObject[]
                    {
                        //new PullResponseObject { Id = 1,Version = 1001, AccessControls = new long[]{101} },
                        new PullResponseObject { Id = 2, Version = 1002, AccessControls = new long[]{102},},
                        //new PullResponseObject { Id = 3, Version = 1003 },
                    },
            };

            var requireLoad = this.Database.Diff(pullResponse);

            _ = Assert.Single(requireLoad.Objects);

            Assert.Equal(2, requireLoad.Objects[0]);
        }
    }
}

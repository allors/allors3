// <copyright file="Fixture.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Remote
{
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Workspace.Meta;

    public class Fixture
    {
        public static SyncResponse LoadData(M m) =>
            new SyncResponse
            {
                Objects = new[]
                {
                    new SyncResponseObject
                    {
                        Id = 1,
                        Version = 1001,
                        ObjectType = m.Person.Tag,
                        Roles = new[]
                        {
                            new SyncResponseRole {RoleType = m.Person.FirstName.RelationType.Tag, Value = "Koen"},
                            new SyncResponseRole {RoleType = m.Person.LastName.RelationType.Tag, Value = "Van Exem"},
                            new SyncResponseRole
                            {
                                RoleType = m.Person.BirthDate.RelationType.Tag, Value = "1973-03-27T18:00:00Z"
                            },
                            new SyncResponseRole {RoleType = m.Person.IsStudent.RelationType.Tag, Value = "1"},
                        },
                        AccessControls = new long[]{101},
                    },
                    new SyncResponseObject
                    {
                        Id = 2,
                        Version = 1002,
                        ObjectType = m.Person.Tag,
                        Roles = new[]
                        {
                            new SyncResponseRole {RoleType = m.Person.FirstName.RelationType.Tag, Value = "Patrick"},
                            new SyncResponseRole {RoleType = m.Person.LastName.RelationType.Tag, Value = "De Boeck"},
                            new SyncResponseRole {RoleType = m.Person.IsStudent.RelationType.Tag, Value = "0"},
                        },
                        AccessControls = new long[]{102},
                        DeniedPermissions = new long[]{103},
                    },
                    new SyncResponseObject
                    {
                        Id = 3,
                        Version = 1003,
                        ObjectType = m.Person.Tag,
                        Roles = new[]
                        {
                            new SyncResponseRole {RoleType = m.Person.FirstName.RelationType.Tag, Value = "Martien"},
                            new SyncResponseRole {RoleType = m.Person.MiddleName.RelationType.Tag, Value = "van"},
                            new SyncResponseRole {RoleType = m.Person.LastName.RelationType.Tag, Value = "Knippenberg"},
                        },
                    },
                    new SyncResponseObject
                    {
                        Id = 101,
                        Version = 1101,
                        ObjectType = m.Organisation.Tag,
                        Roles = new[]
                        {
                            new SyncResponseRole {RoleType = m.Organisation.Name.RelationType.Tag, Value = "Acme"},
                            new SyncResponseRole {RoleType = m.Organisation.Owner.RelationType.Tag, Object = 1},
                            new SyncResponseRole {RoleType = m.Organisation.Employees.RelationType.Tag, Collection = new long[]{1,2,3}},
                            new SyncResponseRole {RoleType = m.Organisation.Manager.RelationType.Tag},
                        },
                    },
                    new SyncResponseObject
                    {
                        Id = 102,
                        Version = 1102,
                        ObjectType = m.Organisation.Tag,
                        Roles = new[]
                        {
                            new SyncResponseRole {RoleType = m.Organisation.Name.RelationType.Tag, Value = "Ocme"},
                            new SyncResponseRole {RoleType = m.Organisation.Owner.RelationType.Tag, Value = "2"},
                            new SyncResponseRole {RoleType = m.Organisation.Employees.RelationType.Tag, Collection = new long[]{1}},
                        },
                    },
                    new SyncResponseObject
                    {
                        Id = 103,
                        Version = 1103,
                        ObjectType = m.Organisation.Tag,
                        Roles = new[]
                        {
                            new SyncResponseRole {RoleType = m.Organisation.Name.RelationType.Tag, Value = "icme"},
                            new SyncResponseRole {RoleType = m.Organisation.Owner.RelationType.Tag, Object = 3},
                        },
                    },
                },
            };
    }
}

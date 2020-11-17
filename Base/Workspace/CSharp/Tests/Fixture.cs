// <copyright file="Fixture.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
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
                        Id = "1",
                        Version = "1001",
                        ObjectTypeOrKey = m.Person.ObjectType.IdAsString,
                        Roles = new[]
                        {
                            new SyncResponseRole {RoleType = m.Person.FirstName.RelationType.IdAsString, Value = "Koen"},
                            new SyncResponseRole {RoleType = m.Person.LastName.RelationType.IdAsString, Value = "Van Exem"},
                            new SyncResponseRole
                            {
                                RoleType = m.Person.BirthDate.RelationType.IdAsString, Value = "1973-03-27T18:00:00Z"
                            },
                            new SyncResponseRole {RoleType = m.Person.IsStudent.RelationType.IdAsString, Value = "1"},
                        },
                        AccessControls = "101",
                    },
                    new SyncResponseObject
                    {
                        Id = "2",
                        Version = "1002",
                        ObjectTypeOrKey = m.Person.ObjectType.IdAsString,
                        Roles = new[]
                        {
                            new SyncResponseRole {RoleType = m.Person.FirstName.RelationType.IdAsString, Value = "Patrick"},
                            new SyncResponseRole {RoleType = m.Person.LastName.RelationType.IdAsString, Value = "De Boeck"},
                            new SyncResponseRole {RoleType = m.Person.IsStudent.RelationType.IdAsString, Value = "0"},
                        },
                        AccessControls = "102",
                        DeniedPermissions = "103",
                    },
                    new SyncResponseObject
                    {
                        Id = "3",
                        Version = "1003",
                        ObjectTypeOrKey = m.Person.ObjectType.IdAsString,
                        Roles = new[]
                        {
                            new SyncResponseRole {RoleType = m.Person.FirstName.RelationType.IdAsString, Value = "Martien"},
                            new SyncResponseRole {RoleType = m.Person.MiddleName.RelationType.IdAsString, Value = "van"},
                            new SyncResponseRole {RoleType = m.Person.LastName.RelationType.IdAsString, Value = "Knippenberg"},
                        },
                    },
                    new SyncResponseObject
                    {
                        Id = "101",
                        Version = "1101",
                        ObjectTypeOrKey = m.Organisation.ObjectType.IdAsString,
                        Roles = new[]
                        {
                            new SyncResponseRole {RoleType = m.Organisation.Name.RelationType.IdAsString, Value = "Acme"},
                            new SyncResponseRole {RoleType = m.Organisation.Owner.RelationType.IdAsString, Value = "1"},
                            new SyncResponseRole {RoleType = m.Organisation.Employees.RelationType.IdAsString, Value = "1|2|3"},
                            new SyncResponseRole {RoleType = m.Organisation.Manager.RelationType.IdAsString},
                        },
                    },
                    new SyncResponseObject
                    {
                        Id = "102",
                        Version = "1102",
                        ObjectTypeOrKey = m.Organisation.ObjectType.IdAsString,
                        Roles = new[]
                        {
                            new SyncResponseRole {RoleType = m.Organisation.Name.RelationType.IdAsString, Value = "Ocme"},
                            new SyncResponseRole {RoleType = m.Organisation.Owner.RelationType.IdAsString, Value = "2"},
                            new SyncResponseRole {RoleType = m.Organisation.Employees.RelationType.IdAsString, Value = "1"},
                        },
                    },
                    new SyncResponseObject
                    {
                        Id = "103",
                        Version = "1103",
                        ObjectTypeOrKey = m.Organisation.ObjectType.IdAsString,
                        Roles = new[]
                        {
                            new SyncResponseRole {RoleType = m.Organisation.Name.RelationType.IdAsString, Value = "icme"},
                            new SyncResponseRole {RoleType = m.Organisation.Owner.RelationType.IdAsString, Value = "3"},
                        },
                    },
                },
            };
    }
}

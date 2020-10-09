// <copyright file="Fixture.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Adapters
{
    using Allors.Protocol.Database.Sync;
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
                        I = "1",
                        V = "1001",
                        T = m.Person.ObjectType.IdAsString,
                        R = new[]
                        {
                            new SyncResponseRole {T = m.Person.FirstName.RelationType.IdAsString, V = "Koen"},
                            new SyncResponseRole {T = m.Person.LastName.RelationType.IdAsString, V = "Van Exem"},
                            new SyncResponseRole
                            {
                                T = m.Person.BirthDate.RelationType.IdAsString, V = "1973-03-27T18:00:00Z"
                            },
                            new SyncResponseRole {T = m.Person.IsStudent.RelationType.IdAsString, V = "1"},
                        },
                        A = "101",
                    },
                    new SyncResponseObject
                    {
                        I = "2",
                        V = "1002",
                        T = m.Person.ObjectType.IdAsString,
                        R = new[]
                        {
                            new SyncResponseRole {T = m.Person.FirstName.RelationType.IdAsString, V = "Patrick"},
                            new SyncResponseRole {T = m.Person.LastName.RelationType.IdAsString, V = "De Boeck"},
                            new SyncResponseRole {T = m.Person.IsStudent.RelationType.IdAsString, V = "0"},
                        },
                        A = "102",
                        D = "103",
                    },
                    new SyncResponseObject
                    {
                        I = "3",
                        V = "1003",
                        T = m.Person.ObjectType.IdAsString,
                        R = new[]
                        {
                            new SyncResponseRole {T = m.Person.FirstName.RelationType.IdAsString, V = "Martien"},
                            new SyncResponseRole {T = m.Person.MiddleName.RelationType.IdAsString, V = "van"},
                            new SyncResponseRole {T = m.Person.LastName.RelationType.IdAsString, V = "Knippenberg"},
                        },
                    },
                    new SyncResponseObject
                    {
                        I = "101",
                        V = "1101",
                        T = m.Organisation.ObjectType.IdAsString,
                        R = new[]
                        {
                            new SyncResponseRole {T = m.Organisation.Name.RelationType.IdAsString, V = "Acme"},
                            new SyncResponseRole {T = m.Organisation.Owner.RelationType.IdAsString, V = "1"},
                            new SyncResponseRole {T = m.Organisation.Employees.RelationType.IdAsString, V = "1|2|3"},
                            new SyncResponseRole {T = m.Organisation.Manager.RelationType.IdAsString},
                        },
                    },
                    new SyncResponseObject
                    {
                        I = "102",
                        V = "1102",
                        T = m.Organisation.ObjectType.IdAsString,
                        R = new[]
                        {
                            new SyncResponseRole {T = m.Organisation.Name.RelationType.IdAsString, V = "Ocme"},
                            new SyncResponseRole {T = m.Organisation.Owner.RelationType.IdAsString, V = "2"},
                            new SyncResponseRole {T = m.Organisation.Employees.RelationType.IdAsString, V = "1"},
                        },
                    },
                    new SyncResponseObject
                    {
                        I = "103",
                        V = "1103",
                        T = m.Organisation.ObjectType.IdAsString,
                        R = new[]
                        {
                            new SyncResponseRole {T = m.Organisation.Name.RelationType.IdAsString, V = "icme"},
                            new SyncResponseRole {T = m.Organisation.Owner.RelationType.IdAsString, V = "3"},
                        },
                    },
                },
            };
    }
}

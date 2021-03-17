// <copyright file="SessionTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Remote
{
    using System.Linq;
    using Allors;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Workspace.Adapters.Remote;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public class SessionTests : Test
    {
        [Fact]
        public async void UnitGet()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));
            var session = this.CreateSession();

            var koen = session.InstantiateDatabaseObject(1 ).Object as Person;

            Assert.Equal("Koen", koen.FirstName);
            Assert.Null(koen.MiddleName);
            Assert.Equal("Van Exem", koen.LastName);
            Assert.Equal(UnitConvert.Parse(this.M.Person.BirthDate.ObjectType.Id, "1973-03-27T18:00:00Z"), koen.BirthDate);
            Assert.True(koen.IsStudent);

            var patrick = session.InstantiateDatabaseObject(2).Object as Person;

            Assert.Equal("Patrick", patrick.FirstName);
            Assert.Equal("De Boeck", patrick.LastName);
            Assert.Null(patrick.MiddleName);
            Assert.Null(patrick.BirthDate);
            Assert.False(patrick.IsStudent);

            var martien = session.InstantiateDatabaseObject(3).Object as Person;

            Assert.Equal("Martien", martien.FirstName);
            Assert.Equal("Knippenberg", martien.LastName);
            Assert.Equal("van", martien.MiddleName);
            Assert.Null(martien.BirthDate);
            Assert.Null(martien.IsStudent);
        }

        [Fact]
        public void UnitSet()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));

            var session1 = this.CreateSession();
            var martien1 = session1.InstantiateDatabaseObject(3).Object as Person;

            var session2 = this.CreateSession();
            var martien2 = session2.InstantiateDatabaseObject(3).Object as Person;

            martien2.FirstName = "Martinus";
            martien2.MiddleName = "X";

            Assert.Equal("Martien", martien1.FirstName);
            Assert.Equal("Knippenberg", martien1.LastName);
            Assert.Equal("van", martien1.MiddleName);

            Assert.Equal("Martinus", martien2.FirstName);
            Assert.Equal("Knippenberg", martien2.LastName);
            Assert.Equal("X", martien2.MiddleName);
        }

        [Fact]
        public void HasChanges()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));

            var session = this.CreateSession();
            var martien = session.InstantiateDatabaseObject(3).Object as Person;
            var acme = session.InstantiateDatabaseObject(101).Object as Organisation;

            Assert.False(session.HasDatabaseChanges);

            var firstName = martien.FirstName;
            martien.FirstName = firstName;

            Assert.False(session.HasDatabaseChanges);

            martien.UserName = null;

            Assert.False(session.HasDatabaseChanges);

            var owner = acme.Owner;
            acme.Owner = owner;

            Assert.False(session.HasDatabaseChanges);

            acme.CycleOne = null;

            Assert.False(session.HasDatabaseChanges);

            var employees = acme.Employees;
            acme.Employees = employees;

            Assert.False(session.HasDatabaseChanges);

            employees = employees.Reverse().ToArray();
            acme.Employees = employees;

            Assert.False(session.HasDatabaseChanges);

            acme.CycleMany = null;

            Assert.False(session.HasDatabaseChanges);
        }

        [Fact]
        public void UnitSave()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));
            var session = this.CreateSession();

            var koen = session.InstantiateDatabaseObject(1).Object as Person;
            var patrick = session.InstantiateDatabaseObject(2).Object as Person;
            var martien = session.InstantiateDatabaseObject(3).Object as Person;

            koen.FirstName = "K";
            koen.LastName = "VE";
            martien.FirstName = "Martinus";
            martien.MiddleName = "X";

            var save = session.PushRequest();

            Assert.Equal(2, save.Objects.Length);

            var savedKoen = save.Objects.First(v => v.DatabaseId == "1");

            Assert.Equal("1001", savedKoen.Version);
            Assert.Equal(2, savedKoen.Roles.Length);

            var savedKoenFirstName = savedKoen.Roles.First(v => v.RelationType == this.M.Person.FirstName.RelationType.IdAsString);
            var savedKoenLastName = savedKoen.Roles.First(v => v.RelationType == this.M.Person.LastName.RelationType.IdAsString);

            Assert.Equal("K", savedKoenFirstName.SetRole);
            Assert.Null(savedKoenFirstName.AddRole);
            Assert.Null(savedKoenFirstName.RemoveRole);
            Assert.Equal("VE", savedKoenLastName.SetRole);
            Assert.Null(savedKoenLastName.AddRole);
            Assert.Null(savedKoenLastName.RemoveRole);

            var savedMartien = save.Objects.First(v => v.DatabaseId == "3");

            Assert.Equal("1003", savedMartien.Version);
            Assert.Equal(2, savedMartien.Roles.Length);

            var savedMartienFirstName = savedMartien.Roles.First(v => v.RelationType == this.M.Person.FirstName.RelationType.IdAsString);
            var savedMartienMiddleName = savedMartien.Roles.First(v => v.RelationType == this.M.Person.MiddleName.RelationType.IdAsString);

            Assert.Equal("Martinus", savedMartienFirstName.SetRole);
            Assert.Null(savedMartienFirstName.AddRole);
            Assert.Null(savedMartienFirstName.RemoveRole);
            Assert.Equal("X", savedMartienMiddleName.SetRole);
            Assert.Null(savedMartienMiddleName.AddRole);
            Assert.Null(savedMartienMiddleName.RemoveRole);
        }

        [Fact]
        public void OneGet()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));
            var session = this.CreateSession();

            var koen = session.InstantiateDatabaseObject(1).Object as Person;
            var patrick = session.InstantiateDatabaseObject(2).Object as Person;
            var martien = session.InstantiateDatabaseObject(3).Object as Person;

            var acme = session.InstantiateDatabaseObject(101).Object as Organisation;
            var ocme = session.InstantiateDatabaseObject(102).Object as Organisation;
            var icme = session.InstantiateDatabaseObject(103).Object as Organisation;

            Assert.Equal(koen, acme.Owner);
            Assert.Equal(patrick, ocme.Owner);
            Assert.Equal(martien, icme.Owner);

            Assert.Null(acme.Manager);
            Assert.Null(ocme.Manager);
            Assert.Null(icme.Manager);
        }

        [Fact]
        public void OneSet()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));

            var session1 = this.CreateSession();

            var session2 = this.CreateSession();
            
            var koen1 = session1.InstantiateDatabaseObject(1).Object as Person;
            var patrick1 = session1.InstantiateDatabaseObject(2).Object as Person;
            var martien1 = session1.InstantiateDatabaseObject(3).Object as Person;

            var acme1 = session1.InstantiateDatabaseObject(101).Object as Organisation;
            var ocme1 = session1.InstantiateDatabaseObject(102).Object as Organisation;
            var icme1 = session1.InstantiateDatabaseObject(103).Object as Organisation;

            var koen2 = session2.InstantiateDatabaseObject(1).Object as Person;
            var patrick2 = session2.InstantiateDatabaseObject(2).Object as Person;
            var martien2 = session2.InstantiateDatabaseObject(3).Object as Person;

            var acme2 = session2.InstantiateDatabaseObject(101).Object as Organisation;
            var ocme2 = session2.InstantiateDatabaseObject(102).Object as Organisation;
            var icme2 = session2.InstantiateDatabaseObject(103).Object as Organisation;

            acme2.Owner = martien2;
            ocme2.Owner = null;
            acme2.Manager = patrick2;

            Assert.Equal(koen1, acme1.Owner);
            Assert.Equal(patrick1, ocme1.Owner);
            Assert.Equal(martien1, icme1.Owner);

            Assert.Null(acme1.Manager);
            Assert.Null(ocme1.Manager);
            Assert.Null(icme1.Manager);

            Assert.Equal(martien2, acme2.Owner); // x
            Assert.Null(ocme2.Owner);
            Assert.Equal(martien2, icme2.Owner);

            Assert.Equal(patrick2, acme2.Manager); // x
            Assert.Null(ocme2.Manager);
            Assert.Null(icme2.Manager);
        }

        [Fact]
        public void OneSave()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));
            var session = this.CreateSession();

            var koen = session.InstantiateDatabaseObject(1).Object as Person;
            var patrick = session.InstantiateDatabaseObject(2).Object as Person;
            var martien = session.InstantiateDatabaseObject(3).Object as Person;

            var acme = session.InstantiateDatabaseObject(101).Object as Organisation;
            var ocme = session.InstantiateDatabaseObject(102).Object as Organisation;
            var icme = session.InstantiateDatabaseObject(103).Object as Organisation;

            acme.Owner = martien;
            ocme.Owner = null;

            acme.Manager = patrick;

            var save = session.PushRequest();

            Assert.Equal(2, save.Objects.Length);

            var savedAcme = save.Objects.First(v => v.DatabaseId == "101");

            Assert.Equal("1101", savedAcme.Version);
            Assert.Equal(2, savedAcme.Roles.Length);

            var savedAcmeOwner = savedAcme.Roles.First(v => v.RelationType == this.M.Organisation.Owner.RelationType.IdAsString);
            var savedAcmeManager = savedAcme.Roles.First(v => v.RelationType == this.M.Organisation.Manager.RelationType.IdAsString);

            Assert.Equal("3", savedAcmeOwner.SetRole);
            Assert.Null(savedAcmeOwner.AddRole);
            Assert.Null(savedAcmeOwner.RemoveRole);
            Assert.Equal("2", savedAcmeManager.SetRole);
            Assert.Null(savedAcmeManager.AddRole);
            Assert.Null(savedAcmeManager.RemoveRole);

            var savedOcme = save.Objects.First(v => v.DatabaseId == "102");

            Assert.Equal("1102", savedOcme.Version);
            Assert.Single(savedOcme.Roles);

            var savedOcmeOwner = savedOcme.Roles.First(v => v.RelationType == this.M.Organisation.Owner.RelationType.IdAsString);

            Assert.Null(savedOcmeOwner.SetRole);
            Assert.Null(savedOcmeOwner.AddRole);
            Assert.Null(savedOcmeOwner.RemoveRole);
        }

        [Fact]
        public void ManyGet()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));
            var session = this.CreateSession();

            var koen = session.InstantiateDatabaseObject(1).Object as Person;
            var patrick = session.InstantiateDatabaseObject(2).Object as Person;
            var martien = session.InstantiateDatabaseObject(3).Object as Person;

            var acme = session.InstantiateDatabaseObject(101).Object as Organisation;
            var ocme = session.InstantiateDatabaseObject(102).Object as Organisation;
            var icme = session.InstantiateDatabaseObject(103).Object as Organisation;

            Assert.Equal(3, acme.Employees.Count());
            Assert.Contains(koen, acme.Employees);
            Assert.Contains(martien, acme.Employees);
            Assert.Contains(patrick, acme.Employees);

            Assert.Single(ocme.Employees);
            Assert.Contains(koen, ocme.Employees);

            Assert.Empty(icme.Employees);

            Assert.Empty(acme.Shareholders);
            Assert.Empty(ocme.Shareholders);
            Assert.Empty(icme.Shareholders);
        }

        [Fact]
        public void ManySet()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));

            var session1 = this.CreateSession();

            var session2 = this.CreateSession();

            var koen1 = session1.InstantiateDatabaseObject(1).Object as Person;
            var patrick1 = session1.InstantiateDatabaseObject(2).Object as Person;
            var martien1 = session1.InstantiateDatabaseObject(3).Object as Person;

            var acme1 = session1.InstantiateDatabaseObject(101).Object as Organisation;
            var ocme1 = session1.InstantiateDatabaseObject(102).Object as Organisation;
            var icme1 = session1.InstantiateDatabaseObject(103).Object as Organisation;

            var koen2 = session2.InstantiateDatabaseObject(1).Object as Person;
            var patrick2 = session2.InstantiateDatabaseObject(2).Object as Person;
            var martien2 = session2.InstantiateDatabaseObject(3).Object as Person;

            var acme2 = session2.InstantiateDatabaseObject(101).Object as Organisation;
            var ocme2 = session2.InstantiateDatabaseObject(102).Object as Organisation;
            var icme2 = session2.InstantiateDatabaseObject(103).Object as Organisation;

            acme2.Employees = null;
            icme2.Employees = new[] { koen2, patrick2, martien2 };

            Assert.Equal(3, acme1.Employees.Count());
            Assert.Contains(koen1, acme1.Employees);
            Assert.Contains(martien1, acme1.Employees);
            Assert.Contains(patrick1, acme1.Employees);

            Assert.Single(ocme1.Employees);
            Assert.Contains(koen1, ocme1.Employees);

            Assert.Empty(icme1.Employees);

            Assert.Empty(acme2.Employees);

            Assert.Single(ocme2.Employees);
            Assert.Contains(koen2, ocme2.Employees);

            Assert.Equal(3, icme2.Employees.Count());
            Assert.Contains(koen2, icme2.Employees);
            Assert.Contains(martien2, icme2.Employees);
            Assert.Contains(patrick2, icme2.Employees);
        }

        [Fact]
        public void ManySaveWithExistingObjects()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));

            var session = this.CreateSession();

            var koen = session.InstantiateDatabaseObject(1).Object as Person;
            var patrick = session.InstantiateDatabaseObject(2).Object as Person;
            var martien = session.InstantiateDatabaseObject(3).Object as Person;

            var acme = session.InstantiateDatabaseObject(101).Object as Organisation;
            var ocme = session.InstantiateDatabaseObject(102).Object as Organisation;
            var icme = session.InstantiateDatabaseObject(103).Object as Organisation;

            acme.Employees = null;
            ocme.Employees = new[] { martien, patrick };
            icme.Employees = new[] { koen, patrick, martien };

            var save = session.PushRequest();

            Assert.Null(save.NewObjects);
            Assert.Equal(3, save.Objects.Length);

            var savedAcme = save.Objects.First(v => v.DatabaseId == "101");

            Assert.Equal("1101", savedAcme.Version);
            Assert.Single(savedAcme.Roles);

            var savedAcmeEmployees = savedAcme.Roles.First(v => v.RelationType == this.M.Organisation.Employees.RelationType.IdAsString);

            Assert.Null(savedAcmeEmployees.SetRole);
            Assert.Empty(savedAcmeEmployees.AddRole);
            Assert.Contains("1", savedAcmeEmployees.RemoveRole);
            Assert.Contains("2", savedAcmeEmployees.RemoveRole);
            Assert.Contains("3", savedAcmeEmployees.RemoveRole);

            var savedOcme = save.Objects.First(v => v.DatabaseId == "102");

            Assert.Equal("1102", savedOcme.Version);
            Assert.Single(savedOcme.Roles);

            var savedOcmeEmployees = savedOcme.Roles.First(v => v.RelationType == this.M.Organisation.Employees.RelationType.IdAsString);

            Assert.Null(savedOcmeEmployees.SetRole);
            Assert.Equal(2, savedOcmeEmployees.AddRole.Length);
            Assert.Contains("2", savedOcmeEmployees.AddRole);
            Assert.Contains("3", savedOcmeEmployees.AddRole);

            Assert.Single(savedOcmeEmployees.RemoveRole);
            Assert.Contains("1", savedOcmeEmployees.RemoveRole);

            var savedIcme = save.Objects.First(v => v.DatabaseId == "103");

            Assert.Equal("1103", savedIcme.Version);
            Assert.Single(savedIcme.Roles);

            var savedIcmeEmployees = savedIcme.Roles.First(v => v.RelationType == this.M.Organisation.Employees.RelationType.IdAsString);

            Assert.Null(savedIcmeEmployees.SetRole);
            Assert.Equal(3, savedIcmeEmployees.AddRole.Length);
            Assert.Contains("1", savedIcmeEmployees.AddRole);
            Assert.Contains("2", savedIcmeEmployees.AddRole);
            Assert.Contains("3", savedIcmeEmployees.AddRole);
            Assert.Null(savedIcmeEmployees.RemoveRole);
        }

        [Fact]
        public void ManySaveWithNewObjects()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));

            var session = this.CreateSession();

            var martien = session.InstantiateDatabaseObject(3).Object as Person;

            var mathijs = session.Create<Person>() as Person;
            mathijs.FirstName = "Mathijs";
            mathijs.LastName = "Verwer";

            var acme2 = session.Create<Organisation>(this.M.Organisation.Class) as Organisation;
            acme2.Name = "Acme 2";
            acme2.Manager = mathijs;
            acme2.AddEmployee(mathijs);

            var acme3 = session.Create<Organisation>(this.M.Organisation.Class) as Organisation;
            acme3.Name = "Acme 3";
            acme3.Manager = martien;
            acme3.AddEmployee(martien);

            var save = session.PushRequest();

            Assert.Equal(3, save.NewObjects.Length);
            Assert.Empty(save.Objects);
            {
                var savedMathijs = save.NewObjects.First(v => v.NewWorkspaceId == mathijs.Strategy.Identity.ToString());

                Assert.Equal(this.M.Person.Class.IdAsString, savedMathijs.ObjectType);
                Assert.Equal(2, savedMathijs.Roles.Length);

                var savedMathijsFirstName = savedMathijs.Roles.First(v => v.RelationType == this.M.Person.FirstName.RelationType.IdAsString);
                Assert.Equal("Mathijs", savedMathijsFirstName.SetRole);

                var savedMathijsLastName = savedMathijs.Roles.First(v => v.RelationType == this.M.Person.LastName.RelationType.IdAsString);
                Assert.Equal("Verwer", savedMathijsLastName.SetRole);
            }

            {
                var savedAcme2 = save.NewObjects.First(v => v.NewWorkspaceId == acme2.Strategy.Identity.ToString());

                Assert.Equal(this.M.Organisation.Class.IdAsString, savedAcme2.ObjectType);
                Assert.Equal(3, savedAcme2.Roles.Length);

                var savedAcme2Manager = savedAcme2.Roles.First(v => v.RelationType == this.M.Organisation.Manager.RelationType.IdAsString);

                Assert.Equal(mathijs.Strategy.Identity.ToString(), savedAcme2Manager.SetRole);

                var savedAcme2Employees = savedAcme2.Roles.First(v => v.RelationType == this.M.Organisation.Employees.RelationType.IdAsString);

                Assert.Null(savedAcme2Employees.SetRole);
                Assert.Contains(mathijs.Strategy.Identity.ToString(), savedAcme2Employees.AddRole);
                Assert.Null(savedAcme2Employees.RemoveRole);
            }

            {
                var savedAcme3 = save.NewObjects.First(v => v.NewWorkspaceId == acme3.Strategy.Identity.ToString());

                Assert.Equal(this.M.Organisation.Class.IdAsString, savedAcme3.ObjectType);
                Assert.Equal(3, savedAcme3.Roles.Length);

                var savedAcme3Manager = savedAcme3.Roles.First(v => v.RelationType == this.M.Organisation.Manager.RelationType.IdAsString);

                Assert.Equal("3", savedAcme3Manager.SetRole);

                var savedAcme3Employees = savedAcme3.Roles.First(v => v.RelationType == this.M.Organisation.Employees.RelationType.IdAsString);

                Assert.Null(savedAcme3Employees.SetRole);
                Assert.Contains("3", savedAcme3Employees.AddRole);
                Assert.Null(savedAcme3Employees.RemoveRole);
            }
        }

        [Fact]
        public void SyncWithNewObjects()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));

            var session = this.CreateSession();

            var martien = session.InstantiateDatabaseObject(3).Object as Person;

            var mathijs = session.Create<Person>(this.M.Person.Class) as Person;
            mathijs.FirstName = "Mathijs";
            mathijs.LastName = "Verwer";

            var acme2 = session.Create<Organisation>(this.M.Organisation.Class) as Organisation;
            acme2.Name = "Acme 2";
            acme2.Owner = martien;
            acme2.Manager = mathijs;
            acme2.AddEmployee(martien);
            acme2.AddEmployee(mathijs);

            session.Reset();

            // Assert.Null(mathijs.DatabaseId);
            Assert.True(mathijs.Strategy.Identity < 0);
            Assert.Null(mathijs.FirstName);
            Assert.Null(mathijs.LastName);

            // Assert.Null(acme2.DatabaseId);
            Assert.True(acme2.Strategy.Identity < 0);
            Assert.Null(acme2.Owner);
            Assert.Null(acme2.Manager);

            Assert.Empty(acme2.Employees);
        }

        [Fact]
        public void Onsaved()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));

            var session = this.CreateSession();

            var pushResponse = new PushResponse();

            session.PushResponse(pushResponse);

            var mathijs = session.Create<Person>(this.M.Person.Class) as Person;
            mathijs.FirstName = "Mathijs";
            mathijs.LastName = "Verwer";

            var workspaceId = mathijs.Strategy.Identity;

            pushResponse = new PushResponse
            {
                NewObjects = new[] { new PushResponseNewObject { DatabaseId = "10000", WorkspaceId = workspaceId.ToString() } },
            };

            session.PushResponse(pushResponse);

            Assert.NotNull(mathijs.Strategy.Identity);
            Assert.Equal(10000, mathijs.Identity);
            Assert.Equal("Person", mathijs.Strategy.Class.Name);

            var mathijs2 = session.Get<Person>(10000);

            Assert.NotNull(mathijs2);

            Assert.Equal(mathijs, mathijs2);
        }

        /*
        [Fact]
        public void methodCanExecute()
        {
            var database = new Database();
            database.Sync(Fixture.loadData);

            var session = new Session(database);

            var acme = session.InstantiateDatabaseObject(101).Object as Organisation;
            var ocme = session.InstantiateDatabaseObject(102).Object as Organisation;
            var icme = session.InstantiateDatabaseObject(103).Object as Organisation;

            Assert.True(acme.CanExecuteJustDoIt);
            this.isFalse(ocme.CanExecuteJustDoIt);
            this.isFalse(icme.CanExecuteJustDoIt);
        }
        */

        [Fact]
        public void Get()
        {
            this.Database.SyncResponse(Fixture.LoadData(this.M));

            var session = this.CreateSession();

            var acme = (Organisation)session.Create<Organisation>(this.M.Organisation.Class);

            var acmeAgain = session.Get<Organisation>(acme.Identity);

            Assert.Equal(acme, acmeAgain);
        }

        private RemoteSession CreateSession() => (RemoteSession)this.Workspace.CreateSession();
    }
}

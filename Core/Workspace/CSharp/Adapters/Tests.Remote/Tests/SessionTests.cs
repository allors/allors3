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
    using Allors.Workspace.Domain;
    using Xunit;

    public class SessionTests : Test
    {
        [Fact]
        public async void UnitGet()
        {
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));
            var session = this.CreateSession();

            var koen = session.InstantiateDatabaseObject(1 ).Object as Person;

            Assert.Equal("Koen", koen.FirstName);
            Assert.Null(koen.MiddleName);
            Assert.Equal("Van Exem", koen.LastName);
            Assert.Equal(UnitConvert.FromJson(this.M.Person.BirthDate.ObjectType.Tag, "1973-03-27T18:00:00Z"), koen.BirthDate);
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
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));

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
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));

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
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));
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

            var savedKoen = save.Objects.First(v => v.DatabaseId == 1);

            Assert.Equal(1001, savedKoen.Version);
            Assert.Equal(2, savedKoen.Roles.Length);

            var savedKoenFirstName = savedKoen.Roles.First(v => v.RelationType == this.M.Person.FirstName.RelationType.Tag);
            var savedKoenLastName = savedKoen.Roles.First(v => v.RelationType == this.M.Person.LastName.RelationType.Tag);

            Assert.Equal("K", savedKoenFirstName.SetUnitRole);
            Assert.Null(savedKoenFirstName.AddCompositesRole);
            Assert.Null(savedKoenFirstName.RemoveCompositesRole);
            Assert.Equal("VE", savedKoenLastName.SetUnitRole);
            Assert.Null(savedKoenLastName.AddCompositesRole);
            Assert.Null(savedKoenLastName.RemoveCompositesRole);

            var savedMartien = save.Objects.First(v => v.DatabaseId == 3);

            Assert.Equal(1003, savedMartien.Version);
            Assert.Equal(2, savedMartien.Roles.Length);

            var savedMartienFirstName = savedMartien.Roles.First(v => v.RelationType == this.M.Person.FirstName.RelationType.Tag);
            var savedMartienMiddleName = savedMartien.Roles.First(v => v.RelationType == this.M.Person.MiddleName.RelationType.Tag);

            Assert.Equal("Martinus", savedMartienFirstName.SetUnitRole);
            Assert.Null(savedMartienFirstName.AddCompositesRole);
            Assert.Null(savedMartienFirstName.RemoveCompositesRole);
            Assert.Equal("X", savedMartienMiddleName.SetUnitRole);
            Assert.Null(savedMartienMiddleName.AddCompositesRole);
            Assert.Null(savedMartienMiddleName.RemoveCompositesRole);
        }

        [Fact]
        public void OneGet()
        {
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));
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
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));

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
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));
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

            var savedAcme = save.Objects.First(v => v.DatabaseId == 101);

            Assert.Equal(1101, savedAcme.Version);
            Assert.Equal(2, savedAcme.Roles.Length);

            var savedAcmeOwner = savedAcme.Roles.First(v => v.RelationType == this.M.Organisation.Owner.RelationType.Tag);
            var savedAcmeManager = savedAcme.Roles.First(v => v.RelationType == this.M.Organisation.Manager.RelationType.Tag);

            Assert.Equal(3, savedAcmeOwner.SetCompositeRole);
            Assert.Null(savedAcmeOwner.AddCompositesRole);
            Assert.Null(savedAcmeOwner.RemoveCompositesRole);
            Assert.Equal(2, savedAcmeManager.SetCompositeRole);
            Assert.Null(savedAcmeManager.AddCompositesRole);
            Assert.Null(savedAcmeManager.RemoveCompositesRole);

            var savedOcme = save.Objects.First(v => v.DatabaseId == 102);

            Assert.Equal(1102, savedOcme.Version);
            _ = Assert.Single(savedOcme.Roles);

            var savedOcmeOwner = savedOcme.Roles.First(v => v.RelationType == this.M.Organisation.Owner.RelationType.Tag);

            Assert.Null(savedOcmeOwner.SetCompositeRole);
            Assert.Null(savedOcmeOwner.AddCompositesRole);
            Assert.Null(savedOcmeOwner.RemoveCompositesRole);
        }

        [Fact]
        public void ManyGet()
        {
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));
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

            _ = Assert.Single(ocme.Employees);
            Assert.Contains(koen, ocme.Employees);

            Assert.Empty(icme.Employees);

            Assert.Empty(acme.Shareholders);
            Assert.Empty(ocme.Shareholders);
            Assert.Empty(icme.Shareholders);
        }

        [Fact]
        public void ManySet()
        {
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));

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

            _ = Assert.Single(ocme1.Employees);
            Assert.Contains(koen1, ocme1.Employees);

            Assert.Empty(icme1.Employees);

            Assert.Empty(acme2.Employees);

            _ = Assert.Single(ocme2.Employees);
            Assert.Contains(koen2, ocme2.Employees);

            Assert.Equal(3, icme2.Employees.Count());
            Assert.Contains(koen2, icme2.Employees);
            Assert.Contains(martien2, icme2.Employees);
            Assert.Contains(patrick2, icme2.Employees);
        }

        [Fact]
        public void ManySaveWithExistingObjects()
        {
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));

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

            var savedAcme = save.Objects.First(v => v.DatabaseId == 101);

            Assert.Equal(1101, savedAcme.Version);
            _ = Assert.Single(savedAcme.Roles);

            var savedAcmeEmployees = savedAcme.Roles.First(v => v.RelationType == this.M.Organisation.Employees.RelationType.Tag);

            Assert.Null(savedAcmeEmployees.SetUnitRole);
            Assert.Null(savedAcmeEmployees.SetCompositeRole);
            Assert.Empty(savedAcmeEmployees.AddCompositesRole);
            Assert.Contains(1, savedAcmeEmployees.RemoveCompositesRole);
            Assert.Contains(2, savedAcmeEmployees.RemoveCompositesRole);
            Assert.Contains(3, savedAcmeEmployees.RemoveCompositesRole);

            var savedOcme = save.Objects.First(v => v.DatabaseId == 102);

            Assert.Equal(1102, savedOcme.Version);
            _ = Assert.Single(savedOcme.Roles);

            var savedOcmeEmployees = savedOcme.Roles.First(v => v.RelationType == this.M.Organisation.Employees.RelationType.Tag);

            Assert.Null(savedAcmeEmployees.SetUnitRole);
            Assert.Null(savedAcmeEmployees.SetCompositeRole);
            Assert.Equal(2, savedOcmeEmployees.AddCompositesRole.Length);
            Assert.Contains(2, savedOcmeEmployees.AddCompositesRole);
            Assert.Contains(3, savedOcmeEmployees.AddCompositesRole);

            _ = Assert.Single(savedOcmeEmployees.RemoveCompositesRole);
            Assert.Contains(1, savedOcmeEmployees.RemoveCompositesRole);

            var savedIcme = save.Objects.First(v => v.DatabaseId == 103);

            Assert.Equal(1103, savedIcme.Version);
            _ = Assert.Single(savedIcme.Roles);

            var savedIcmeEmployees = savedIcme.Roles.First(v => v.RelationType == this.M.Organisation.Employees.RelationType.Tag);

            Assert.Null(savedAcmeEmployees.SetUnitRole);
            Assert.Null(savedAcmeEmployees.SetCompositeRole);
            Assert.Equal(3, savedIcmeEmployees.AddCompositesRole.Length);
            Assert.Contains(1, savedIcmeEmployees.AddCompositesRole);
            Assert.Contains(2, savedIcmeEmployees.AddCompositesRole);
            Assert.Contains(3, savedIcmeEmployees.AddCompositesRole);
            Assert.Null(savedIcmeEmployees.RemoveCompositesRole);
        }

        [Fact]
        public void ManySaveWithNewObjects()
        {
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));

            var session = this.CreateSession();

            var martien = session.InstantiateDatabaseObject(3).Object as Person;

            var mathijs = session.Create<Person>() as Person;
            mathijs.FirstName = "Mathijs";
            mathijs.LastName = "Verwer";

            var acme2 = session.Create<Organisation>(this.M.Organisation) as Organisation;
            acme2.Name = "Acme 2";
            acme2.Manager = mathijs;
            acme2.AddEmployee(mathijs);

            var acme3 = session.Create<Organisation>(this.M.Organisation) as Organisation;
            acme3.Name = "Acme 3";
            acme3.Manager = martien;
            acme3.AddEmployee(martien);

            var save = session.PushRequest();

            Assert.Equal(3, save.NewObjects.Length);
            Assert.Empty(save.Objects);
            {
                var savedMathijs = save.NewObjects.First(v => v.WorkspaceId == mathijs.Strategy.Id);

                Assert.Equal(this.M.Person.Tag, savedMathijs.ObjectType);
                Assert.Equal(2, savedMathijs.Roles.Length);

                var savedMathijsFirstName = savedMathijs.Roles.First(v => v.RelationType == this.M.Person.FirstName.RelationType.Tag);
                Assert.Equal("Mathijs", savedMathijsFirstName.SetUnitRole);

                var savedMathijsLastName = savedMathijs.Roles.First(v => v.RelationType == this.M.Person.LastName.RelationType.Tag);
                Assert.Equal("Verwer", savedMathijsLastName.SetUnitRole);
            }

            {
                var savedAcme2 = save.NewObjects.First(v => v.WorkspaceId == acme2.Strategy.Id);

                Assert.Equal(this.M.Organisation.Tag, savedAcme2.ObjectType);
                Assert.Equal(3, savedAcme2.Roles.Length);

                var savedAcme2Manager = savedAcme2.Roles.First(v => v.RelationType == this.M.Organisation.Manager.RelationType.Tag);

                Assert.Equal(mathijs.Strategy.Id, savedAcme2Manager.SetCompositeRole);

                var savedAcme2Employees = savedAcme2.Roles.First(v => v.RelationType == this.M.Organisation.Employees.RelationType.Tag);

                Assert.Null(savedAcme2Employees.SetUnitRole);
                Assert.Null(savedAcme2Employees.SetCompositeRole);
                Assert.Contains(mathijs.Strategy.Id, savedAcme2Employees.AddCompositesRole);
                Assert.Null(savedAcme2Employees.RemoveCompositesRole);
            }

            {
                var savedAcme3 = save.NewObjects.First(v => v.WorkspaceId == acme3.Strategy.Id);

                Assert.Equal(this.M.Organisation.Tag, savedAcme3.ObjectType);
                Assert.Equal(3, savedAcme3.Roles.Length);

                var savedAcme3Manager = savedAcme3.Roles.First(v => v.RelationType == this.M.Organisation.Manager.RelationType.Tag);

                Assert.Equal(3, savedAcme3Manager.SetCompositeRole);

                var savedAcme3Employees = savedAcme3.Roles.First(v => v.RelationType == this.M.Organisation.Employees.RelationType.Tag);

                Assert.Null(savedAcme3Employees.SetUnitRole);
                Assert.Null(savedAcme3Employees.SetCompositeRole);
                Assert.Contains(3, savedAcme3Employees.AddCompositesRole);
                Assert.Null(savedAcme3Employees.RemoveCompositesRole);
            }
        }

        [Fact]
        public void SyncWithNewObjects()
        {
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));

            var session = this.CreateSession();

            var martien = session.InstantiateDatabaseObject(3).Object as Person;

            var mathijs = session.Create<Person>(this.M.Person) as Person;
            mathijs.FirstName = "Mathijs";
            mathijs.LastName = "Verwer";

            var acme2 = session.Create<Organisation>(this.M.Organisation) as Organisation;
            acme2.Name = "Acme 2";
            acme2.Owner = martien;
            acme2.Manager = mathijs;
            acme2.AddEmployee(martien);
            acme2.AddEmployee(mathijs);

            session.Reset();

            // Assert.Null(mathijs.DatabaseId);
            Assert.True(mathijs.Strategy.Id < 0);
            Assert.Null(mathijs.FirstName);
            Assert.Null(mathijs.LastName);

            // Assert.Null(acme2.DatabaseId);
            Assert.True(acme2.Strategy.Id < 0);
            Assert.Null(acme2.Owner);
            Assert.Null(acme2.Manager);

            Assert.Empty(acme2.Employees);
        }

        [Fact]
        public void Onsaved()
        {
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));

            var session = this.CreateSession();

            var pushResponse = new PushResponse();

            session.PushResponse(pushResponse);

            var mathijs = session.Create<Person>(this.M.Person) as Person;
            mathijs.FirstName = "Mathijs";
            mathijs.LastName = "Verwer";

            var workspaceId = mathijs.Strategy.Id;

            pushResponse = new PushResponse
            {
                NewObjects = new[] { new PushResponseNewObject { DatabaseId = 10000, WorkspaceId = workspaceId } },
            };

            session.PushResponse(pushResponse);

            Assert.NotNull(mathijs.Strategy.Id);
            Assert.Equal(10000, mathijs.Id);
            Assert.Equal("Person", mathijs.Strategy.Class.SingularName);

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
            _ = this.Database.SyncResponse(Fixture.LoadData(this.M));

            var session = this.CreateSession();

            var acme = (Organisation)session.Create<Organisation>(this.M.Organisation);

            var acmeAgain = session.Get<Organisation>(acme.Id);

            Assert.Equal(acme, acmeAgain);
        }

        private Session CreateSession() => (Session)this.Workspace.CreateSession();
    }
}

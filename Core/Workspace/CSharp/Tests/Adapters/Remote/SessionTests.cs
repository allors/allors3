// <copyright file="SessionTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Adapters.Remote
{
    using System.Linq;
    using Allors.Protocol.Data;
    using Allors.Protocol.Database.Push;
    using Allors.Workspace.Domain;
    using Adapters;
    using Allors.Workspace.Adapters.Remote;
    using Xunit;

    public class SessionTests : Test
    {
        [Fact]
        public void UnitGet()
        {
            this.Database.Sync(Fixture.LoadData(this.M));
            var session = this.CreateSession();

            var koen = session.Instantiate(1) as Person;

            Assert.Equal("Koen", koen.FirstName);
            Assert.Null(koen.MiddleName);
            Assert.Equal("Van Exem", koen.LastName);
            Assert.Equal(UnitConvert.Parse(this.M.Person.BirthDate.ObjectType.Id, "1973-03-27T18:00:00Z"), koen.BirthDate);
            Assert.True(koen.IsStudent);

            var patrick = session.Instantiate(2) as Person;

            Assert.Equal("Patrick", patrick.FirstName);
            Assert.Equal("De Boeck", patrick.LastName);
            Assert.Null(patrick.MiddleName);
            Assert.Null(patrick.BirthDate);
            Assert.False(patrick.IsStudent);

            var martien = session.Instantiate(3) as Person;

            Assert.Equal("Martien", martien.FirstName);
            Assert.Equal("Knippenberg", martien.LastName);
            Assert.Equal("van", martien.MiddleName);
            Assert.Null(martien.BirthDate);
            Assert.Null(martien.IsStudent);
        }

        [Fact]
        public void UnitSet()
        {
            this.Database.Sync(Fixture.LoadData(this.M));

            var session1 = this.CreateSession();
            var martien1 = session1.Instantiate(3) as Person;

            var session2 = this.CreateSession();
            var martien2 = session2.Instantiate(3) as Person;

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
            this.Database.Sync(Fixture.LoadData(this.M));

            var session = this.CreateSession();
            var martien = session.Instantiate(3) as Person;
            var acme = session.Instantiate(101) as Organisation;

            Assert.False(session.HasChanges);

            var firstName = martien.FirstName;
            martien.FirstName = firstName;

            Assert.False(session.HasChanges);

            martien.UserName = null;

            Assert.False(session.HasChanges);

            var owner = acme.Owner;
            acme.Owner = owner;

            Assert.False(session.HasChanges);

            acme.CycleOne = null;

            Assert.False(session.HasChanges);

            var employees = acme.Employees;
            acme.Employees = employees;

            Assert.False(session.HasChanges);

            employees = employees.Reverse().ToArray();
            acme.Employees = employees;

            Assert.False(session.HasChanges);

            acme.CycleMany = null;

            Assert.False(session.HasChanges);
        }

        [Fact]
        public void UnitSave()
        {
            this.Database.Sync(Fixture.LoadData(this.M));
            var session = this.CreateSession();

            var koen = session.Instantiate(1) as Person;
            var patrick = session.Instantiate(2) as Person;
            var martien = session.Instantiate(3) as Person;

            koen.FirstName = "K";
            koen.LastName = "VE";
            martien.FirstName = "Martinus";
            martien.MiddleName = "X";

            var save = session.PushRequest();

            Assert.Equal(2, save.Objects.Length);

            var savedKoen = save.Objects.First(v => v.I == "1");

            Assert.Equal("1001", savedKoen.V);
            Assert.Equal(2, savedKoen.Roles.Length);

            var savedKoenFirstName = savedKoen.Roles.First(v => v.T == this.M.Person.FirstName.RelationType.IdAsString);
            var savedKoenLastName = savedKoen.Roles.First(v => v.T == this.M.Person.LastName.RelationType.IdAsString);

            Assert.Equal("K", savedKoenFirstName.S);
            Assert.Null(savedKoenFirstName.A);
            Assert.Null(savedKoenFirstName.R);
            Assert.Equal("VE", savedKoenLastName.S);
            Assert.Null(savedKoenLastName.A);
            Assert.Null(savedKoenLastName.R);

            var savedMartien = save.Objects.First(v => v.I == "3");

            Assert.Equal("1003", savedMartien.V);
            Assert.Equal(2, savedMartien.Roles.Length);

            var savedMartienFirstName = savedMartien.Roles.First(v => v.T == this.M.Person.FirstName.RelationType.IdAsString);
            var savedMartienMiddleName = savedMartien.Roles.First(v => v.T == this.M.Person.MiddleName.RelationType.IdAsString);

            Assert.Equal("Martinus", savedMartienFirstName.S);
            Assert.Null(savedMartienFirstName.A);
            Assert.Null(savedMartienFirstName.R);
            Assert.Equal("X", savedMartienMiddleName.S);
            Assert.Null(savedMartienMiddleName.A);
            Assert.Null(savedMartienMiddleName.R);
        }

        [Fact]
        public void OneGet()
        {
            this.Database.Sync(Fixture.LoadData(this.M));
            var session = this.CreateSession();

            var koen = session.Instantiate(1) as Person;
            var patrick = session.Instantiate(2) as Person;
            var martien = session.Instantiate(3) as Person;

            var acme = session.Instantiate(101) as Organisation;
            var ocme = session.Instantiate(102) as Organisation;
            var icme = session.Instantiate(103) as Organisation;

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
            this.Database.Sync(Fixture.LoadData(this.M));

            var session1 = this.CreateSession();

            var session2 = this.CreateSession();

            var koen1 = session1.Instantiate(1) as Person;
            var patrick1 = session1.Instantiate(2) as Person;
            var martien1 = session1.Instantiate(3) as Person;

            var acme1 = session1.Instantiate(101) as Organisation;
            var ocme1 = session1.Instantiate(102) as Organisation;
            var icme1 = session1.Instantiate(103) as Organisation;

            var koen2 = session2.Instantiate(1) as Person;
            var patrick2 = session2.Instantiate(2) as Person;
            var martien2 = session2.Instantiate(3) as Person;

            var acme2 = session2.Instantiate(101) as Organisation;
            var ocme2 = session2.Instantiate(102) as Organisation;
            var icme2 = session2.Instantiate(103) as Organisation;

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
            this.Database.Sync(Fixture.LoadData(this.M));
            var session = this.CreateSession();

            var koen = session.Instantiate(1) as Person;
            var patrick = session.Instantiate(2) as Person;
            var martien = session.Instantiate(3) as Person;

            var acme = session.Instantiate(101) as Organisation;
            var ocme = session.Instantiate(102) as Organisation;
            var icme = session.Instantiate(103) as Organisation;

            acme.Owner = martien;
            ocme.Owner = null;

            acme.Manager = patrick;

            var save = session.PushRequest();

            Assert.Equal(2, save.Objects.Length);

            var savedAcme = save.Objects.First(v => v.I == "101");

            Assert.Equal("1101", savedAcme.V);
            Assert.Equal(2, savedAcme.Roles.Length);

            var savedAcmeOwner = savedAcme.Roles.First(v => v.T == this.M.Organisation.Owner.RelationType.IdAsString);
            var savedAcmeManager = savedAcme.Roles.First(v => v.T == this.M.Organisation.Manager.RelationType.IdAsString);

            Assert.Equal("3", savedAcmeOwner.S);
            Assert.Null(savedAcmeOwner.A);
            Assert.Null(savedAcmeOwner.R);
            Assert.Equal("2", savedAcmeManager.S);
            Assert.Null(savedAcmeManager.A);
            Assert.Null(savedAcmeManager.R);

            var savedOcme = save.Objects.First(v => v.I == "102");

            Assert.Equal("1102", savedOcme.V);
            Assert.Single(savedOcme.Roles);

            var savedOcmeOwner = savedOcme.Roles.First(v => v.T == this.M.Organisation.Owner.RelationType.IdAsString);

            Assert.Null(savedOcmeOwner.S);
            Assert.Null(savedOcmeOwner.A);
            Assert.Null(savedOcmeOwner.R);
        }

        [Fact]
        public void ManyGet()
        {
            this.Database.Sync(Fixture.LoadData(this.M));
            var session = this.CreateSession();

            var koen = (Person)session.Instantiate(1);
            var patrick = (Person)session.Instantiate(2);
            var martien = (Person)session.Instantiate(3);

            var acme = (Organisation)session.Instantiate(101);
            var ocme = (Organisation)session.Instantiate(102);
            var icme = (Organisation)session.Instantiate(103);

            Assert.Equal(3, acme.Employees.Length);
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
            this.Database.Sync(Fixture.LoadData(this.M));

            var session1 = this.CreateSession();

            var session2 = this.CreateSession();

            var koen1 = session1.Instantiate(1) as Person;
            var patrick1 = session1.Instantiate(2) as Person;
            var martien1 = session1.Instantiate(3) as Person;

            var acme1 = session1.Instantiate(101) as Organisation;
            var ocme1 = session1.Instantiate(102) as Organisation;
            var icme1 = session1.Instantiate(103) as Organisation;

            var koen2 = session2.Instantiate(1) as Person;
            var patrick2 = session2.Instantiate(2) as Person;
            var martien2 = session2.Instantiate(3) as Person;

            var acme2 = session2.Instantiate(101) as Organisation;
            var ocme2 = session2.Instantiate(102) as Organisation;
            var icme2 = session2.Instantiate(103) as Organisation;

            acme2.Employees = null;
            icme2.Employees = new[] { koen2, patrick2, martien2 };

            Assert.Equal(3, acme1.Employees.Length);
            Assert.Contains(koen1, acme1.Employees);
            Assert.Contains(martien1, acme1.Employees);
            Assert.Contains(patrick1, acme1.Employees);

            Assert.Single(ocme1.Employees);
            Assert.Contains(koen1, ocme1.Employees);

            Assert.Empty(icme1.Employees);

            Assert.Empty(acme2.Employees);

            Assert.Single(ocme2.Employees);
            Assert.Contains(koen2, ocme2.Employees);

            Assert.Equal(3, icme2.Employees.Length);
            Assert.Contains(koen2, icme2.Employees);
            Assert.Contains(martien2, icme2.Employees);
            Assert.Contains(patrick2, icme2.Employees);
        }

        [Fact]
        public void ManySaveWithExistingObjects()
        {
            this.Database.Sync(Fixture.LoadData(this.M));

            var session = this.CreateSession();

            var koen = session.Instantiate(1) as Person;
            var patrick = session.Instantiate(2) as Person;
            var martien = session.Instantiate(3) as Person;

            var acme = session.Instantiate(101) as Organisation;
            var ocme = session.Instantiate(102) as Organisation;
            var icme = session.Instantiate(103) as Organisation;

            acme.Employees = null;
            ocme.Employees = new[] { martien, patrick };
            icme.Employees = new[] { koen, patrick, martien };

            var save = session.PushRequest();

            Assert.Empty(save.NewObjects);
            Assert.Equal(3, save.Objects.Length);

            var savedAcme = save.Objects.First(v => v.I == "101");

            Assert.Equal("1101", savedAcme.V);
            Assert.Single(savedAcme.Roles);

            var savedAcmeEmployees = savedAcme.Roles.First(v => v.T == this.M.Organisation.Employees.RelationType.IdAsString);

            Assert.Null(savedAcmeEmployees.S);
            Assert.Empty(savedAcmeEmployees.A);
            Assert.Contains("1", savedAcmeEmployees.R);
            Assert.Contains("2", savedAcmeEmployees.R);
            Assert.Contains("3", savedAcmeEmployees.R);

            var savedOcme = save.Objects.First(v => v.I == "102");

            Assert.Equal("1102", savedOcme.V);
            Assert.Single(savedOcme.Roles);

            var savedOcmeEmployees = savedOcme.Roles.First(v => v.T == this.M.Organisation.Employees.RelationType.IdAsString);

            Assert.Null(savedOcmeEmployees.S);
            Assert.Equal(2, savedOcmeEmployees.A.Length);
            Assert.Contains("2", savedOcmeEmployees.A);
            Assert.Contains("3", savedOcmeEmployees.A);

            Assert.Single(savedOcmeEmployees.R);
            Assert.Contains("1", savedOcmeEmployees.R);

            var savedIcme = save.Objects.First(v => v.I == "103");

            Assert.Equal("1103", savedIcme.V);
            Assert.Single(savedIcme.Roles);

            var savedIcmeEmployees = savedIcme.Roles.First(v => v.T == this.M.Organisation.Employees.RelationType.IdAsString);

            Assert.Null(savedIcmeEmployees.S);
            Assert.Equal(3, savedIcmeEmployees.A.Length);
            Assert.Contains("1", savedIcmeEmployees.A);
            Assert.Contains("2", savedIcmeEmployees.A);
            Assert.Contains("3", savedIcmeEmployees.A);
            Assert.Null(savedIcmeEmployees.R);
        }

        [Fact]
        public void ManySaveWithNewObjects()
        {
            this.Database.Sync(Fixture.LoadData(this.M));

            var session = this.CreateSession();

            var martien = session.Instantiate(3) as Person;

            var mathijs = session.Create(this.M.Person.Class) as Person;
            mathijs.FirstName = "Mathijs";
            mathijs.LastName = "Verwer";

            var acme2 = session.Create(this.M.Organisation.Class) as Organisation;
            acme2.Name = "Acme 2";
            acme2.Manager = mathijs;
            acme2.AddEmployee(mathijs);

            var acme3 = session.Create(this.M.Organisation.Class) as Organisation;
            acme3.Name = "Acme 3";
            acme3.Manager = martien;
            acme3.AddEmployee(martien);

            var save = session.PushRequest();

            Assert.Equal(3, save.NewObjects.Length);
            Assert.Empty(save.Objects);
            {
                var savedMathijs = save.NewObjects.First(v => v.NI == mathijs.Strategy.NewId?.ToString());

                Assert.Equal(this.M.Person.Class.IdAsString, savedMathijs.T);
                Assert.Equal(2, savedMathijs.Roles.Length);

                var savedMathijsFirstName = savedMathijs.Roles.First(v => v.T == this.M.Person.FirstName.RelationType.IdAsString);
                Assert.Equal("Mathijs", savedMathijsFirstName.S);

                var savedMathijsLastName = savedMathijs.Roles.First(v => v.T == this.M.Person.LastName.RelationType.IdAsString);
                Assert.Equal("Verwer", savedMathijsLastName.S);
            }

            {
                var savedAcme2 = save.NewObjects.First(v => v.NI == acme2.Strategy.NewId?.ToString());

                Assert.Equal(this.M.Organisation.Class.IdAsString, savedAcme2.T);
                Assert.Equal(3, savedAcme2.Roles.Length);

                var savedAcme2Manager = savedAcme2.Roles.First(v => v.T == this.M.Organisation.Manager.RelationType.IdAsString);

                Assert.Equal(mathijs.Strategy.NewId.ToString(), savedAcme2Manager.S);

                var savedAcme2Employees = savedAcme2.Roles.First(v => v.T == this.M.Organisation.Employees.RelationType.IdAsString);

                Assert.Null(savedAcme2Employees.S);
                Assert.Contains(mathijs.Strategy.NewId?.ToString(), savedAcme2Employees.A);
                Assert.Null(savedAcme2Employees.R);
            }

            {
                var savedAcme3 = save.NewObjects.First(v => v.NI == acme3.Strategy.NewId?.ToString());

                Assert.Equal(this.M.Organisation.Class.IdAsString, savedAcme3.T);
                Assert.Equal(3, savedAcme3.Roles.Length);

                var savedAcme3Manager = savedAcme3.Roles.First(v => v.T == this.M.Organisation.Manager.RelationType.IdAsString);

                Assert.Equal("3", savedAcme3Manager.S);

                var savedAcme3Employees = savedAcme3.Roles.First(v => v.T == this.M.Organisation.Employees.RelationType.IdAsString);

                Assert.Null(savedAcme3Employees.S);
                Assert.Contains("3", savedAcme3Employees.A);
                Assert.Null(savedAcme3Employees.R);
            }
        }

        [Fact]
        public void SyncWithNewObjects()
        {
            this.Database.Sync(Fixture.LoadData(this.M));

            var session = this.CreateSession();

            var martien = session.Instantiate(3) as Person;

            var mathijs = session.Create(this.M.Person.Class) as Person;
            mathijs.FirstName = "Mathijs";
            mathijs.LastName = "Verwer";

            var acme2 = session.Create(this.M.Organisation.Class) as Organisation;
            acme2.Name = "Acme 2";
            acme2.Owner = martien;
            acme2.Manager = mathijs;
            acme2.AddEmployee(martien);
            acme2.AddEmployee(mathijs);

            session.Reset();

            // Assert.Null(mathijs.Id);
            Assert.True(mathijs.Strategy.NewId < 0);
            Assert.Null(mathijs.FirstName);
            Assert.Null(mathijs.LastName);

            // Assert.Null(acme2.Id);
            Assert.True(acme2.Strategy.NewId < 0);
            Assert.Null(acme2.Owner);
            Assert.Null(acme2.Manager);

            Assert.Empty(acme2.Employees);
        }

        [Fact]
        public void Onsaved()
        {
            this.Database.Sync(Fixture.LoadData(this.M));

            var session = this.CreateSession();

            var pushResponse = new PushResponse();

            session.PushResponse(pushResponse);

            var mathijs = session.Create(this.M.Person.Class) as Person;
            mathijs.FirstName = "Mathijs";
            mathijs.LastName = "Verwer";

            var newId = mathijs.Strategy.NewId.Value;

            pushResponse = new PushResponse
            {
                NewObjects = new[] { new PushResponseNewObject { I = "10000", NI = newId.ToString() } },
            };

            session.PushResponse(pushResponse);

            Assert.Null(mathijs.Strategy.NewId);
            Assert.Equal(10000, mathijs.Id);
            Assert.Equal("Person", mathijs.Strategy.ObjectType.Name);

            mathijs = session.Instantiate(10000) as Person;

            Assert.NotNull(mathijs);

            var exceptionThrown = false;
            try
            {
                session.Instantiate(newId);
            }
            catch
            {
                exceptionThrown = true;
            }

            Assert.True(exceptionThrown);
        }

        /*
        [Fact]
        public void methodCanExecute()
        {
            var database = new Database();
            database.Sync(Fixture.loadData);

            var session = new Session(database);

            var acme = session.Instantiate("101") as Organisation;
            var ocme = session.Instantiate("102") as Organisation;
            var icme = session.Instantiate("102") as Organisation;

            Assert.True(acme.CanExecuteJustDoIt);
            this.isFalse(ocme.CanExecuteJustDoIt);
            this.isFalse(icme.CanExecuteJustDoIt);
        }
        */

        [Fact]
        public void Get()
        {
            this.Database.Sync(Fixture.LoadData(this.M));

            var session = this.CreateSession();

            var acme = (Organisation)session.Create(this.M.Organisation.Class);

            var acmeAgain = session.Instantiate(acme.Id);

            Assert.Equal(acme, acmeAgain);

            acmeAgain = session.Instantiate(acme.Strategy.NewId.Value);

            Assert.Equal(acme, acmeAgain);
        }

        private Session CreateSession() => (Session)this.Workspace.CreateSession();
    }
}

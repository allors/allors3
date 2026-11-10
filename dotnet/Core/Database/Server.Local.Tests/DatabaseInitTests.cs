// <copyright file="DatabaseInitTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.IO;
    using System.Xml;
    using Allors.Database;
    using Allors.Database.Adapters;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Microsoft.Extensions.Configuration;
    using Xunit;
    using C1 = Allors.Database.Domain.C1;
    using Person = Allors.Database.Domain.Person;

    /// <summary>
    /// A database that lives longer than its population, as the Core test server's does: Init replaces the
    /// population while the database and its services stay.
    /// </summary>
    public class DatabaseInitTests
    {
        [Fact]
        public void InitForgetsTheObjectsOfThePreviousPopulation()
        {
            var configuration = new ConfigurationBuilder().AddAllorsConfiguration("core").Build();
            var metaPopulation = new MetaBuilder().Build();
            var engine = new Engine(Rules.Create(metaPopulation));
            var database = new DatabaseBuilder(new DefaultDatabaseServices(engine), configuration, new ObjectFactory(metaPopulation, typeof(C1))).Build();

            database.Init();
            new Setup(database, new Config()).Apply();

            using (var transaction = database.CreateTransaction())
            {
                // Resolving the group caches its id in the services of the database.
                Assert.Equal(UserGroups.AdministratorsId, new UserGroups(transaction).Administrators.UniqueId);
            }

            database.Init();

            using (var transaction = database.CreateTransaction())
            {
                // An object created before the new setup shifts every id: a cached id would name another object on
                // an adapter that restarts its ids, and no object on one that continues them.
                new C1Builder(transaction).Build();
                transaction.Commit();
            }

            new Setup(database, new Config()).Apply();

            using (var transaction = database.CreateTransaction())
            {
                var administrators = new UserGroups(transaction).Administrators;
                Assert.Equal(UserGroups.AdministratorsId, administrators.UniqueId);
                Assert.Equal(administrators, Assert.Single(new Grants(transaction).Administrator.SubjectGroups));
            }
        }

        [Theory]
        [InlineData("memory")]
        [InlineData("snapshot")]
        public void LoadPreservesAdministratorAccess(string adapter)
        {
            var metaPopulation = new MetaBuilder().Build();
            var engine = new Engine(Rules.Create(metaPopulation));
            var services = new DefaultDatabaseServices(engine);
            var objectFactory = new ObjectFactory(metaPopulation, typeof(C1));
            IDatabase database = adapter == "memory"
                ? new Allors.Database.Adapters.Memory.Database(services, new Allors.Database.Adapters.Memory.Configuration { ObjectFactory = objectFactory })
                : new Allors.Database.Adapters.Snapshot.Database(services, new Allors.Database.Adapters.Snapshot.Configuration { ObjectFactory = objectFactory });

            database.Init();
            new Setup(database, new Config()).Apply();

            long administratorId;
            using (var transaction = database.CreateTransaction())
            {
                var administrator = new Users(transaction).FindBy(metaPopulation.User.UserName, "jane@example.com");
                administratorId = administrator.Id;

                Assert.Equal(UserGroups.AdministratorsId, new UserGroups(transaction).Administrators.UniqueId);
                var accessControl = new DatabaseAccessControl(database.Services.Get<ISecurity>(), administrator);
                Assert.True(accessControl[administrator].CanRead(metaPopulation.Person.FirstName));
            }

            using var population = new MemoryStream();
            using (var writer = XmlWriter.Create(population, new XmlWriterSettings { CloseOutput = false }))
            {
                database.Save(writer);
            }

            population.Position = 0;
            using (var reader = XmlReader.Create(population))
            {
                database.Load(reader);
            }

            using (var transaction = database.CreateTransaction())
            {
                var administrator = Assert.IsType<Person>(transaction.Instantiate(administratorId));
                Assert.Equal("Jane", administrator.FirstName);
                Assert.Equal(UserGroups.AdministratorsId, new UserGroups(transaction).Administrators.UniqueId);

                var accessControl = new DatabaseAccessControl(database.Services.Get<ISecurity>(), administrator);
                Assert.True(accessControl[administrator].CanRead(metaPopulation.Person.FirstName));
            }
        }
    }
}

// <copyright file="DatabaseInitTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using Allors.Database;
    using Allors.Database.Adapters;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Microsoft.Extensions.Configuration;
    using Xunit;
    using C1 = Allors.Database.Domain.C1;

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
    }
}

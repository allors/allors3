// <copyright file="RepeatedSetupTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Collections.Generic;
    using Allors.Database.Adapters;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using Microsoft.Extensions.Configuration;
    using Xunit;
    using C1 = Allors.Database.Domain.C1;
    using ObjectFactory = Allors.Database.ObjectFactory;

    public class RepeatedSetupTests : IClassFixture<Fixture>
    {
        private readonly Fixture fixture;

        public RepeatedSetupTests(Fixture fixture) => this.fixture = fixture;

        // The out-of-process server runs this exact sequence for every Test/Setup, reusing a single
        // IDatabase. A second cycle used to fail on Npgsql with "Grant.Subjects, Grant.SubjectGroups at
        // least one!": Init recreates the schema and restarts object-id allocation, but data-scoped
        // database services kept pre-wipe id mappings, so the security Setup merger resolved a Grant to a
        // dead id and never re-linked its subjects. Both cycles must succeed on every adapter.
        [Fact]
        public void RepeatedSetupSucceeds()
        {
            var connectionString = this.fixture.EnsureDatabase(this.GetType().Name);

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddAllorsConfiguration("core", "commands");
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
            });
            var configuration = configurationBuilder.Build();

            var database = new DatabaseBuilder(
                new DefaultDatabaseServices(this.fixture.Engine),
                configuration,
                new ObjectFactory(this.fixture.MetaPopulation, typeof(C1))).Build();

            var m = this.fixture.MetaPopulation;

            for (var cycle = 0; cycle < 2; cycle++)
            {
                database.Init();

                new Setup(database, new Config()).Apply();

                using var transaction = database.CreateTransaction();
                transaction.Derive();
                transaction.Commit();

                var administrator = new PersonBuilder(transaction).WithUserName("administrator").Build();
                new UserGroups(transaction).Administrators.AddMember(administrator);
                transaction.Services.Get<IUserService>().User = administrator;

                new TestPopulation(transaction).Apply();
                transaction.Derive();
                transaction.Commit();
            }
        }
    }
}

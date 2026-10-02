// <copyright file="IdentityUserResolverTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Globalization;
    using System.Security.Claims;
    using Allors.Database;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Allors.Security;
    using Xunit;
    using MemoryConfiguration = Allors.Database.Adapters.Memory.Configuration;
    using MemoryDatabase = Allors.Database.Adapters.Memory.Database;
    using User = Allors.Database.Domain.User;

    // ASP.NET Core Identity signs a user in with the Allors object id as its user id, so the
    // NameIdentifier claim of an Identity principal is that id.
    public class IdentityUserResolverTests
    {
        [Fact]
        public void NameIdentifierResolvesTheUser()
        {
            var database = NewDatabase();
            var userId = NewUser(database);
            using var transaction = database.CreateTransaction();

            var user = new IdentityUserResolver().Resolve(Principal(new Claim(ClaimTypes.NameIdentifier, userId.ToString(CultureInfo.InvariantCulture))), transaction);

            Assert.Equal(userId, user?.Id);
        }

        [Fact]
        public void PrincipalWithoutNameIdentifierResolvesNoUser()
        {
            using var transaction = NewDatabase().CreateTransaction();

            var user = new IdentityUserResolver().Resolve(Principal(new Claim(ClaimTypes.Name, "jane@example.com")), transaction);

            Assert.Null(user);
        }

        [Fact]
        public void NameIdentifierThatIsNoObjectIdResolvesNoUser()
        {
            using var transaction = NewDatabase().CreateTransaction();

            var user = new IdentityUserResolver().Resolve(Principal(new Claim(ClaimTypes.NameIdentifier, "not-an-object-id")), transaction);

            Assert.Null(user);
        }

        private static ClaimsPrincipal Principal(Claim claim) => new(new ClaimsIdentity(new[] { claim }, "Tests"));

        private static long NewUser(IDatabase database)
        {
            using var transaction = database.CreateTransaction();
            var person = new PersonBuilder(transaction).WithUserName("resolved").Build();
            transaction.Derive();
            transaction.Commit();
            return person.Id;
        }

        private static IDatabase NewDatabase()
        {
            var metaPopulation = new MetaBuilder().Build();
            var database = new MemoryDatabase(
                new DefaultDatabaseServices(new Engine(Rules.Create(metaPopulation))),
                new MemoryConfiguration
                {
                    ObjectFactory = new ObjectFactory(metaPopulation, typeof(User)),
                });

            database.Init();
            new Setup(database, new Config { SetupSecurity = false }).Apply();

            return database;
        }
    }
}

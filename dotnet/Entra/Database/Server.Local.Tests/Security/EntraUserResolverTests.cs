// <copyright file="EntraUserResolverTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
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

    // A principal of the Entra plug-in names its Entra identity, from the session or from a bearer
    // token, and the user is looked up by it on every request.
    public class EntraUserResolverTests
    {
        private static readonly Guid Tenant = new Guid("cd3598ab-ef18-4774-bfce-c1b3cebe5a42");

        [Fact]
        public void TheEntraIdentityResolvesTheUser()
        {
            var database = NewDatabase();
            var objectId = Guid.NewGuid();
            var userId = NewUser(database, objectId);
            using var transaction = database.CreateTransaction();

            var user = new EntraUserResolver().Resolve(Principal(Tenant, objectId), transaction);

            Assert.Equal(userId, user?.Id);
        }

        [Fact]
        public void AnUnknownIdentityResolvesNoUser()
        {
            var database = NewDatabase();
            NewUser(database, Guid.NewGuid());
            using var transaction = database.CreateTransaction();

            Assert.Null(new EntraUserResolver().Resolve(Principal(Tenant, Guid.NewGuid()), transaction));
        }

        [Fact]
        public void APrincipalWithoutAnEntraIdentityResolvesNoUser()
        {
            using var transaction = NewDatabase().CreateTransaction();

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "Tests"));

            Assert.Null(new EntraUserResolver().Resolve(principal, transaction));
        }

        private static ClaimsPrincipal Principal(Guid tenantId, Guid objectId) =>
            new(new ClaimsIdentity(new[] { new Claim(EntraClaims.TenantIdClaim, tenantId.ToString()), new Claim(EntraClaims.ObjectIdClaim, objectId.ToString()) }, "Tests"));

        private static long NewUser(IDatabase database, Guid objectId)
        {
            using var transaction = database.CreateTransaction();
            var person = new PersonBuilder(transaction).Build();
            person.EntraTenantId = Tenant;
            person.EntraObjectId = objectId;
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

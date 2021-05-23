// <copyright file="DefaultDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Configuration
{
    using Bogus;
    using Database;
    using Domain;
    using Domain.Derivations.Default;
    using Microsoft.AspNetCore.Http;

    public class TestsDatabaseContext : DefaultDatabaseContext
    {
        public TestsDatabaseContext(Engine engine, IHttpContextAccessor httpContextAccessor = null) : base(engine, httpContextAccessor) { }

        public override void OnInit(IDatabase database)
        {
            this.PasswordHasher = new TestPasswordHasher();

            base.OnInit(database);

            this.Faker = new Faker();
        }

        public Faker Faker { get; set; }

        private class TestPasswordHasher : IPasswordHasher
        {
            public string HashPassword(string user, string password) => password;

            public bool VerifyHashedPassword(string user, string hashedPassword, string providedPassword) => hashedPassword == providedPassword;
        }
    }
}

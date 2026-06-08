// <copyright file="DefaultDatabaseScope.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Configuration
{
    using Database.Derivations;
    using Derivations.Default;
    using Domain;
    using Microsoft.Extensions.Configuration;

    public class TestDatabaseServices : DatabaseServices
    {
        public TestDatabaseServices(Engine engine, IConfiguration configuration = null) : base(engine, configuration) { }

        protected override IPasswordHasher CreatePasswordHasher() => new TestPasswordHasher();

        protected override IMediaContentFactory CreateMediaContentFactory() =>
            new MediaContentFactory(transaction => new EmbeddedMediaContentBuilder(transaction).Build());

        protected override IDerivationService CreateDerivationFactory() => new DerivationService(this.Engine);

        private class TestPasswordHasher : IPasswordHasher
        {
            public string HashPassword(string user, string password) => password;

            public bool VerifyHashedPassword(string user, string hashedPassword, string providedPassword) => hashedPassword == providedPassword;

            public bool CheckStrength(string password) => true;
        }
    }
}

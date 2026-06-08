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

    public class DefaultDatabaseServices : DatabaseServices
    {
        public DefaultDatabaseServices(Engine engine, IConfiguration configuration = null) : base(engine, configuration) { }

        protected override IPasswordHasher CreatePasswordHasher() => new PasswordHasher();

        protected override IMediaContentFactory CreateMediaContentFactory() =>
            new MediaContentFactory(transaction => new EmbeddedMediaContentBuilder(transaction).Build());

        protected override IDerivationService CreateDerivationFactory() => new DerivationService(this.Engine);
    }
}

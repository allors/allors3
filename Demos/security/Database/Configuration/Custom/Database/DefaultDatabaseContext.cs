// <copyright file="DefaultDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Configuration
{
    using Domain;
    using Domain.Derivations.Default;
    using Microsoft.AspNetCore.Http;
    

    public class DefaultDatabaseContext : DatabaseContext
    {
        public DefaultDatabaseContext(Engine engine, IHttpContextAccessor httpContextAccessor = null) : base(httpContextAccessor) => this.Engine = engine;

        public Engine Engine { get; }

        public override void OnInit(IDatabase database)
        {
            base.OnInit(database);
            this.DerivationFactory = new DerivationFactory(this.Engine);
        }
    }
}

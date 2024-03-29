// <copyright file="DefaultDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

using Allors.Database.Configuration.Derivations.Default;

namespace Allors.Database.Configuration
{
    using Microsoft.AspNetCore.Http;
    using Domain;
    using Domain.Derivations.Rules;

    public class DefaultDatabaseServices : DatabaseServices
    {
        public DefaultDatabaseServices(IHttpContextAccessor httpContextAccessor = null) : base(httpContextAccessor) { }

        public override void OnInit(IDatabase database)
        {
            base.OnInit(database);

            var m = this.M;

            var rules = new Rule[]
            {
                // Core
                new MediaRule(m),
                new TransitionalDeniedPermissionRule(m),

                // Custom
                new ScoreboardRule(m),
                new GameRule(m),
                new StartEndDateRule(m),
                new GameDefenderRule(m),
                new ScoreRule(m),
                new AccumulatedScoreRule(m),
            };

            this.Engine = new Engine(rules);
        }
    }
}

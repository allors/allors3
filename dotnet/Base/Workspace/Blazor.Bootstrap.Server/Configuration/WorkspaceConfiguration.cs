// <copyright file="ServiceCollectionExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Workspace.Configuration
{
    using Adapters;
    using Allors.Ranges;
    using Derivations;
    using Domain;
    using Meta.Lazy;
    using Microsoft.AspNetCore.Components.Authorization;
    using Services;

    public static class WorkspaceConfiguration
    {
        public static void AddAllorsWorkspace(this IServiceCollection services)
        {
            var rangesFactory = () => new DefaultStructRanges<long>();
            var servicesBuilder = () => new WorkspaceServices();

            var metaPopulation = new MetaBuilder().Build();
            var objectFactory = new ReflectionObjectFactory(metaPopulation, typeof(Person));
            var rules = new IRule[] { };
            var configuration = new Adapters.Local.Configuration("Default", metaPopulation, objectFactory, rules);

            services.AddScoped<DatabaseConnection>(serviceProvider =>
            {
                var database = serviceProvider.GetRequiredService<IDatabaseService>().Database;
                var authenticationState = serviceProvider.GetRequiredService<AuthenticationStateProvider>();
                var user = authenticationState.GetAuthenticationStateAsync().Result.User;
                var id = user.
                return new Adapters.Local.DatabaseConnection(configuration, database, servicesBuilder, rangesFactory);
            });
        }
    }
}

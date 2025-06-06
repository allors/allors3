// <copyright file="ServiceCollectionExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Workspace.Configuration
{
    using System.Globalization;
    using System.Security.Claims;
    using Adapters;
    using Allors.Ranges;
    using Derivations;
    using Domain;
    using Meta.Lazy;
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
                var claimsPrincipalService = serviceProvider.GetRequiredService<IClaimsPrincipalService>();
                var user = claimsPrincipalService.User;
                var claim = user.FindFirst(ClaimTypes.NameIdentifier);
                var userId = claim != null ? int.Parse(claim.Value, CultureInfo.InvariantCulture) : 0;
                return new Adapters.Local.DatabaseConnection(configuration, database, servicesBuilder, rangesFactory) { UserId = userId };
            });

            services.AddScoped(serviceProvider =>
            {
                var databaseConnection = serviceProvider.GetRequiredService<DatabaseConnection>();
                return databaseConnection.CreateWorkspace();
            });
        }
    }
}

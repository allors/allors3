// <copyright file="ServiceCollectionExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Workspace.Configuration
{
    using Database;
    using Database.Configuration;
    using Database.Configuration.Derivations.Default;
    using Database.Domain;
    using Database.Meta;
    using Services;

    public static class DatabaseConfiguration
    {
        public static void AddAllorsDatabase(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            var metaPopulation = new MetaBuilder().Build();
            var engine = new Engine(Rules.Create(metaPopulation));
            var objectFactory = new ObjectFactory(metaPopulation, typeof(Database.Domain.User));
            var databaseScope = new DefaultDatabaseServices(engine);
            var databaseBuilder = new Database.Adapters.DatabaseBuilder(databaseScope, configuration, objectFactory);
            var database = databaseBuilder.Build();

            services.AddSingleton<IDatabaseService>(new DatabaseService { Build = databaseBuilder.Build, Database = database});
            services.AddScoped<ITransactionService, TransactionService>();
        }
    }
}

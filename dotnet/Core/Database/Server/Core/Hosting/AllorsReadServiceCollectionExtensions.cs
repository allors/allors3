// <copyright file="AllorsReadServiceCollectionExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using Allors.Database.Protocol.Json;
    using Allors.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class AllorsReadServiceCollectionExtensions
    {
        /// <summary>
        /// Enables complete reads using immutable deployment-owned revisions and inline declarations.
        /// The host remains responsible for completing authentication/admission before read routing.
        /// </summary>
        public static IServiceCollection AddAllorsRead(this IServiceCollection services, ReadConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            services.AddSingleton(configuration);
            services.AddScoped(provider => new ReadExecutor(provider.GetRequiredService<IDatabaseService>().Database, configuration, logger: provider.GetService<ILogger<ReadExecutor>>()));
            services.AddScoped(provider => new PushReadExecutor(provider.GetRequiredService<IDatabaseService>().Database, configuration, logger: provider.GetService<ILogger<PushReadExecutor>>()));
            services.AddScoped(provider => new InvokeReadExecutor(provider.GetRequiredService<IDatabaseService>().Database, configuration, logger: provider.GetService<ILogger<InvokeReadExecutor>>()));
            return services;
        }
    }
}

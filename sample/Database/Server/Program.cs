// <copyright file="Program.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Server
{
    using System;
    using System.Collections.Generic;
    using Allors.Database.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using NLog;
    using NLog.Web;

    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                logger.Error(e, "The server stopped because of an exception");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        // The folder custom of the configuration root holds the settings of the sample's domain, and its
        // folder server those of this server. Environment variables override both.
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder => builder
                    .UseStartup<Startup>()
                    .ConfigureAppConfiguration((context, configurationBuilder) =>
                    {
                        // Replace the default sources, the settings of the command line included. The host has
                        // read its environment and its web host options at this point; the variables with the
                        // prefixes DOTNET_ and ASPNETCORE_ keep the options that ASP.NET Core reads later, such
                        // as ASPNETCORE_URLS.
                        configurationBuilder.Sources.Clear();
                        configurationBuilder.AddEnvironmentVariables("DOTNET_");
                        configurationBuilder.AddEnvironmentVariables("ASPNETCORE_");
                        configurationBuilder.AddAllorsConfiguration("custom", "server", context.HostingEnvironment.EnvironmentName);

                        // Keep the URLs of --urls, without the other settings of the command line.
                        var urls = new ConfigurationBuilder().AddCommandLine(args).Build()[WebHostDefaults.ServerUrlsKey];
                        if (urls != null)
                        {
                            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
                            {
                                [WebHostDefaults.ServerUrlsKey] = urls,
                            });
                        }
                    })
                    .UseNLog());
    }
}

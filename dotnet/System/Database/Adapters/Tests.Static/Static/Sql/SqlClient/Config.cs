// <copyright file="Config.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System.IO;
    using System.Runtime.InteropServices;
    using Microsoft.Extensions.Configuration;

    public static class Config
    {
        private const string ConfigPath = "/opt/sqlclient";

        private static readonly IConfigurationRoot Configuration;

        static Config()
        {
            var platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" :
                           RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
                           RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "other";

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile(Path.Combine(ConfigPath, "appSettings.json"), true);
            configurationBuilder.AddJsonFile(Path.Combine(ConfigPath, $"appSettings.{platform}.json"), true);
            configurationBuilder.AddEnvironmentVariables();
            Configuration = configurationBuilder.Build();
        }

        public static string ConnectionString => Configuration["ConnectionStrings:DefaultConnection"]
            ?? @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true";
    }
}

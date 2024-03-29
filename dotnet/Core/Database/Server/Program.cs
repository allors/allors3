namespace Allors.Server
{
    using System;
    using System.IO;
    using Database.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using NLog;
    using NLog.Web;

    public class Program
    {
        private const string ConfigPath = "/config/core";

        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder => builder
                    .UseStartup<Startup>()
                    .ConfigureAppConfiguration((hostingContext, configurationBuilder) =>
                    {
                        var environmentName = hostingContext.HostingEnvironment.EnvironmentName;
                        configurationBuilder.AddCrossPlatform(".", environmentName, true);
                        configurationBuilder.AddCrossPlatform(ConfigPath, environmentName);
                        configurationBuilder.AddCrossPlatform(Path.Combine(ConfigPath, hostingContext.HostingEnvironment.ApplicationName), environmentName);
                    })
                    .UseNLog());
    }
}

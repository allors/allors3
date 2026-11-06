namespace Allors.Server
{
    using System;
    using System.Linq;
    using Database.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using NLog;
    using NLog.Web;

    public class Program
    {
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
                        // The settings come from the configuration root and from the environment only: the
                        // default builder's appsettings files in the folder of the server and its user
                        // secrets are not read.
                        foreach (var source in configurationBuilder.Sources.OfType<FileConfigurationSource>().ToArray())
                        {
                            configurationBuilder.Sources.Remove(source);
                        }

                        var environmentName = hostingContext.HostingEnvironment.EnvironmentName;
                        configurationBuilder.AddAllorsConfiguration("core", hostingContext.HostingEnvironment.ApplicationName, environmentName);
                    })
                    .UseNLog());
    }
}

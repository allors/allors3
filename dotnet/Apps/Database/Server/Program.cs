namespace Allors.Database.Server.Controllers
{
    using System;
    using Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
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
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder => builder
                    .UseStartup<Startup>()
                    .ConfigureAppConfiguration((hostingContext, configurationBuilder) =>
                    {
                        var environmentName = hostingContext.HostingEnvironment.EnvironmentName;
                        configurationBuilder.AddAllorsConfiguration("apps", hostingContext.HostingEnvironment.ApplicationName, environmentName);
                    })
                    .UseNLog());
    }
}

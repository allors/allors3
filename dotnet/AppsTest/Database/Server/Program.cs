namespace Allors.Database.Server.Controllers
{
    using System.IO;
    using Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    public class Program
    {
        private const string ConfigPath = "/opt/apps";

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
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
                    }));
    }
}

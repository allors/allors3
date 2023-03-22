namespace Workspace.ViewModels.WinForms
{
    using Controllers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Services;
    using ViewModels.Services;

    internal static partial class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IMessageService, MessageService>();
                    services.AddTransient<MainFormController>();
                    services.AddTransient<MainForm>();
                });
        }
    }
}

namespace Workspace.ViewModels.WinForms
{
    using Microsoft.Extensions.DependencyInjection;

    internal static partial class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var host = CreateHostBuilder().Build();
            ServiceProvider = host.Services;

            Application.Run(ServiceProvider.GetRequiredService<MainForm>());
        }
    }
}

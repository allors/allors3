namespace Workspace.ViewModels.WinForms
{
    using Allors.Ranges;
    using Allors.Workspace.Adapters;
    using Allors.Workspace;
    using Allors.Workspace.Derivations;
    using Allors.Workspace.Meta.Lazy;
    using Features;
    using Forms;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Services;
    using ViewModels.Services;
    using Configuration = Allors.Workspace.Adapters.Remote.Configuration;
    using DatabaseConnection = Allors.Workspace.Adapters.Remote.SystemText.DatabaseConnection;

    internal static partial class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        static IHostBuilder CreateHostBuilder()
        {
            var httpClient = new HttpClient
            {
                // TODO: Login form
                //BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
                BaseAddress = new Uri("http://localhost:5000/allors/")
            };

            var idGenerator = new IdGenerator();
            DefaultRanges<long> defaultRanges = new DefaultStructRanges<long>();
            var metaPopulation = new MetaBuilder().Build();
            var objectFactory = new ReflectionObjectFactory(metaPopulation, typeof(Allors.Workspace.Domain.Person));
            var rules = new IRule[]
            {
                //new PersonSessionFullNameRule(metaPopulation)
            };
            var configuration = new Configuration("Default", metaPopulation, objectFactory, rules);

            var databaseConnection = new DatabaseConnection(configuration, () => new WorkspaceServices(), httpClient, idGenerator, defaultRanges);

            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Allors
                    services.AddSingleton(databaseConnection);
                    services.AddSingleton<IDatabaseService, DatabaseService>();
                    services.AddSingleton(v => v.GetService<IDatabaseService>().CreateWorkspace());
                    services.AddScoped(v => v.GetService<IWorkspace>().CreateSession());

                    // Services
                    services.AddSingleton<IMdiService, MdiService>();
                    services.AddSingleton<IMessageService, MessageService>();

                    // Controllers
                    services.AddScoped<MainFormViewModel>();
                    services.AddScoped<PersonFormViewModel>();

                    // Forms
                    services.AddSingleton<MainForm>();
                    services.AddTransient<PersonForm>();
                });
        }
    }
}

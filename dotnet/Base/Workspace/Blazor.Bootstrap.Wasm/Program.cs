using Allors.Ranges;
using Allors.Workspace;
using Allors.Workspace.Adapters;
using Allors.Workspace.Blazor;
using Allors.Workspace.Derivations;
using Allors.Workspace.Meta.Lazy;
using Blazor.Bootstrap.Wasm;
using BlazorStrap;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Configuration = Allors.Workspace.Adapters.Remote.Configuration;
using DatabaseConnection = Allors.Workspace.Adapters.Remote.SystemText.DatabaseConnection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var httpClient = new HttpClient
{
    //BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    BaseAddress = new Uri("http://localhost:5000/allors/")
};

Func<IWorkspaceServices> servicesBuilder = () => new WorkspaceServices();
var idGenerator = new IdGenerator();
DefaultRanges<long> defaultRanges = new DefaultStructRanges<long>();
var metaPopulation = new MetaBuilder().Build();
var objectFactory = new ReflectionObjectFactory(metaPopulation, typeof(Allors.Workspace.Domain.Person));
var rules = new IRule[]
{
    //new PersonSessionFullNameRule(metaPopulation)
};
var configuration = new Configuration("Default", metaPopulation, objectFactory, rules);

var databaseConnection = new DatabaseConnection(configuration, servicesBuilder, httpClient, idGenerator, defaultRanges);
builder.Services.AddSingleton(databaseConnection);

builder.Services.AddScoped(_ => httpClient);
builder.Services.AddScoped(_ => databaseConnection.CreateWorkspace());

builder.Services.AddSingleton(new AllorsAuthenticationStateProviderConfig
{
    AuthenticationUrl = "TestAuthentication/Token",
});
builder.Services.AddScoped<AllorsAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<AllorsAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

builder.Services.AddBlazorStrap();

await builder.Build().RunAsync();

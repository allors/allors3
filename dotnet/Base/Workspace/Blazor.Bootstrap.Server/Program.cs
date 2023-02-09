using Allors.Ranges;
using Allors.Workspace;
using Allors.Workspace.Adapters;
using Allors.Workspace.Domain;
using Allors.Workspace.Meta.Lazy;
using Allors.Workspace.Derivations;
using BlazorStrap;

// Allors Database
var databaseMeta = new Allors.Database.Meta.MetaBuilder().Build();
var engine = new Allors.Database.Configuration.Derivations.Default.Engine(Allors.Database.Domain.Rules.Create(databaseMeta));
var database = new Allors.Database.Adapters.Sql.SqlClient.Database(
    new Allors.Database.Configuration.DefaultDatabaseServices(engine),
    new Allors.Database.Adapters.Sql.Configuration
    {
        ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Base;Integrated Security=true;Pooling=false",
        ObjectFactory = new Allors.Database.ObjectFactory(databaseMeta, typeof(Allors.Database.Domain.Person)),
    });

// Allors Workspace
var metaPopulation = new MetaBuilder().Build();
var objectFactory = new ReflectionObjectFactory(metaPopulation, typeof(Person));
var rules = new IRule[] { };
var configuration = new Allors.Workspace.Adapters.Local.Configuration("Default", metaPopulation, objectFactory, rules);

var servicesBuilder = () => new WorkspaceServices();
var rangesFactory = () => new DefaultStructRanges<long>();
var databaseConnection = new Allors.Workspace.Adapters.Local.DatabaseConnection(configuration, database, servicesBuilder, rangesFactory);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddBlazorStrap();

builder.Services.AddSingleton<DatabaseConnection>(databaseConnection);
builder.Services.AddSingleton(databaseConnection.CreateWorkspace());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

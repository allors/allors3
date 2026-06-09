using System.IO;
using Nuke.Common;

partial class Build
{
    [Parameter("Database provider templates to install: 'sqlclient' or 'npgsql' (default 'sqlclient')")]
    private readonly string Provider = "sqlclient";

    [Parameter("Target directory for InstallConfig (default '/opt/allors')")]
    private readonly string ConfigRoot = "/opt/allors";

    protected override void OnBuildInitialized()
    {
        base.OnBuildInitialized();

        // The database test targets run the real Server/Commands, which require ALLORS_CONFIG_ROOT.
        // Default it to the in-repo templates for the selected provider; an externally-set value is respected.
        if (string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("ALLORS_CONFIG_ROOT")))
        {
            System.Environment.SetEnvironmentVariable("ALLORS_CONFIG_ROOT", (Paths.Config / Provider).ToString());
        }

        // Make the selected adapter explicit for the Server/Commands child processes (overrides the JSON).
        System.Environment.SetEnvironmentVariable("adapter", Provider);
    }

    // Points the Server/Commands child processes at a connection string for <paramref name="database"/>,
    // derived from the admin connection in ALLORS_NPGSQL / ALLORS_SQLCLIENT. Allors configuration layers
    // environment variables last, so ConnectionStrings__DefaultConnection wins over the JSON template.
    // The database itself is (re)created by 'Commands.dll Init', which reads the same admin connection.
    private void SetDatabaseEnvironment(string database)
    {
        System.Environment.SetEnvironmentVariable("adapter", Provider);
        System.Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", DeriveConnectionString(database));
    }

    private string DeriveConnectionString(string database) =>
        Provider.Trim().ToLowerInvariant() switch
        {
            "npgsql" => new Npgsql.NpgsqlConnectionStringBuilder(RequireAdmin("ALLORS_NPGSQL"))
            {
                Database = database.ToLowerInvariant(),
                Pooling = false,
                Enlist = false,
                // Force a finite command timeout: the admin connection often carries 'Command Timeout=0'
                // (infinite), which would turn a blocked command into a permanent server hang.
                CommandTimeout = 300,
            }.ConnectionString,
            "sqlclient" => new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(RequireAdmin("ALLORS_SQLCLIENT"))
            {
                InitialCatalog = database,
            }.ConnectionString,
            _ => throw new System.ArgumentOutOfRangeException(nameof(Provider), Provider, "Expected 'npgsql' or 'sqlclient'."),
        };

    private static string RequireAdmin(string variable)
    {
        var value = System.Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrWhiteSpace(value))
        {
            Assert.Fail($"{variable} is not set. Export an admin connection string that can create databases, e.g. export {variable}=\"...\"");
        }

        return value;
    }

    // Copies the chosen provider's templates (config/<provider>/<domain>/...) into the config root,
    // e.g. ./build.sh InstallConfig --provider npgsql --config-root /opt/allors
    private Target InstallConfig => _ => _
        .Executes(() =>
        {
            var source = (Paths.Config / Provider).ToString();
            if (!Directory.Exists(source))
            {
                Assert.Fail($"Config provider '{Provider}' not found at '{source}'. Expected 'sqlclient' or 'npgsql'.");
            }

            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(source, file);
                var destination = Path.Combine(ConfigRoot, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(file, destination, true);
                Serilog.Log.Information("Installed {Destination}", destination);
            }
        });
}

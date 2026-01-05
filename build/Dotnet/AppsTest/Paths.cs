using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetAppsTest => Dotnet / "AppsTest";
    public AbsolutePath DotnetAppsTestRepositoryDomainRepository => DotnetAppsTest / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetAppsTestDatabase => DotnetAppsTest / "Database";
    public AbsolutePath DotnetAppsTestDatabaseMetaGenerated => DotnetAppsTestDatabase / "Meta/Generated";
    public AbsolutePath DotnetAppsTestDatabaseGenerate => DotnetAppsTestDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetAppsTestDatabaseServer => DotnetAppsTestDatabase / "Server";
    public AbsolutePath DotnetAppsTestDatabaseDomainTests => DotnetAppsTestDatabase / "Domain.Tests/Domain.Tests.csproj";
    public AbsolutePath DotnetAppsTestDatabaseResources => DotnetAppsTestDatabase / "Resources";
    public AbsolutePath DotnetAppsTestDatabaseResourcesApps => DotnetAppsTestDatabaseResources / "Apps";

    public AbsolutePath DotnetAppsTestWorkspaceTypescript => DotnetAppsTest / "Workspace/Typescript";
}

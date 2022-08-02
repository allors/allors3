using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetApps => Dotnet / "Apps";
    public AbsolutePath DotnetAppsRepositoryDomainRepository => DotnetApps / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetAppsDatabase => DotnetApps / "Database";
    public AbsolutePath DotnetAppsDatabaseMetaGenerated => DotnetAppsDatabase / "Meta/Generated";
    public AbsolutePath DotnetAppsDatabaseGenerate => DotnetAppsDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetAppsDatabaseCommands => DotnetAppsDatabase / "Commands";
    public AbsolutePath DotnetAppsDatabaseServer => DotnetAppsDatabase / "Server";
    public AbsolutePath DotnetAppsDatabaseDomainTests => DotnetAppsDatabase / "Domain.Tests/Domain.Tests.csproj";
    public AbsolutePath DotnetAppsDatabaseResources => DotnetAppsDatabase / "Resources";
    public AbsolutePath DotnetAppsDatabaseResourcesApps => DotnetAppsDatabaseResources / "Apps";

    public AbsolutePath DotnetAppsWorkspaceTypescript => DotnetApps / "Workspace/Typescript";
}

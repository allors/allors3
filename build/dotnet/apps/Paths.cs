using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetApps => this.Dotnet / "apps";
    public AbsolutePath DotnetAppsRepositoryDomainRepository => this.DotnetApps / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetAppsDatabase => this.DotnetApps / "Database";
    public AbsolutePath DotnetAppsDatabaseMetaGenerated => this.DotnetAppsDatabase / "Meta/generated";
    public AbsolutePath DotnetAppsDatabaseGenerate => this.DotnetAppsDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetAppsDatabaseCommands => this.DotnetAppsDatabase / "Commands";
    public AbsolutePath DotnetAppsDatabaseServer => this.DotnetAppsDatabase / "Server";
    public AbsolutePath DotnetAppsDatabaseDomainTests => this.DotnetAppsDatabase / "Domain.Tests/Domain.Tests.csproj";
    public AbsolutePath DotnetAppsDatabaseResources => this.DotnetAppsDatabase / "Resources";
    public AbsolutePath DotnetAppsDatabaseResourcesApps => this.DotnetAppsDatabaseResources / "Apps";

    public AbsolutePath DotnetAppsWorkspaceTypescript => this.DotnetApps / "Workspace/Typescript";
}

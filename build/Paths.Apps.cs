using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Apps => this.Root / "apps";
    public AbsolutePath AppsRepositoryDomainRepository => this.Apps / "Repository/Domain/Repository.csproj";

    public AbsolutePath AppsDatabase => this.Apps / "Database";
    public AbsolutePath AppsDatabaseMetaGenerated => this.AppsDatabase / "Meta/generated";
    public AbsolutePath AppsDatabaseGenerate => this.AppsDatabase / "Generate/Generate.csproj";
    public AbsolutePath AppsDatabaseMerge => this.AppsDatabase / "Merge/Merge.csproj";
    public AbsolutePath AppsDatabaseCommands => this.AppsDatabase / "Commands";
    public AbsolutePath AppsDatabaseServer => this.AppsDatabase / "Server";
    public AbsolutePath AppsDatabaseDomainTests => this.AppsDatabase / "Domain.Tests/Domain.Tests.csproj";
    public AbsolutePath AppsDatabaseResources => this.AppsDatabase / "Resources";
    public AbsolutePath AppsDatabaseResourcesApps => this.AppsDatabaseResources / "Apps";

    public AbsolutePath AppsWorkspaceTypescript => this.Apps / "Workspace/Typescript";
}

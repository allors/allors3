using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetBase => this.Dotnet / "base";
    public AbsolutePath DotnetBaseRepositoryDomainRepository => this.DotnetBase / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetBaseDatabase => this.DotnetBase / "Database";
    public AbsolutePath DotnetBaseDatabaseMetaGenerated => this.DotnetBaseDatabase / "Meta/generated";
    public AbsolutePath DotnetBaseDatabaseGenerate => this.DotnetBaseDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetBaseDatabaseCommands => this.DotnetBaseDatabase / "Commands";
    public AbsolutePath DotnetBaseDatabaseServer => this.DotnetBaseDatabase / "Server";
    public AbsolutePath DotnetBaseDatabaseDomainTests => this.DotnetBaseDatabase / "Domain.Tests/Domain.Tests.csproj";
    public AbsolutePath DotnetBaseDatabaseResources => this.DotnetBaseDatabase / "Resources";
    public AbsolutePath DotnetBaseDatabaseResourcesBase => this.DotnetBaseDatabaseResources / "Base";

    public AbsolutePath DotnetBaseWorkspaceTypescript => this.DotnetBase / "Workspace/Typescript";
}

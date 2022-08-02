using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetBase => Dotnet / "Base";
    public AbsolutePath DotnetBaseRepositoryDomainRepository => DotnetBase / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetBaseDatabase => DotnetBase / "Database";
    public AbsolutePath DotnetBaseDatabaseMetaGenerated => DotnetBaseDatabase / "Meta/Generated";
    public AbsolutePath DotnetBaseDatabaseGenerate => DotnetBaseDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetBaseDatabaseCommands => DotnetBaseDatabase / "Commands";
    public AbsolutePath DotnetBaseDatabaseServer => DotnetBaseDatabase / "Server";
    public AbsolutePath DotnetBaseDatabaseDomainTests => DotnetBaseDatabase / "Domain.Tests/Domain.Tests.csproj";
    public AbsolutePath DotnetBaseDatabaseResources => DotnetBaseDatabase / "Resources";
    public AbsolutePath DotnetBaseDatabaseResourcesBase => DotnetBaseDatabaseResources / "Base";

    public AbsolutePath DotnetBaseWorkspaceTypescript => DotnetBase / "Workspace/Typescript";
}
